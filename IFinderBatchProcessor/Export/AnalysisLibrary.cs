using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFinderBatchProcessor.Export
{
    using System.Data.SQLite;
    using System.IO;
    using System.Runtime.CompilerServices;

    using IFinderBatchProcessor.Util;

    using ImsInformed.Targets;
    using ImsInformed.Workflows.CrossSectionExtraction;

    // A Sqlite library as a sqlite database connection
    public class AnalysisLibrary 
    {
        private readonly SQLiteCommand Insert;
        private readonly AsyncLock dbLock;
        private readonly string dbPath;
        private SQLiteConnection dbConnection;

        private readonly string createDatasetTableCommand = ("create table datasets (" + 
                "id integer primary key," + 
                "name text," + 
                "path text," + 
                "date text" + 
                ");");

        private readonly string createAnalysisTableCommand = ("create table analyses (" + 
                "id integer primary key," + 
                "analyzed_dataset integer," + 
                "analyzed_target integer," + 
                "analysis_status text," + 
                "average_intensity_score real," + 
                "average_peak_shape_score real," + 
                "average_isotopic_score real," + 
                "environment_stability_score," + 
                "data_likelihood real," + 
                "a_posteriori_probability real," + 
                "qc_plot blob," + 
                "foreign key(analyzed_dataset) references datasets(id)," + 
                "foreign key(analyzed_target) references targets(id)" + 
                ");");

        private readonly string createDetectionTableCommand = ("create table identifications (" + 
                "id integer primary key," + 
                "detection_analysis integer," +  
                
                "measured_mz_in_dalton real," + 
                "ppm_error integer," + 
                "viper_compatible_mz real," + 
                "r2 real," + 
                "analysis_status text," + 
                "intensity_score real," + 
                "peak_shape_score real," + 
                "isotopic_score real," + 
                "collision_cross_section real," +
                "mobility real," + 
                "t0 real," + 
                "foreign key(detection_analysis) references analyses(id)" + 
                ");");

        private readonly string createSnapshotTableCommand = ("create table peaks (" + 
                "id integer primary key," + 
                "ion integer," +  
                "arrival_time_in_ms real," + 
                "temperature real," + 
                "pressure real," + 
                "drift_tube_voltage real," + 
                "peak_profile blob," + 
                "foreign key(ion) references identifications(id)" +
                ");");

        private readonly string createChemicalsTableCommand = "create table chemicals (" + 
                "name text primary key," + 
                "empirical_formula text," + 
                "chemical_class text" + 
                ");";

        private readonly string createTargetTableCommand = "create table targets (" + 
                "id integer primary key," + 
                "target_chemical text," +  
                "target_type text," + 
                "adduct text," + 
                "target_description text," + 
                "monoisotopic_mass real," + 
                "composition_with_adduct text," + 
                "mass_with_adduct real," + 
                "charge_state integer," + 
                "foreign key(target_chemical) references chemicals(name)" + 
                ");";

        public AnalysisLibrary(string path)
        {
            this.dbLock = new AsyncLock();
            this.dbPath = path;

            string dir = Path.GetDirectoryName(path);

            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(this.dbPath))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception)
                {
                    throw new FieldAccessException(string.Format("Try to overwrite but cannot delete existing library at {0}", path));
                }
            }

            string connectionString = "Data Source = " + path + "; Version=3; DateTimeFormat=Ticks;";
            SQLiteConnection.CreateFile(path);
            this.dbConnection = new SQLiteConnection(connectionString, true);
        }

        /// <summary>
        /// Insert a single result
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public async Task InsertResult(CrossSectionWorkflowResult result)
        {
            using (await this.dbLock.LockAsync())
            {
                this.dbConnection.Open();
                
                using (SQLiteCommand cmd = new SQLiteCommand(this.dbConnection))
                {
                    await this.InsertResult(cmd, result);
                }
                
                this.dbConnection.Close();
            }
        }

        /// <summary>
        /// Insert multiple results
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public async Task InsertResult(IEnumerable<CrossSectionWorkflowResult> results)
        {
            using (await this.dbLock.LockAsync())
            {
                this.dbConnection.Open();
                
                using (SQLiteCommand cmd = new SQLiteCommand(this.dbConnection))
                {
                    foreach (var result in results)
                    {
                        await this.InsertResult(cmd, result);
                    }
                }
                
                this.dbConnection.Close();
            }
        }

        public async Task CreateTables()
        {
            using (await this.dbLock.LockAsync())
            {
                this.dbConnection.Open();
                
                using (SQLiteCommand cmd = new SQLiteCommand(this.dbConnection))
                {
                    // Create Tables In the Analysis Library
                    cmd.CommandText = this.createChemicalsTableCommand;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = this.createDatasetTableCommand;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = this.createTargetTableCommand;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = this.createAnalysisTableCommand;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = this.createDetectionTableCommand;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = this.createSnapshotTableCommand;
                    cmd.ExecuteNonQuery();
                }
                
                this.dbConnection.Close();
            }
        }

        private async Task InsertResult(SQLiteCommand cmd, CrossSectionWorkflowResult result)
        {
            // Insert chemical info to chemicals table
            await this.InsertChemical(cmd, result.Target);
            
            // Insert dataset info to datasets table
            long datasetId = await this.InsertDataset(cmd, result);

            // Insert target info th targets table
            long targetId = await this.InsertTarget(cmd, result.Target);

            // Insert analysis info the analyses table
            long analysisID = await this.InsertAnalysis(cmd, result, targetId, datasetId);

            // Insert identification info to identifications table
            foreach (var detection in result.IdentifiedIsomers)
            {
                long detectionId = await this.InsertIdentifications(cmd, detection, analysisID);
                foreach (var snapshot in detection.ArrivalTimeSnapShots)
                {
                    // Insert snapshot info to peaks table
                    await this.InsertSnapshots(cmd, snapshot, detectionId);
                }
            }
        }

        private async Task<string> InsertChemical(SQLiteCommand cmd, IImsTarget target)
        {
            // Query if chemical is already added.
            string chemicalName = target.SampleClass;

            cmd.CommandText = string.Format("SELECT count(*) FROM chemicals WHERE name='{0}'", chemicalName); 
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if(count == 0)
            {
                // Insert chemical info to chemicals table
                cmd.CommandText = "INSERT INTO chemicals (name, empirical_formula, chemical_class) VALUES      (@name, @empirical_formula, @chemical_class)";

                
                cmd.Parameters.AddWithValue("@name", chemicalName);
                cmd.Parameters.AddWithValue("@empirical_formula", target.EmpiricalFormula);
                cmd.Parameters.AddWithValue("@chemical_class", "");
                await Task.Run(() => cmd.ExecuteNonQuery());

                return chemicalName;
            }

            return "";
        }

        private async Task<long> InsertDataset(SQLiteCommand cmd, CrossSectionWorkflowResult result)
        {
            cmd.CommandText = string.Format("SELECT count(*) FROM datasets WHERE name='{0}'", result.DatasetName);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if(count == 0)
            {
                // Insert dataset info to chemicals table
                cmd.CommandText = "INSERT INTO datasets (name, path, date) VALUES      (@name, @path, @date)";
                cmd.Parameters.AddWithValue("@name", result.DatasetName);
                cmd.Parameters.AddWithValue("@path", "");
                cmd.Parameters.AddWithValue("@date", "");
                await Task.Run(() => cmd.ExecuteNonQuery());
                return await this.LastID(cmd);
            }
            else
            {
                cmd.CommandText = string.Format("SELECT id FROM datasets WHERE name='{0}'", result.DatasetName);
                var id =  cmd.ExecuteScalar();
                return (long)id;
            }
            
            // Return the datasetID
        }

        private async Task<long> InsertTarget(SQLiteCommand cmd, IImsTarget target)
        {
            cmd.CommandText = string.Format("SELECT count(*) FROM targets WHERE target_description ='{0}'", target.TargetDescriptor);
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            if(count == 0)
            {
                // Insert target info to chemicals table
                cmd.CommandText = "INSERT INTO targets (target_chemical, target_type, adduct, target_description, monoisotopic_mass, composition_with_adduct, mass_with_adduct,     charge_state) VALUES      (@target_chemical, @target_type, @adduct, @target_description, @monoisotopic_mass, @composition_with_adduct, @mass_with_adduct,   @charge_state)";
                cmd.Parameters.AddWithValue("@target_chemical", target.SampleClass);
                cmd.Parameters.AddWithValue("@target_type", target.TargetType);
                cmd.Parameters.AddWithValue("@adduct", target.Adduct);
                cmd.Parameters.AddWithValue("@target_description", target.TargetDescriptor);
                cmd.Parameters.AddWithValue("@monoisotopic_mass", target.MonoisotopicMass);
                cmd.Parameters.AddWithValue("@composition_with_adduct", target.CompositionWithAdduct);
                cmd.Parameters.AddWithValue("@mass_with_adduct", target.MassWithAdduct);
                cmd.Parameters.AddWithValue("@charge_state", target.ChargeState);
                await Task.Run(() => cmd.ExecuteNonQuery());
                return await this.LastID(cmd);
            }
            else
            {
                cmd.CommandText = string.Format("SELECT rowid FROM targets WHERE target_description ='{0}'", target.TargetDescriptor);
                int id =  Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            }
        }

        private async Task<long> InsertAnalysis(SQLiteCommand cmd, CrossSectionWorkflowResult result, long targetID, long datasetID)
        {
            // Insert analysis info to chemicals table
            cmd.CommandText = "INSERT INTO analyses (analyzed_dataset , analyzed_target, analysis_status, average_intensity_score, average_peak_shape_score, average_isotopic_score, environment_stability_score, data_likelihood, a_posteriori_probability, qc_plot) VALUES      (@analyzed_dataset , @analyzed_target, @analysis_status, @average_intensity_score, @average_peak_shape_score, @average_isotopic_score, @environment_stability_score, @data_likelihood, @a_posteriori_probability, @qc_plot)";
            cmd.Parameters.AddWithValue("@analyzed_dataset", datasetID);
            cmd.Parameters.AddWithValue("@analyzed_target", targetID);
            cmd.Parameters.AddWithValue("@analysis_status", result.AnalysisStatus.ToString());
            cmd.Parameters.AddWithValue("@average_intensity_score", result.AssociationHypothesisInfo != null ? 
                result.AverageObservedPeakStatistics.IntensityScore : 0);
            cmd.Parameters.AddWithValue("@average_peak_shape_score", result.AssociationHypothesisInfo != null ?
                result.AverageObservedPeakStatistics.PeakShapeScore : 0);
            cmd.Parameters.AddWithValue("@average_isotopic_score", result.AssociationHypothesisInfo != null ?
                result.AverageObservedPeakStatistics.IsotopicScore : 0);
            cmd.Parameters.AddWithValue("@environment_stability_score", result.AverageVoltageGroupStability);
            cmd.Parameters.AddWithValue("@data_likelihood", result.AssociationHypothesisInfo != null ? 
                result.AssociationHypothesisInfo.ProbabilityOfDataGivenHypothesis : 0);
            cmd.Parameters.AddWithValue("@a_posteriori_probability", result.AssociationHypothesisInfo != null ?result.AssociationHypothesisInfo.ProbabilityOfHypothesisGivenData : 0);
            cmd.Parameters.AddWithValue("@qc_plot", "");
            await Task.Run(() => cmd.ExecuteNonQuery());
            return await this.LastID(cmd);
        }

        private async Task<long> InsertIdentifications(SQLiteCommand cmd, IdentifiedIsomerInfo result, long analysisID)
        {
                cmd.CommandText = "INSERT INTO identifications (detection_analysis , measured_mz_in_dalton, ppm_error, viper_compatible_mz, r2 , analysis_status, intensity_score , peak_shape_score , isotopic_score , collision_cross_section, mobility, t0) VALUES      (@detection_analysis , @measured_mz_in_dalton, @ppm_error, @viper_compatible_mz, @r2, @analysis_status, @intensity_score, @peak_shape_score, @isotopic_score, @collision_cross_section, @mobility, @t0)";
                cmd.Parameters.AddWithValue("@detection_analysis", analysisID);
                cmd.Parameters.AddWithValue("@measured_mz_in_dalton", result.MzInDalton);
                cmd.Parameters.AddWithValue("@ppm_error", result.MzInPpm);
                cmd.Parameters.AddWithValue("@viper_compatible_mz", result.ViperCompatibleMass);
                cmd.Parameters.AddWithValue("@r2", result.RSquared);
                cmd.Parameters.AddWithValue("@analysis_status", result.AnalysisStatus.ToString());
                cmd.Parameters.AddWithValue("@intensity_score", result.PeakScores.IntensityScore);
                cmd.Parameters.AddWithValue("@peak_shape_score", result.PeakScores.PeakShapeScore);
                cmd.Parameters.AddWithValue("@isotopic_score", result.PeakScores.IsotopicScore);
                cmd.Parameters.AddWithValue("@collision_cross_section", result.CrossSectionalArea);
                cmd.Parameters.AddWithValue("@mobility", result.Mobility);
                cmd.Parameters.AddWithValue("@t0", result.T0);
                await Task.Run(() => cmd.ExecuteNonQuery());
                return await this.LastID(cmd);
        }

        private async Task InsertSnapshots(SQLiteCommand cmd, ArrivalTimeSnapShot result, long IdentificationID)
        {
            cmd.CommandText = "INSERT INTO peaks (ion , arrival_time_in_ms , temperature , pressure , drift_tube_voltage  , peak_profile)  VALUES  (@ion , @arrival_time_in_ms, @temperature, @pressure, @drift_tube_voltage, @peak_profile)";
            cmd.Parameters.AddWithValue("@ion", IdentificationID);
            cmd.Parameters.AddWithValue("@arrival_time_in_ms", result.MeasuredArrivalTimeInMs);
            cmd.Parameters.AddWithValue("@temperature", result.TemperatureInKelvin);
            cmd.Parameters.AddWithValue("@pressure", result.PressureInTorr);
            cmd.Parameters.AddWithValue("@drift_tube_voltage", result.DriftTubeVoltageInVolt);
            cmd.Parameters.AddWithValue("@peak_profile", "");
            await Task.Run(() => cmd.ExecuteNonQuery());
        }

        private async Task<long> LastID(SQLiteCommand cmd)
        {
            string sql = @"select last_insert_rowid()";
            cmd.CommandText = sql;
            object lastId = cmd.ExecuteScalar();
            
            return (long)lastId;
        }
    }
}
