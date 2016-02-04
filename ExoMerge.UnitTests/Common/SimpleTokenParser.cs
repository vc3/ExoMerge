using System;
using ExoMerge.Analysis;

namespace ExoMerge.UnitTests.Common
{
	/// <summary>
	/// Parses tokens as repeatable and conditional blocks by looking for the presence of keyword prefixes,
	/// e.g. "if, else if, else & end if", and "each & end each", followed by an expression where appropriate.
	/// To keep the syntax simple, punctuation such as parenthesis or curly braces are not used.
	/// </summary>
	public class SimpleTokenParser : KeywordTokenParser<Type>
	{
	}
}
