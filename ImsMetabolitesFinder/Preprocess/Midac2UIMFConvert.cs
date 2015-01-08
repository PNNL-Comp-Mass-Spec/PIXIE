using System.Collections.Generic;

namespace ImsMetabolitesFinder.Preprocess
{
    using System.Globalization;
    using System.IO;

    using Agilent.MassSpectrometry.MIDAC;

    using UIMFLibrary;

    public class Midac2UIMFConvert
    {
        public static void Convert(string inputFilePath, string outputDirectory)
        {
            string filePath = inputFilePath;
            var directoryInfo = new DirectoryInfo(filePath);
            string fileName = directoryInfo.Name.Replace(".d", "");
            string uimfFileName = outputDirectory + fileName + ".uimf";

            // Reset UIMF File
            if (File.Exists(uimfFileName)) 
            {
                File.Delete(uimfFileName);
            }
            // File.Create(uimfFileName);

            // Returns a reader alredy opened to the specified file path
            var reader = MidacFileAccess.ImsDataReader(filePath);

            // Get overall file metadata
            IMidacFileInfo fileInfo = reader.FileInfo;

            // Get info from the Agilent file
            double binWidth = fileInfo.TfsMsDetails.TofBinWidth;
            int numFrames = fileInfo.NumFrames;
            int numBins = fileInfo.MaxFlightTimeBin;
            IMidacFrameInfo firstFrameInfo = reader.FrameInfo(1);
            int numScans = fileInfo.MaxNonTfsMsPerFrame;
            int numAccumulations = firstFrameInfo.NumTransients;
            double averageTofLength = fileInfo.FileUnitConverter.DriftBinWidth * 1e6;
            double calibrationSlope = fileInfo.FileUnitConverter.TofMassCalA * 1e3;
            double calibrationIntercept = fileInfo.FileUnitConverter.TofMassCalTo / 1e3;

            // Open UIMF Writer
            using (var uimfWriter = new DataWriter(uimfFileName))
            {
                // Setup UIMF File
                uimfWriter.CreateTables(null);
                var globalParams = new GlobalParams();

                globalParams.AddUpdateValue(GlobalParamKeyType.BinWidth, binWidth);
                globalParams.AddUpdateValue(GlobalParamKeyType.Bins, numBins);
                globalParams.AddUpdateValue(GlobalParamKeyType.DateStarted, fileInfo.AcquisitionDate.ToString(CultureInfo.InvariantCulture));
                globalParams.AddUpdateValue(GlobalParamKeyType.InstrumentName, "Agilent " + fileInfo.InstrumentName);
                globalParams.AddUpdateValue(GlobalParamKeyType.NumFrames, numBins);
                globalParams.AddUpdateValue(GlobalParamKeyType.TOFCorrectionTime, numBins);
                globalParams.AddUpdateValue(GlobalParamKeyType.TimeOffset, numBins);
                globalParams.AddUpdateValue(GlobalParamKeyType.TOFIntensityType, numBins);
                globalParams.AddUpdateValue(GlobalParamKeyType.PrescanTOFPulses, numScans);
                globalParams.AddUpdateValue(GlobalParamKeyType.PrescanAccumulations, numAccumulations);

                uimfWriter.InsertGlobal(globalParams);

                for (int frame = 1; frame <= numFrames; frame++)
                {
                    Agilent.MassSpectrometry.IRlzArrayIterator[] iterArray;
                    reader.FrameMsIterators(frame, 0, numScans, out iterArray);
                    IMidacFrameInfo frameInfo = reader.FrameInfo(frame);

                    if (iterArray != null)
                    {
                        FrameParams frameParams = new FrameParams();

                        frameParams.AddUpdateValue(FrameParamKeyType.Accumulations, numAccumulations);
                        frameParams.AddUpdateValue(FrameParamKeyType.AverageTOFLength, averageTofLength);
                        frameParams.AddUpdateValue(FrameParamKeyType.CalibrationSlope, calibrationSlope);
                        frameParams.AddUpdateValue(FrameParamKeyType.CalibrationIntercept, calibrationIntercept);
                        frameParams.AddUpdateValue(FrameParamKeyType.CalibrationDone, 1);
                        frameParams.AddUpdateValue(FrameParamKeyType.Decoded, 0);
                        frameParams.AddUpdateValue(FrameParamKeyType.PressureFront, 4000);
                        frameParams.AddUpdateValue(FrameParamKeyType.PressureBack, frameInfo.DriftPressure);
                        frameParams.AddUpdateValue(FrameParamKeyType.Scans, iterArray.Length);
                        frameParams.AddUpdateValue(FrameParamKeyType.FrameType, (int)DataReader.FrameType.MS1);
                        frameParams.AddUpdateValue(FrameParamKeyType.AmbientTemperature, frameInfo.DriftTemperature);
                        frameParams.AddUpdateValue(FrameParamKeyType.FloatVoltage, frameInfo.DriftField * 100);

                        uimfWriter.InsertFrame(frame, frameParams);

                        for (int i = 0; i < iterArray.Length; i++)
                        {
                            Agilent.MassSpectrometry.IRlzArrayIterator iter = iterArray[i];

                            if (iter != null)
                            {
                                var binList = new List<int>();
                                var intensityList = new List<int>();

                                int bin;
                                int intensity;
                                while (iter.Next(out bin, out intensity))
                                {
                                    binList.Add(bin);
                                    intensityList.Add(intensity);
                                }

                               uimfWriter.InsertScan(frame, frameParams, i, intensityList, binWidth);
                            }
                        }
                    }
                }
            }
        }
    }
}
