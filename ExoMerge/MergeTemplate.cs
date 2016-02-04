using System;
using System.Collections.Generic;
using System.Linq;
using ExoMerge.Analysis;
using ExoMerge.Structure;

namespace ExoMerge
{
	/// <summary>
	/// Represents the result of compiling a template.
	/// </summary>
	/// <typeparam name="TTemplate">The type of template that was compiled.</typeparam>
	/// <typeparam name="TToken">The type that represents a token.</typeparam>
	/// <typeparam name="TTarget">The type of object that the container's rendered content will apply to.</typeparam>
	/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
	/// <typeparam name="TSourceType">The type that identifies the type of the data source, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the data source that the container's content will be rendered for, e.g. 'Type'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	internal class MergeTemplate<TTemplate, TToken, TTarget, TElement, TSourceType, TSource, TExpression>
		where TToken : class, IToken
		where TExpression : class
	{
		public MergeTemplate(TTemplate template, TToken[] tokens, Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> root, MergeTemplateError<TToken>[] errors)
		{
			Template = template;
			Tokens = tokens;
			Root = root;
			Errors = errors;
		}

		/// <summary>
		/// Gets the underlying template object.
		/// </summary>
		public TTemplate Template { get; private set; }

		/// <summary>
		/// Gets the set of tokens that the template contains.
		/// </summary>
		public TToken[] Tokens { get; private set; }

		/// <summary>
		/// Gets the root container for the template.
		/// </summary>
		public Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> Root { get; private set; }

		/// <summary>
		/// Gets the set of errors that were discovered in the template.
		/// </summary>
		public MergeTemplateError<TToken>[] Errors { get; private set; }
	}

	/// <summary>
	/// Represents an error that was discovered when creating a template.
	/// </summary>
	public class MergeTemplateError<TToken> : MergeError<TToken>
		where TToken : class, IToken
	{
		internal MergeTemplateError(MergeTemplateErrorType type, TToken token, TokenType tokenType, IRegion<TToken> parentRegion, Exception exception = null)
			: base(MergeErrorType.Compilation, token, exception)
		{
			Type = type;
			TokenType = tokenType;
			ParentRegion = parentRegion;
		}

		/// <summary>
		/// 
		/// </summary>
		public new MergeTemplateErrorType Type { get; private set; }

		/// <summary>
		/// The parsed token type.
		/// </summary>
		public TokenType TokenType { get; private set; }

		/// <summary>
		/// The current open region when the error occurred.
		/// </summary>
		public IRegion<TToken> ParentRegion { get; private set; }
	}

	/// <summary>
	/// Represents the type of compilation error.
	/// </summary>
	public enum MergeTemplateErrorType
	{
		/// <summary>
		/// Unable to parse a token for an unknown or unspecified reason.
		/// </summary>
		UnableToParseToken,

		/// <summary>
		/// The expression parser could not parse the expression.
		/// </summary>
		UnableToParseExpression,

		/// <summary>
		/// Unable to parse an expression because the source data type is not known.
		/// </summary>
		UnknownDataType,

		/// <summary>
		/// Could not determine the type of the token.
		/// </summary>
		UnknownTokenType,

		/// <summary>
		/// A field found in an invalid location.
		/// </summary>
		InvalidFieldLocation,

		/// <summary>
		/// A token should have included an expression, but did not, e.g. a repeatable block with expression defined.
		/// </summary>
		MissingExpression,

		/// <summary>
		/// A region found in an invalid location.
		/// </summary>
		InvalidRegionLocation,

		/// <summary>
		/// A conditional option token found outside of a conditional, e.g. an "else" block with no preceding "if".
		/// </summary>
		OptionOutsideOfConditional,

		/// <summary>
		/// A conditional has no test options prior to encountering a default option.
		/// </summary>
		DefaultOptionStartsConditional,

		/// <summary>
		/// Tokens were not balanced properly, e.g. {A} {B} {/A} {/B}.
		/// </summary>
		UnbalancedTokens,

		/// <summary>
		/// A region is missings its end token.
		/// </summary>
		MissingRegionEnd,

		/// <summary>
		/// A region end token with no matching start token.
		/// </summary>
		UnexpectedRegionEnd,
	}

	/// <summary>
	/// An exception that occurred when creating a template.
	/// </summary>
	public class MergeTemplateException : Exception
	{
		internal MergeTemplateException(IEnumerable<IMergeError> errors)
			: base("Unable to compile template.")
		{
			Errors = errors.ToArray();
		}

		internal MergeTemplateException(string message, IEnumerable<IMergeError> errors)
			: base(message)
		{
			Errors = errors.ToArray();
		}

		/// <summary>
		/// The set of errors that were discovered in the template.
		/// </summary>
		public IMergeError[] Errors { get; private set; }
	}
}
