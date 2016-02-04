using Aspose.Words;

namespace ExoMerge.Aspose.Extensions
{
	public static class BorderExtensions
	{
		/// <summary>
		/// Copy the border's settings into the target border.
		/// </summary>
		/// <param name="border">The border to copy settings from.</param>
		/// <param name="target">The border to copy settings to.</param>
		public static void CopyTo(this Border border, Border target)
		{
			target.Color = border.Color;
			target.DistanceFromText = border.DistanceFromText;
			//other.IsVisible = self.IsVisible;
			target.LineStyle = border.LineStyle;
			target.LineWidth = border.LineWidth;
			target.Shadow = border.Shadow;
		}
	}
}
