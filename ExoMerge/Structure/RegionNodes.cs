using System;

namespace ExoMerge.Structure
{
	[Flags]
	public enum RegionNodes
	{
		/// <summary>
		/// The start tag of the region
		/// </summary>
		Start = 1,

		/// <summary>
		/// The content of the region
		/// </summary>
		Content = 2,

		/// <summary>
		/// The end tag of the region
		/// </summary>
		End = 4,

		/// <summary>
		/// The start and end tags of the region
		/// </summary>
		Tags = 5,

		/// <summary>
		/// All region nodes
		/// </summary>
		All = 7,
	}
}