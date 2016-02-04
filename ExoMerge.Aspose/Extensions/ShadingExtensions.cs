using Aspose.Words;

namespace ExoMerge.Aspose.Extensions
{
	public static class ShadingExtensions
	{
		/// <summary>
		/// Copy the shading's settings into the target shading.
		/// </summary>
		/// <param name="shading">The shading to copy settings from.</param>
		/// <param name="target">The shading to copy settings to.</param>
		public static void CopyTo(this Shading shading, Shading target)
		{
			if (ReferenceEquals(shading, target))
				return;

			target.BackgroundPatternColor = shading.BackgroundPatternColor;
			target.ForegroundPatternColor = shading.ForegroundPatternColor;
			target.Texture = shading.Texture;
		}
	}
}
