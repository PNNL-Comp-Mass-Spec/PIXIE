// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AgilentToUimfConversion.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the AgilentToUimfConversion type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PIXIE.Preprocess
{
    using System.Threading.Tasks;

    /// <summary>
    /// The agilent to uimf conversion.
    /// </summary>
    public class AgilentToUimfConversion
    {
        /// <summary>
        /// The convert to uimf.
        /// </summary>
        /// <param name="inputPath">
        /// The input path.
        /// </param>
        /// <param name="outputPath">
        /// The output path.
        /// </param>
        /// <param name="tubeLength">Drift tube length</param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static async Task ConvertToUimf(string inputPath, string outputPath, string tubeLength)
        {
            string[] args;
            if (tubeLength != null)
            {
                args = new string[] {inputPath, "-l", tubeLength, "-o", outputPath};
            }
            else
            {
                args = new string[] { inputPath, "-o", outputPath };
            }
            await Task.Run(() => AgilentToUimfConverter.Program.Main(args));
        }
    }
}
