using System.IO;
using Aspose.Words;
using ExoMerge.Aspose.UnitTests.Extensions;
using ExoMerge.UnitTests.Common;

namespace ExoMerge.Aspose.UnitTests.Common
{
	public class DocumentTestsBase : TestsBase
	{
		internal static bool HasLicense { get; private set; }

		protected override void OnBeforeTest()
		{
			var projectDirectory = GetProjectDirectory();
			var licensePath = Path.Combine(projectDirectory, "Aspose.Words.lic");

			if (File.Exists(licensePath))
			{
				var license = new License();

				try
				{
					using (var stream = File.OpenRead(licensePath))
					{
						license.SetLicense(stream);
					}

					HasLicense = true;
				}
				catch
				{
					HasLicense = false;
				}
			}
		}

		protected void AssertText(Document document, string text, bool hasSaved = false, bool convertFieldCodes = false, bool asMarkdown = false)
		{
			document.AssertText(TestContext, text, hasSaved: hasSaved, convertFieldCodes: convertFieldCodes, asMarkdown: asMarkdown);
		}
	}
}
