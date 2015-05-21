// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BincCentricIndexing.cs" company="PNNL">
//   Written for the Department of Energy (PNNL, Richland, WA)
//   Copyright 2015, Battelle Memorial Institute.  All Rights Reserved.
// </copyright>
// <summary>
//   Defines the BincCentricIndexing type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ImsMetabolitesFinder.Preprocess
{
    using System;

    using UIMFLibrary;

    public class BincCentricIndexing
    {
        public static void IndexUimfFile(string uimfFileLocation)
        {
            bool indexed = false;
            using (var uimfReader = new DataReader(uimfFileLocation))
            {
                if (uimfReader.DoesContainBinCentricData())
                {
                    indexed = true;    
                    Console.WriteLine("Bin centric data found in dataset {0}.", uimfFileLocation);
                }
                else 
                {
                    Console.WriteLine("No bin centric data found for file {0}.", uimfFileLocation);
                }

                uimfReader.Dispose();
            }
            
            if (!indexed)
            {
                Console.WriteLine("Creating bin centric data for {0}.", uimfFileLocation);
                using (DataWriter dataWriter = new DataWriter(uimfFileLocation))
                {
                    dataWriter.CreateBinCentricTables();
                    dataWriter.Dispose();
                }
            }
        }
    }
}
