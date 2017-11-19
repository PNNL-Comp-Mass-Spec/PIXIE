## Overview

PIXiE is an open source, C# software toolkit that automates extraction of arrival times 
and calculation of Collision Cross Sections (CCS) for molecules measured across 
multiple electric fields.

## Details

The primary application for PIXiE is in the construction of a reference library 
containing accurate mass and CCS data for metabolites and other small molecules. 

PIXiE also includes a parallel batch processor (PIXIEBatchProcessor) which 
automates data analysis for an arbitrary amount of targets. The batch processor 
searches for multiple targets in a set of DT-IMS-MS data, aggregating the results 
in a single SQLite database consisting of relational tables such as chemical targets,
data files, analyses, detected ions and peaks. This database can later be 
queried directly for quality control, post-processing or visualization purposes. 
Results after post processing can then be exported to an accurate mass and 
CCS library.

## Download
[PIXiE.zip](https://raw.github.com/PNNL-Comp-Mass-Spec/PIXIE/master/PIXiE_GUI.zip) from GitHub

## Contacts

Written by Jian Ma and Chris Wilkins for the Department of Energy (PNNL, Richland, WA) \
E-mail: proteomics@pnnl.gov \
Website: https://panomics.pnl.gov/ or https://omics.pnl.gov

## License

See the details in file [PNNL OSS License file](https://github.com/PNNL-Comp-Mass-Spec/PIXIE/blob/master/PNNL%20OSS%20License_citation.doc) on GitHub.
