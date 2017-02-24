using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIXIE.Runners
{
    using System.IO;

    using ImsInformed.IO;
    using ImsInformed.Workflows.VoltageAccumulation;

    using PIXIE.Options;
    using PIXIE.Preprocess;

    public class Converter
    {
        /// <summary>
        /// The execute converter.
        /// </summary>
        /// <param name="options">
        /// The options.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public static int ExecuteConverter(ConverterOptions options)
        {
            string inputPath = options.InputPath;
            string outputPath = options.OutputPath;
            string tubeLength = options.TubeLength;
            string inputExtension = Path.GetExtension(inputPath).ToLower();
            string conversionType = options.ConversionType.ToLower();
            string outputExtension = Path.GetExtension(outputPath).ToLower();
            if (inputExtension == ".uimf")
            {
                if (conversionType == "uimf")
                {
                    VoltageAccumulationWorkflow workflow = new VoltageAccumulationWorkflow(false, inputPath, outputPath);
                    return Convert.ToInt32(workflow.RunVoltageAccumulationWorkflow(FileFormatEnum.UIMF));
                }
                else if (conversionType == "mzml")
                {
                    VoltageAccumulationWorkflow workflow = new VoltageAccumulationWorkflow(false, inputPath, outputPath);
                    return Convert.ToInt32(workflow.RunVoltageAccumulationWorkflow(FileFormatEnum.MzML));
                }
                else
                {
                    throw new Exception("Output type " + inputExtension.ToLower() + " not supported");
                }
            }
            else if (inputExtension == ".d")
            {
                if (outputExtension == "uimf" || conversionType == "uimf")
                {
                    if (outputExtension != "uimf")
                    {
                        DirectoryInfo info = new DirectoryInfo(inputPath);
                        string fileName = info.Name.Replace(".d", "");
                    }

                    Task conversion = AgilentToUimfConversion.ConvertToUimf(inputPath, outputPath, tubeLength);
                    conversion.Wait();
                }
                else
                {
                    throw new Exception("Output type " + inputExtension.ToLower() + " not supported");
                }
            }
            else
            {
                throw new Exception("Input type " + inputExtension.ToLower() + " not supported");
            }

            return 0;
        }
    }
}
