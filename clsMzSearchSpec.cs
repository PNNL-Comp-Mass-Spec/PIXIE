using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IMSMzFinder
{
	class clsMzSearchSpec
	{
		/// <summary>
		/// m/z value to search for
		/// </summary>
		public double MZ { get; set; }

		/// <summary>
		/// Central frame number where we should find this m/z
		/// </summary>
		public int FrameNumCenter { get; set; }

		/// <summary>
		/// Frame number tolerance
		/// </summary>
		public int FrameNumTolerance { get; set; }

		/// <summary>
		/// Description of this species
		/// </summary>
		public string Description { get; set; }

        /// <summary>
        /// Ion type of the target m/z value
        /// </summary>
        public string Ion { get; set; }
	}
}
