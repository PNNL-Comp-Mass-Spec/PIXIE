
namespace IMSMzFinder
{
	class clsIsotopicFeature
	{
		public int FrameNum { get; set; }
		public int IMSScanNum { get; set; }
		public int Charge { get; set; }
		public double Abundance { get; set; }
		public double MZ { get; set; }
		public double Fit { get; set; }
		public double MonoisotopicMass { get; set; }
		public double SignalToNoise { get; set; }
		public double DriftTime { get; set; }
	}
}
