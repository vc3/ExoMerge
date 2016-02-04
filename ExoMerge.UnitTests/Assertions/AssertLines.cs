using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.UnitTests.Assertions
{
	public static class AssertLines
	{
		public static void Match(string expectedText, string actualText, string lineTerminator)
		{
			if (expectedText != actualText)
			{
				var expectedLines = expectedText.Split(new[] { lineTerminator }, StringSplitOptions.None);
				var actualLines = actualText.Split(new[] { lineTerminator }, StringSplitOptions.None);

				var diff = new StringBuilder();

				var e = 0;
				var a = 0;

				while (e < expectedLines.Length || a < actualLines.Length)
				{
					if (e >= expectedLines.Length)
						diff.Append("+ " + actualLines[a++] + lineTerminator);
					else if (a >= actualLines.Length)
						diff.Append("- " + expectedLines[e++] + lineTerminator);
					else if (expectedLines[e] == actualLines[a])
					{
						diff.Append("   " + expectedLines[e++] + lineTerminator);
						a++;
					}
					else
					{
						var actualMatchingLineIndex = -1;

						for (var i = a; i < actualLines.Length; i++)
						{
							if (expectedLines[e] == actualLines[i])
							{
								actualMatchingLineIndex = i;
								break;
							}
						}

						var expectedMatchingLineIndex = -1;

						for (var i = e; i < expectedLines.Length; i++)
						{
							if (actualLines[a] == expectedLines[i])
							{
								expectedMatchingLineIndex = i;
								break;
							}
						}

						var distanceToActualMatch = actualMatchingLineIndex == -1 ? int.MaxValue : actualMatchingLineIndex - a;
						var distanceToExpectedMatch = expectedMatchingLineIndex == -1 ? int.MaxValue : expectedMatchingLineIndex - e;

						if (actualMatchingLineIndex == -1 && expectedMatchingLineIndex == -1)
						{
							diff.Append("- " + expectedLines[e++] + lineTerminator);
							diff.Append("+ " + actualLines[a++] + lineTerminator);
						}
						else if (actualMatchingLineIndex != -1 && distanceToActualMatch <= distanceToExpectedMatch)
						{
							while (a < actualMatchingLineIndex)
								diff.Append("+ " + actualLines[a++] + lineTerminator);
							diff.Append("   " + actualLines[a++] + lineTerminator);

							e++;
						}
						else if (expectedMatchingLineIndex != -1 && distanceToExpectedMatch <= distanceToActualMatch)
						{
							while (e < expectedMatchingLineIndex)
								diff.Append("- " + expectedLines[e++] + lineTerminator);
							diff.Append("   " + expectedLines[e++] + lineTerminator);

							a++;
						}
						else
							throw new InvalidOperationException();
					}
				}

				//Assert.AreEqual(expectedText, actualText, "\r\nActual lines do not match expected:\r\n--------------------------------------------\r\n{0}", diff);
				Assert.Fail("\r\nActual lines do not match expected:\r\n--------------------------------------------\r\n{0}", diff);
			}
		}
	}
}
