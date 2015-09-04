using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFinderBatchProcessor.Export
{
    using System.Data.SQLite;
    using System.IO;

    using IFinderBatchProcessor.Util;

    // A Sqlite library as a sqlite database connection
    public class AnalysisLibrary 
    {
        private readonly SQLiteCommand Insert;
        private readonly AsyncLock dbLock;
        private readonly string dbPath;
        private SQLiteConnection dbConnection;

        private readonly string createDatasetTableCommand = ("create table datasets (" + 
                "id primary key," + 
                "dataset_chemical integer," + 
                "name text," + 
                "path text," + 
                "date text," + 
                "foreign key(dataset_chemical) references chemicals(id)" + 
                ");");

        private readonly string createAnalysisTableCommand = ("create table analyses (" + 
                "id primary key," + 
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
                "id primary key," + 
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
                "id primary key," + 
                "ion integer," +  
                "arrival_time_in_ms real," + 
                "temperature real," + 
                "pressure real," + 
                "drift_tube_voltage real," + 
                "peak_profile blob," + 
                "foreign key(ion) references identifications(id)" +
                ");");

        private readonly string createChemicalsTableCommand = "create table chemicals (" + 
                "id primary key," + 
                "name text," + 
                "empirical_formula text," + 
                "chemical_class text" + 
                ");";

        private readonly string createTargetTableCommand = "create table targets (" + 
                "id primary key," + 
                "target_chemical integer," +  
                "target_type text," + 
                "adduct text," + 
                "target_description text," + 
                "monoisotopic_mass real," + 
                "composition_with_adduct text," + 
                "mass_with_adduct real," + 
                "charge_state integer," + 
                "foreign key(target_chemical) references chemicals(id)" + 
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
            this.CreateTables();
        }

        private async Task CreateTables()
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
    }
}
