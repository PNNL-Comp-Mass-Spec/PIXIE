The IMS Mz Finder will search for m/z values in a series of
DeconTools _isos.csv files, reporting the intensity and drift time
of the best match for each m/z (within a given target frame range)

Future development may add the ability to search .UIMF files directly,
with support for a series of drift tube voltages and computation of
reduced mobility (K_0) and cross section.

Program syntax #1:
IMSMzFinder.exe DatasetAndMzFile.txt DatasetFileSpec

DatasetAndMzFile specifies a tab-delimited text file with
Dataset name, m/z, target frame, and frame tolerance

DatasetFileSpec specifies the file or files to analyze, for example
Dataset_isos.csv for one dataset or *_isos.csv for all datasets in a folder


-------------------------------------------------------------------------------
Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
Program started August 29, 2014

E-mail: matthew.monroe@pnnl.gov or matt@alchemistmatt.com
Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov or http://www.sysbio.org/resources/staff/
-------------------------------------------------------------------------------

Licensed under the Apache License, Version 2.0; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0
