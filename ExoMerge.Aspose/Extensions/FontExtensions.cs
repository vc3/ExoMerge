using Aspose.Words;

namespace ExoMerge.Aspose.Extensions
{
	public static class FontExtensions
	{
		/// <summary>
		/// Copy the font's settings into the target font.
		/// </summary>
		/// <param name="font">The font to copy settings from.</param>
		/// <param name="target">The font to copy settings to.</param>
		public static void CopyTo(this Font font, Font target)
		{
			target.AllCaps = font.AllCaps;
			target.Bidi = font.Bidi;
			target.Bold = font.Bold;
			target.BoldBi = font.BoldBi;

			font.Border.CopyTo(target.Border);

			target.Color = font.Color;
			target.ComplexScript = font.ComplexScript;
			target.DoubleStrikeThrough = font.DoubleStrikeThrough;
			target.Emboss = font.Emboss;
			target.Engrave = font.Engrave;
			target.Hidden = font.Hidden;
			target.HighlightColor = font.HighlightColor;
			target.Italic = font.Italic;
			target.ItalicBi = font.ItalicBi;
			target.Kerning = font.Kerning;
			target.LocaleId = font.LocaleId;
			target.LocaleIdBi = font.LocaleIdBi;
			target.LocaleIdFarEast = font.LocaleIdFarEast;
			target.Name = font.Name;
			target.NameAscii = font.NameAscii;
			target.NameBi = font.NameBi;
			target.NameFarEast = font.NameFarEast;
			target.NameOther = font.NameOther;
			target.NoProofing = font.NoProofing;
			target.Outline = font.Outline;
			target.Position = font.Position;
			target.Scaling = font.Scaling;

			font.Shading.CopyTo(target.Shading);

			target.Shadow = font.Shadow;
			target.Size = font.Size;
			target.SizeBi = font.SizeBi;
			target.SmallCaps = font.SmallCaps;
			target.Spacing = font.Spacing;
			target.StrikeThrough = font.StrikeThrough;
			target.Subscript = font.Subscript;
			target.Superscript = font.Superscript;
			target.TextEffect = font.TextEffect;
			target.Underline = font.Underline;
			target.UnderlineColor = font.UnderlineColor;
		}
	}
}
