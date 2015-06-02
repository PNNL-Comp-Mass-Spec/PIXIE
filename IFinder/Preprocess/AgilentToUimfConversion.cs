// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AgilentToUimfConversion.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the AgilentToUimfConversion type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinder.Preprocess
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// The agilent to uimf conversion.
    /// </summary>
    public class AgilentToUimfConversion
    {
        private const string Converter = "AgilentToUimfConverter.exe";

        /// <summary>
        /// The convert to uimf.
        /// </summary>
        /// <param name="inputPath">
        /// The input path.
        /// </param>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public async static Task ConvertToUimf(string inputPath, string outputPath)
        {
            Process conversionProcess = new Process();
            conversionProcess.StartInfo.FileName = Converter;
            conversionProcess.StartInfo.Arguments = inputPath + " " + outputPath;
            conversionProcess.StartInfo.UseShellExecute = false;
            conversionProcess.StartInfo.CreateNoWindow = true;
            conversionProcess.Start();
            await Task.Run(() => conversionProcess.WaitForExit());
        }
    }
}
