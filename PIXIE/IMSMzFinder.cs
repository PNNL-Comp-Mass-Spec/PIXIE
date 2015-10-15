namespace PIXIE
{
	//class ImsMzFinder 
	//{
	//	/// <summary>
	//	/// Constructor
	//	/// </summary>
	//	public ImsMzFinder()
	//	{
	//		mFileDate = "September 3, 2014";
	//		InitializeLocalVariables();
    //        iparams = new InformedParameters
    //        {
    //            ChargeStateMax = 5,
	//		    NetTolerance = 0.1,
	//		    IsotopicFitScoreMax = 0.15,
	//		    MassToleranceInPpm = 30,
	//	        NumPointForSmoothing = 9
    //        };
	//	}
    //
	//	#region "Constants and enums"
    //
	//	public const float DEFAULT_MZ_TOLERANCE_PPM = 30;
    //
	//	public const float MINIMUM_DRIFT_TIME_SEPARATION_MSEC = 5;
    //
	//	public enum ePepXMLMergerStatus
	//	{
	//		NoError = 0,
	//		UnspecifiedError = -1
	//	}
    //
	//	private enum eIsosFileColumns
	//	{
	//		FrameNum = 0,
	//		ImsScanNum = 1,
	//		Charge = 2,
	//		Abundance = 3, 
	//		MZ = 4,
	//		Fit = 5,
	//		MonoisotopicMass = 6,
	//		SignalToNoise = 7,
	//		DriftTime = 8
	//	}
    //
	//	private enum eTargetMzColumns
	//	{
	//		Dataset	= 0,
	//		MZ = 1,
	//		FrameNum = 2,
	//		FrameNumTolerance = 3,
	//		Description = 4
	//	}
    //
	//	#endregion
    //
	//	#region "Classwide Variables"
    //
	//	private ePepXMLMergerStatus mLocalErrorCode;
    //
	//	private Dictionary<string, List<MzSearchSpec>> targets_MZs_by_dataset;
    //
    //    public InformedParameters iparams;
    //
	//	private StreamWriter mResultsFile;
    //
	//	#endregion
    //
	//	#region "Properties"
    //
	//	public ePepXMLMergerStatus LocalErrorCode
	//	{
	//		get
	//		{
	//			return mLocalErrorCode;
	//		}
	//	}
    //
	//	public string DatasetAndMzFilePath { get; set; }
    //
	//	public float MZTolerancePPM { get; set; }
    //
	//	public string ResultsFileName { get; set; }
    //
	//	#endregion
    //
	//	public void CloseResultsFile()
	//	{
	//		if (mResultsFile != null)
	//		{
	//			try
	//			{
	//				mResultsFile.Close();
	//			}
	//			catch (ObjectDisposedException)
	//			{
	//				// This exception can be ignored
	//			}
	//			catch (Exception ex)
	//			{
	//				Console.WriteLine("Error closing file: " + ex.Message);					
	//			}
    //
	//		}
	//		
	//	}
    //
	//	private bool FeatureMatchesTargetMZs(IEnumerable<MzSearchSpec> lstMzSearchInfo, IsotopicFeature isosFeature)
	//	{
	//		foreach (var mzTarget in lstMzSearchInfo)
	//		{
	//			if (IsFeatureMatch(isosFeature, mzTarget))
	//			{
	//				return true;
	//			}				
	//		}
    //
	//		return false;
	//	}
    //
	//	public override string GetErrorMessage()
	//	{
    //
	//		string strErrorMessage;
    //
	//		if (base.ErrorCode == eProcessFilesErrorCodes.LocalizedError || base.ErrorCode == eProcessFilesErrorCodes.NoError)
	//		{
	//			switch (mLocalErrorCode)
	//			{
	//				case ePepXMLMergerStatus.NoError:
	//					strErrorMessage = "";
	//					break;
    //
	//				case ePepXMLMergerStatus.UnspecifiedError:
	//					strErrorMessage = "Unspecified localized error";
	//					break;
    //
	//				default:
	//					// This shouldn't happen
	//					strErrorMessage = "Unknown error state";
	//					break;
	//			}
	//		}
	//		else
	//		{
	//			strErrorMessage = base.GetBaseClassErrorMessage();
	//		}
    //
	//		return strErrorMessage;
    //
	//	}
    //
	//	private List<MzSearchSpec> GetTargetMZsForDataset(string baseName, out string datasetName)
	//	{
	//		// Find the the data for baseName in targets_MZs_by_dataset
    //
	//		datasetName = string.Copy(baseName);
    //
	//		if (targets_MZs_by_dataset == null)
	//		{
	//			ShowErrorMessage("targets_MZs_by_dataset is null in GetTargetMZsForDataset; programming error");				
	//			return new List<MzSearchSpec>();
	//		}
    //
	//		// First search for an exact match to baseName
	//		List<MzSearchSpec> lstTargetMZs;
	//		if (targets_MZs_by_dataset.TryGetValue(baseName, out lstTargetMZs))
	//		{
	//			return lstTargetMZs;
	//		}
    //
	//		// Exact match not found; look for a partial (but exact) match
	//		for (var i = baseName.Length - 1; i > 4; i--)
	//		{
	//			if (targets_MZs_by_dataset.TryGetValue(baseName.Substring(0, i), out lstTargetMZs))
	//			{
	//				datasetName = baseName.Substring(0, i);
	//				return lstTargetMZs;
	//			}
	//		}
    //
	//		// Partial match not found; find the best match (and show a warning)
    //
	//		for (var i = baseName.Length - 1; i > 4; i--)
	//		{
    //
	//			// The key in the bestMatch variable is a key name in targets_MZs_by_dataset
	//			// The value is the number of characters in that key name that were matched
	//			var bestMatch = new KeyValuePair<string, int>(string.Empty, 0);
    //
	//			foreach (var mzSearchSpec in targets_MZs_by_dataset)
	//			{
	//				if (mzSearchSpec.Key.ToLower().StartsWith(baseName.Substring(0, i).ToLower()))
	//				{
	//					if (string.IsNullOrEmpty(bestMatch.Key) || i > bestMatch.Value)
	//					{
	//						bestMatch = new KeyValuePair<string, int>(mzSearchSpec.Key, i);
	//					}
	//				}
	//			}
    //
	//			if (!string.IsNullOrEmpty(bestMatch.Key))
	//			{
	//				ShowWarning("Warning: Exact match not found in targets_MZs_by_dataset for " + baseName + "; using best match: " + bestMatch.Key);
    //
	//				if (targets_MZs_by_dataset.TryGetValue(bestMatch.Key, out lstTargetMZs))
	//				{
	//					datasetName = bestMatch.Key;
	//					return lstTargetMZs;
	//				}
	//			}
	//		}
    //
	//		// Match not found
	//		ShowErrorMessage("Dataset not found in targets_MZs_by_dataset: " + baseName);
	//		return new List<MzSearchSpec>();
	//	}
    //
	//	private string GetValue(string[] dataVals, Dictionary<Int16, int> dctHeaderMap, Int16 columnCode)
	//	{
	//		int columnIndex;
    //
	//		if (dctHeaderMap.TryGetValue(columnCode, out columnIndex))
	//		{
	//			if (columnIndex >= 0)
	//			{
	//				return dataVals[columnIndex];					
	//			}
	//		}
    //
	//		return string.Empty;
	//	}
    //
	//	private int GetValueInt(string[] dataVals, Dictionary<Int16, int> dctHeaderMap, Int16 columnCode)
	//	{
	//		string valueText = GetValue(dataVals, dctHeaderMap, columnCode);
	//		
	//		int value;
	//		if (int.TryParse(valueText, out value))
	//			return value;
    //
	//		return 0;
	//	}
    //
	//	private double GetValueDouble(string[] dataVals, Dictionary<Int16, int> dctHeaderMap, Int16 columnCode)
	//	{
	//		string valueText = GetValue(dataVals, dctHeaderMap, columnCode);
	//		
	//		double value;
	//		if (double.TryParse(valueText, out value))
	//			return value;
    //
	//		return 0;
	//	}
	//	
	//	private void InitializeLocalVariables()
	//	{
	//		base.ShowMessages = true;
	//		base.LogMessagesToFile = false;
    //
	//		targets_MZs_by_dataset = new Dictionary<string, List<MzSearchSpec>>(StringComparer.CurrentCultureIgnoreCase);
    //
	//		MZTolerancePPM = DEFAULT_MZ_TOLERANCE_PPM;
    //
	//		DatasetAndMzFilePath = string.Empty;
	//		ResultsFileName = string.Empty;
    //
	//		mLocalErrorCode = ePepXMLMergerStatus.NoError;
	//	}
	//			
	//	private bool InitializeResultsFile(FileInfo fiInputFile, string outputFolderPath)
	//	{
	//		try
	//		{
    //
	//			string outputFilePath;
    //
	//			if (string.IsNullOrWhiteSpace(ResultsFileName))
	//			{
	//				outputFilePath = Path.GetFileNameWithoutExtension(fiInputFile.Name) + "_results.txt";
	//			}
	//			else
	//			{
	//				outputFilePath = string.Copy(ResultsFileName);
	//			}
    //
	//			if (!string.IsNullOrWhiteSpace(outputFolderPath))
	//				outputFilePath = Path.Combine(outputFolderPath, outputFilePath);
    //
	//			Console.WriteLine();
	//			ShowMessage("Creating results file: " + outputFilePath);
    //
	//			mResultsFile = new StreamWriter(new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
	//			{
	//				AutoFlush = true
	//			};
    //
	//			mResultsFile.WriteLine("Dataset" + '\t' +
	//			                       "MZ_Target" + '\t' +
	//								   "MZ_Observed" + '\t' +
	//			                       "FrameNum" + '\t' +
	//			                       "DriftTime" + '\t' +
	//								   "Abundance" + '\t' +
	//								   "PercentMaxAbu" + '\t' +
	//								   "Description");
	//		}
	//		catch (Exception ex)
	//		{
	//			ShowErrorMessage("Error initializing the results file: " + ex.Message);
	//			return false;
	//		}
    //
	//		return true;
	//	}
    //
	//	private bool IsFeatureMatch(IsotopicFeature isosFeature, MzSearchSpec mzTarget)
	//	{
	//		bool isMatch = false;
	//		if (Math.Abs(isosFeature.FrameNum - mzTarget.FrameNumCenter) <= mzTarget.FrameNumTolerance)
	//		{
	//			// The isos feature is within the desired frame range; check m/z
	//			if (Math.Abs(isosFeature.MZ - mzTarget.MZ) / (mzTarget.MZ / 1E6) <= MZTolerancePPM)
	//			{
	//				// The isos feature is within the desired mass tolerance
	//				isMatch = true;
	//			}
	//		}
    //
	//		return isMatch;
	//	}
    //
	//	private bool LoadTargetMZs()
	//	{
	//		if (targets_MZs_by_dataset != null)
	//			targets_MZs_by_dataset.Clear();
	//		else
	//			targets_MZs_by_dataset = new Dictionary<string, List<MzSearchSpec>>(StringComparer.CurrentCultureIgnoreCase);
    //
	//		try
	//		{
	//			if (string.IsNullOrWhiteSpace(DatasetAndMzFilePath))
	//			{
	//				ShowWarning("DatasetAndMzFilePath is empty; cannot load the target MZs");
	//				return false;
	//			}
    //
	//			var fiTargetMZs = new FileInfo(DatasetAndMzFilePath);
	//			if (!fiTargetMZs.Exists)
	//			{
	//				ShowWarning("Target MZs file not found: " + DatasetAndMzFilePath);
	//				return false;
	//			}
    //
	//			ShowMessage("\nReading targets from: " + DatasetAndMzFilePath);
    //
	//			using (var srTargetMZs = new StreamReader(new FileStream(fiTargetMZs.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
	//			{
	//				// Keys are column name enums (as Int16 values); values are the column index
	//				Dictionary<Int16, int> dctHeaderMap = null;
    //
	//				while (srTargetMZs.Peek() > -1)
	//				{
	//					// Required column names: DatasetName, Target_MZ, FrameNum, FrameTolerance, Description
    //
	//					var dataLine = srTargetMZs.ReadLine();
	//					if (string.IsNullOrWhiteSpace(dataLine))
	//						continue;
    //
	//					var dataVals = dataLine.Split('\t');
	//					if (dataVals.Length == 0)
	//						continue;
	//					
	//					if (dctHeaderMap == null)
	//					{
	//						dctHeaderMap = ParseTargetMZHeaderLine(dataVals);
	//						continue;
	//					}
    //
	//					string dataset = GetValue(dataVals, dctHeaderMap, (Int16)eTargetMzColumns.Dataset);
	//					var targetMZ = GetValueDouble(dataVals, dctHeaderMap, (Int16)eTargetMzColumns.MZ);
    //
	//					if (Math.Abs(targetMZ) < double.Epsilon)
	//					{
	//						ShowMessage("Skipping invalid target m/z for " + dataLine);
	//						continue;
	//					}
    //
	//					List<MzSearchSpec> lstMZsForDataset;
    //
	//					if (!targets_MZs_by_dataset.TryGetValue(dataset, out lstMZsForDataset))
	//					{
	//						lstMZsForDataset = new List<MzSearchSpec>();
	//						targets_MZs_by_dataset.Add(dataset, lstMZsForDataset);
	//					}
    //
	//					var mzSearchSpec = new MzSearchSpec
	//					{
	//						MZ = targetMZ,
	//						FrameNumCenter = GetValueInt(dataVals, dctHeaderMap, (Int16)eTargetMzColumns.FrameNum),
	//						FrameNumTolerance = GetValueInt(dataVals, dctHeaderMap, (Int16)eTargetMzColumns.FrameNumTolerance),
	//						Description = GetValue(dataVals, dctHeaderMap, (Int16)eTargetMzColumns.Description)
	//					};
    //
	//					lstMZsForDataset.Add(mzSearchSpec);
    //
	//				}
	//			}
    //
	//			ShowMessage("Loaded " + targets_MZs_by_dataset.Count + " targets");
    //
	//		}
	//		catch (Exception ex)
	//		{
	//			ShowErrorMessage("Error loading the target MZs for each dataset: " + ex.Message);
	//			return false;
	//		}
    //
	//		return true;
	//	}
    //
	//	private void MatchFeaturesToTargets(
	//		IEnumerable<MzSearchSpec> lstMzSearchInfo, 
	//		List<IsotopicFeature> lstIsosFeatures, 
	//		string datasetName,
	//		double maxAbundanceInDataset)
	//	{
	//		var mzTargetMatches = new Dictionary<MzSearchSpec, List<IsotopicFeature>>();
    //
	//		foreach (var mzTarget in lstMzSearchInfo)
	//		{
	//			mzTargetMatches.Add(mzTarget, new List<IsotopicFeature>());
	//		}
    //
	//		foreach (var mzTargetMatch in mzTargetMatches)
	//		{
	//			foreach (var isosFeature in lstIsosFeatures)
	//			{
	//				if (!IsFeatureMatch(isosFeature, mzTargetMatch.Key))
	//				{
	//					continue;
	//				}
    //
	//				// Associate this isos feature with this Mz Target
	//					
	//				// Look for an existing isosFeature match within MINIMUM_DRIFT_TIME_SEPARATION_MSEC msec of this one
	//				// If one is found, keep the one that has higher abundance
	//				// If none is found, add this isosFeature
    //
	//				if (mzTargetMatch.Value.Count == 0)
	//				{
	//					mzTargetMatch.Value.Add(isosFeature);
	//					continue;
	//				}
    //
	//				bool matchFound = false;
	//				for (int i = 0; i < mzTargetMatch.Value.Count; i++)
	//				{
	//					if (Math.Abs(mzTargetMatch.Value[i].DriftTime - isosFeature.DriftTime) < MINIMUM_DRIFT_TIME_SEPARATION_MSEC)
	//					{
	//						if (isosFeature.Abundance > mzTargetMatch.Value[i].Abundance)
	//						{
	//							// Replace the stored isos feature
	//							mzTargetMatch.Value[i] = isosFeature;									
	//						}
    //
	//						matchFound = true;
	//						break;
	//					}
	//				}
    //
	//				if (!matchFound)
	//				{
	//					// Match not found within MINIMUM_DRIFT_TIME_SEPARATION_MSEC; add a new match
	//					mzTargetMatch.Value.Add(isosFeature);
	//				}
    //
	//			} // foreach isosFeature
	//		} // foreach mzTargetMatch
    //
	//		// Write out the results
	//		foreach (var mzTargetMatch in mzTargetMatches)
	//		{
    //
	//			if (mzTargetMatch.Value.Count == 0)
	//			{
	//				WriteEmptyMatch(mzTargetMatch.Key, datasetName);
	//				continue;
	//			}
    //
	//			// Write out the top 3 matches, but only write out matches with abundance values at least 50% of the highest value
	//			double maxAbundance = 0;
	//			foreach (var isosFeature in mzTargetMatch.Value)
	//			{
	//				if (isosFeature.Abundance > maxAbundance)
	//					maxAbundance = isosFeature.Abundance;
	//			}
    //
	//			double abundanceThreshold = maxAbundance / 2.0;
    //
	//			// Select the top 3 items
	//			var query = (from item in mzTargetMatch.Value orderby item.Abundance descending select item).Take(3);
    //
	//			foreach (var isosFeature in query)
	//			{
	//				if (isosFeature.Abundance < abundanceThreshold)
	//					break;
    //
	//				double percentMaxAbundance = 0;
	//				if (maxAbundanceInDataset > 0)
	//					percentMaxAbundance = (isosFeature.Abundance / maxAbundanceInDataset) * 100;
    //
	//				mResultsFile.WriteLine(datasetName + '\t' +
	//								   mzTargetMatch.Key.MZ + '\t' +
	//								   isosFeature.MZ + '\t' +
	//								   isosFeature.FrameNum + '\t' +
	//								   isosFeature.DriftTime.ToString("0.00") + '\t' +
	//								   isosFeature.Abundance.ToString("0") + '\t' +
	//								   percentMaxAbundance.ToString("0") + "%" + '\t' +
	//								   mzTargetMatch.Key.Description
	//								   );
	//			}
	//			
    //
	//		}
	//	}
	//
	//	private Dictionary<Int16, int> ParseIsosHeaderLine(string[] dataVals)
	//	{
	//		var dctHeaderMap = new Dictionary<Int16, int>();
    //
	//		var dctColumnsToFind = new Dictionary<string, eIsosFileColumns>
	//		{
	//			{"frame_num", eIsosFileColumns.FrameNum},
	//			{"ims_scan_num", eIsosFileColumns.ImsScanNum},
	//			{"charge", eIsosFileColumns.Charge},
	//			{"abundance", eIsosFileColumns.Abundance},
	//			{"mz", eIsosFileColumns.MZ},
	//			{"fit", eIsosFileColumns.Fit},
	//			{"monoisotopic_mw", eIsosFileColumns.MonoisotopicMass},
	//			{"signal_noise", eIsosFileColumns.SignalToNoise},
	//			{"drift_time", eIsosFileColumns.DriftTime},
	//		};
    //
	//		// Initialize the values in dctHeaderMap
	//		foreach (var item in dctColumnsToFind)
	//		{
	//			dctHeaderMap.Add((Int16)item.Value, -1);
	//		}
    //
	//		// Update the values in dctHeaderMap with the actual column indices
	//		for (int i = 0; i < dataVals.Length; i++)
	//		{
	//			eIsosFileColumns columnCode;
	//			if (dctColumnsToFind.TryGetValue(dataVals[i], out columnCode))
	//				dctHeaderMap[(Int16)columnCode] = i;				
	//		}
	//		
	//		return dctHeaderMap;
    //
	//	}
    //
	//	private Dictionary<Int16, int> ParseTargetMZHeaderLine(string[] dataVals)
	//	{
	//		var dctHeaderMap = new Dictionary<Int16, int>();
    //
	//		var dctColumnsToFind = new Dictionary<string, eTargetMzColumns>
	//		{
	//			{"Dataset", eTargetMzColumns.Dataset},
	//			{"MZ", eTargetMzColumns.MZ},
	//			{"FrameNum", eTargetMzColumns.FrameNum},
	//			{"FrameTolerance", eTargetMzColumns.FrameNumTolerance},
	//			{"Description", eTargetMzColumns.Description}			
	//		};
    //
	//		// Initialize the values in dctHeaderMap
	//		foreach (var item in dctColumnsToFind)
	//		{
	//			dctHeaderMap.Add((Int16)item.Value, -1);
	//		}
    //
	//		// Update the values in dctHeaderMap with the actual column indices
	//		for (int i = 0; i < dataVals.Length; i++)
	//		{
	//			eTargetMzColumns columnCode;
	//			if (dctColumnsToFind.TryGetValue(dataVals[i], out columnCode))
	//				dctHeaderMap[(Int16)columnCode] = i;				
	//		}
	//		
	//		return dctHeaderMap;
    //
	//	}
    //
	//	/// <summary>
	//	/// Main processing function
	//	/// </summary>
	//	/// <param name="strInputFilePath"></param>
	//	/// <param name="strOutputFolderPath"></param>
	//	/// <param name="strParameterFilePath"></param>
	//	/// <param name="blnResetErrorCode"></param>
	//	/// <returns>True if success, false if an error</returns>
	//	public override bool ProcessFile(string strInputFilePath, string strOutputFolderPath, string strParameterFilePath, bool blnResetErrorCode)
	//	{
	//		bool success = false;
    //
	//		if (blnResetErrorCode)
	//		{
	//			base.SetBaseClassErrorCode(eProcessFilesErrorCodes.NoError);
	//		}
    //
	//		try
	//		{
	//			if (string.IsNullOrWhiteSpace(strInputFilePath))
	//			{
	//				ShowWarning("Input file name is empty");
	//				base.SetBaseClassErrorCode(eProcessFilesErrorCodes.InvalidInputFilePath);
	//				return false;
    //
	//			}
    //
	//			var fiInputFile = new FileInfo(strInputFilePath);
	//			if (!fiInputFile.Exists)
	//			{
	//				ShowWarning("Input file not found: " + strInputFilePath);
	//				base.SetBaseClassErrorCode(eProcessFilesErrorCodes.InvalidInputFilePath);
	//				return false;
	//			}
    //
	//			// Load the search info if necessary
	//			if (targets_MZs_by_dataset == null || targets_MZs_by_dataset.Count == 0)
	//			{
	//				success = LoadTargetMZs();
	//				if (!success)
	//					return false;
	//			}
    //
	//			// Initialize the results file if necessary
	//			// Note that results for all datasets searched will be included in a single results file
	//			if (mResultsFile == null)
	//				success = InitializeResultsFile(fiInputFile, strOutputFolderPath);
	//			else
	//				success = true;
    //
	//			if (!success)
	//				return false;
	//			
	//			// Examine the input file name to determine the correct processing method
	//			string lowercaseFilename = fiInputFile.Name.ToLower();
	//			if (lowercaseFilename.EndsWith("_isos.csv"))
	//			{
	//				success = SearchIsosForMzList(fiInputFile);
	//			}
	//			else if (lowercaseFilename.EndsWith(".uimf"))
	//			{
	//				success = SearchUIMFForMzList(fiInputFile);
    //                Console.WriteLine("Debugging anchor: Press any key to exit...");
    //                Console.ReadKey();  
	//			}
	//			else
	//			{
	//				ShowErrorMessage("The input file must be a DeconTools _isos.csv file or a .UIMF file");
	//				success = false;
	//			}
	//		}
	//		catch (Exception ex)
	//		{
	//			ShowErrorMessage("Error in ProcessFolder: " + ex.Message);
	//			return false;
	//		}
    //
	//		return success;
    //
	//	}
    //
	//	public bool SearchIsosForMzList(FileInfo fiIsosFile)
	//	{
	//		try
	//		{
	//			string datasetName;
	//			string baseName;
    //
	//			Console.WriteLine();
	//			ShowMessage("Processing file " + fiIsosFile.FullName);
    //
	//			if (!TrimSuffix(fiIsosFile.Name, "_Filtered_isos.csv", out baseName))
	//			{
	//				if (!TrimSuffix(fiIsosFile.Name, "_isos.csv", out baseName))
	//				{
	//					baseName = Path.GetFileNameWithoutExtension(fiIsosFile.Name);
	//				}
	//			}
    //
	//			List<MzSearchSpec> lstMzSearchInfo = GetTargetMZsForDataset(baseName, out datasetName);
    //
	//			if (lstMzSearchInfo.Count == 0)
	//			{
	//				// Nothing to do; treat this as not a success
	//				ShowWarning("Nothing to do: target m/z list was empty for this dataset " + datasetName);
	//				return false;
	//			}
    //
	//			// Keys are column name enums (as Int16 values); values are the column index
	//			Dictionary<Int16, int> dctHeaderMap = null;
    //
	//			// Keep track of the highest abundance value seen
	//			double maxAbundanceInDataset = 0;
    //
	//			var lstIsosFeatures = new List<IsotopicFeature>();
    //
	//			using (var srIsosFile = new StreamReader(new FileStream(fiIsosFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
	//			{
	//				while (srIsosFile.Peek() > -1)
	//				{
	//					var lineIn = srIsosFile.ReadLine();
	//					if (string.IsNullOrWhiteSpace(lineIn))
	//						continue;
    //
	//					// Split the line on commas
	//					var dataVals = lineIn.Split(',');
    //
	//					if (dctHeaderMap == null)
	//					{
	//						// Parse the header line
	//						dctHeaderMap = ParseIsosHeaderLine(dataVals);
	//						continue;
	//					}
    //
    //
	//					// Pull out the data values
	//					var isosFeature = new IsotopicFeature
	//					{
	//						FrameNum = GetValueInt(dataVals, dctHeaderMap, (Int16)eIsosFileColumns.FrameNum),
	//						MZ = GetValueDouble(dataVals, dctHeaderMap, (Int16)eIsosFileColumns.MZ),
	//						Abundance = GetValueDouble(dataVals, dctHeaderMap, (Int16)eIsosFileColumns.Abundance)
	//					};
    //
	//					if (isosFeature.Abundance > maxAbundanceInDataset)
	//						maxAbundanceInDataset = isosFeature.Abundance;
    //
	//					// Check whether this feature is in lstMzSearchInfo
	//					if (FeatureMatchesTargetMZs(lstMzSearchInfo, isosFeature))
	//					{
	//						// Keep this feature
	//						// First parse the remaining values
	//						isosFeature.IMSScanNum = GetValueInt(dataVals, dctHeaderMap, (Int16)eIsosFileColumns.ImsScanNum);
	//						isosFeature.Charge = GetValueInt(dataVals, dctHeaderMap, (Int16)eIsosFileColumns.Charge);							
	//						isosFeature.Fit = GetValueDouble(dataVals, dctHeaderMap, (Int16)eIsosFileColumns.Fit);
	//						isosFeature.MonoisotopicMass = GetValueDouble(dataVals, dctHeaderMap, (Int16)eIsosFileColumns.MonoisotopicMass);
	//						isosFeature.SignalToNoise = GetValueDouble(dataVals, dctHeaderMap, (Int16)eIsosFileColumns.SignalToNoise);
	//						isosFeature.DriftTime = GetValueDouble(dataVals, dctHeaderMap, (Int16)eIsosFileColumns.DriftTime);
    //
	//						lstIsosFeatures.Add(isosFeature);
	//					}
	//					
    //
	//				}
	//			}
    //
	//			if (lstIsosFeatures.Count == 0)
	//			{
	//				ShowMessage("Warning: No matches found for " + datasetName);
    //
	//				foreach (var mzTarget in lstMzSearchInfo)
	//				{
	//					WriteEmptyMatch(mzTarget, datasetName);
	//				}
	//				
	//				return true;
	//			}
    //
	//			// Parse the cached Isos Features to find the best match (or matches) for each target m/z value
	//			// Write the results to mResultsFile
	//			MatchFeaturesToTargets(lstMzSearchInfo, lstIsosFeatures, datasetName, maxAbundanceInDataset);
    //
	//		}
	//		catch (Exception ex)
	//		{
	//			ShowErrorMessage("Error in SearchIsosForMzList: " + ex.Message);
	//			return false;
	//		}
    //
	//		return true;
	//	}
    //
    //
    //    // AgilentToUIMFConvert the UIMF file to iso.csv file and match features with the target m/z list.
	//	private bool SearchUIMFForMzList(FileInfo fiInputFile)
	//	{
	//		Console.WriteLine("Processing file:" + fiInputFile.Name + " using ImsInformed");
    //        var informed_workflow = new InformedWorkflow(fiInputFile.FullName, iparams);
    //        //informed_workflow.RunInformedWorkflow(void);
    //        string random_peptite = "ATVLNYLPK";
    //        //informed_workflow.ExtractData();
    //        ImsTarget target = new ImsTarget(1, random_peptite, 0.3612);
    //        
    //        //foreach ()
    //        //ChargeStateCorrelationResult correlationResult = informed_workflow.
	//		return false;
	//	}
    //
	//	private bool TrimSuffix(string fileName, string suffix, out string trimmedFileName)
	//	{
    //
	//		if (fileName.ToLower().EndsWith(suffix.ToLower()))
	//		{
	//			trimmedFileName = fileName.Substring(0, fileName.Length - suffix.Length);
	//			return true;
	//		}
    //
	//		trimmedFileName = string.Empty;
	//		return false;
    //
	//	}
    //
	//	private void WriteEmptyMatch(MzSearchSpec mzTarget, string datasetName)
	//	{
	//		mResultsFile.WriteLine(datasetName + '\t' +
	//								   mzTarget.MZ + '\t' +
	//								   "-NoMatch-" + '\t' +    // Observed m/z
	//								   string.Empty + '\t' +	// FrameNum
	//								   string.Empty + '\t' +	// Drift Time
	//								   string.Empty + '\t' +    // Abundance
	//								   string.Empty + '\t' +    // Percent of MaxAbundance
	//								   mzTarget.Description
	//								   );
	//	}
	//}

}
