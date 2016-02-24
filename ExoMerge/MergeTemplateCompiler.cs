using System;
using System.Collections.Generic;
using System.Linq;
using ExoMerge.Analysis;
using ExoMerge.Rendering;
using ExoMerge.Structure;

namespace ExoMerge
{
	/// <summary>
	/// Compiles tokens into a hierarchy of region and field objects.
	/// </summary>
	public static class MergeTemplateCompiler
	{
		/// <summary>
		/// Parses and analyzes the given tokens and returns a root region which contains the appropriate region and field objects. 
		/// </summary>
		/// <typeparam name="TTarget">The type of object that the compiled template will create.</typeparam>
		/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
		/// <typeparam name="TToken">The type that represents a token.</typeparam>
		/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
		/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
		/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
		/// <param name="rootType">The root source type.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="tokens">The tokens to compile.</param>
		/// <returns>A root region object that represents the compiled template.</returns>
		public static Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> Compile<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(TSourceType rootType, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IEnumerable<TToken> tokens)
			where TToken : class, IToken
			where TSourceType : class
			where TExpression : class
		{
			Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> root;

			MergeTemplateError<TToken>[] errors;
			if (!Compile(rootType, tokenParser, expressionParser, tokens, null, false, out root, out errors) || errors.Length > 0)
				throw new MergeTemplateException(errors);

			return root;
		}

		/// <summary>
		/// Parses and analyzes the given tokens and returns a root region which contains the appropriate region and field objects, as well as any errors that were encountered.
		/// </summary>
		/// <typeparam name="TTarget">The type of object that the compiled template will create.</typeparam>
		/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
		/// <typeparam name="TToken">The type that represents a token.</typeparam>
		/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
		/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
		/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
		/// <param name="target">The target object to add regions and fields to.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="tokens">The tokens to compile.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="continueOnError">Whether or not to continue when an error is encountered.</param>
		/// <param name="errors">The compilation errors that were encountered.</param>
		internal static void CompileInto<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> target, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IEnumerable<TToken> tokens, IGeneratorFactory<IGenerator<TTarget, TElement, TSourceType, TSource, TExpression>> generatorFactory, bool continueOnError, out MergeTemplateError<TToken>[] errors)
			where TToken : class, IToken
			where TSourceType : class
			where TExpression : class
		{
			MergeTemplateCompiler<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.CompileInto(target, tokenParser, expressionParser, tokens, generatorFactory, continueOnError, out errors);
		}

		/// <summary>
		/// Parses and analyzes the given tokens and returns a root region which contains the appropriate region and field objects, as well as any errors that were encountered.
		/// </summary>
		/// <typeparam name="TTarget">The type of object that the returned template will create.</typeparam>
		/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
		/// <typeparam name="TToken">The type that represents a token.</typeparam>
		/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
		/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
		/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
		/// <param name="rootType">The root type.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the tokens' raw expression text.</param>
		/// <param name="tokens">The tokens to compile.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="continueOnError">Whether or not to continue when an error is encountered.</param>
		/// <param name="root">The root region that represents the compiled template.</param>
		/// <param name="errors">The errors that were encountered, if any.</param>
		/// <returns>Whether or not compilation completed.</returns>
		public static bool Compile<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(TSourceType rootType, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IEnumerable<TToken> tokens, IGeneratorFactory<IGenerator<TTarget, TElement, TSourceType, TSource, TExpression>> generatorFactory, bool continueOnError, out Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> root, out MergeTemplateError<TToken>[] errors)
			where TToken : class, IToken
			where TSourceType : class
			where TExpression : class
		{
			root = new Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(rootType);
			return MergeTemplateCompiler<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.CompileInto(root, tokenParser, expressionParser, tokens, generatorFactory, continueOnError, out errors);
		}
	}

	/// <summary>
	/// Compiles tokens into a hierarchy of region and field objects.
	/// </summary>
	/// <typeparam name="TTarget">The type of object that will be created.</typeparam>
	/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
	/// <typeparam name="TToken">The type that represents a token.</typeparam>
	/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	internal static class MergeTemplateCompiler<TTarget, TElement, TToken, TSourceType, TSource, TExpression>
		where TToken : class, IToken
		where TSourceType : class
		where TExpression : class
	{
		/// <summary>
		/// Parses and analyzes the given tokens and adds the appropriate region and field objects to the given container object. 
		/// </summary>
		/// <param name="target">The target object to add regions and fields to.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the tokens' raw expression text.</param>
		/// <param name="tokens">The tokens to compile.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="continueOnError">Whether or not to continue when an error is encountered.</param>
		/// <param name="errors">The compilation errors that were encountered.</param>
		public static bool CompileInto(Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> target, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IEnumerable<TToken> tokens, IGeneratorFactory<IGenerator<TTarget, TElement, TSourceType, TSource, TExpression>> generatorFactory, bool continueOnError, out MergeTemplateError<TToken>[] errors)
		{
			try
			{
				var currentType = target.SourceType;

				var errorList = new List<MergeTemplateError<TToken>>();

				var openRegions = new Stack<IBuildableRegion<TToken>>();

				foreach (var token in tokens)
				{
					TokenParseResult parsed;

					try
					{
						parsed = tokenParser.Parse(currentType, token.Value);

						if (parsed == null)
							continue;
					}
					catch (Exception e)
					{
						RecordError(errorList, continueOnError, MergeTemplateErrorType.UnableToParseToken, token, TokenType.Unknown, openRegions, e);
						continue;
					}

					switch (parsed.Type)
					{
						case TokenType.ContentField:

							var regionToAddFieldTo = GetCurrentContainer(target, openRegions);
							if (regionToAddFieldTo == null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.InvalidFieldLocation, token, parsed.Type, openRegions);
								continue;
							}

							var value = parsed.Value;

							IGenerator<TTarget, TElement, TSourceType, TSource, TExpression> customGenerator = null;

							if (generatorFactory != null)
							{
								string remainder;
								customGenerator = generatorFactory.GetGenerator(value, out remainder);
								if (customGenerator != null)
									value = remainder;
							}

							TExpression fieldExpression;

							if (string.IsNullOrEmpty(value))
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.MissingExpression, token, parsed.Type, openRegions);

								fieldExpression = null;
							}
							else if (currentType == null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.UnknownDataType, token, parsed.Type, openRegions);

								fieldExpression = null;
							}
							else
							{
								try
								{
									var expectedType = customGenerator != null ? customGenerator.ExpectedType :
														parsed.Options != null && parsed.Options.Any(o => o.Key.Equals("format", StringComparison.InvariantCultureIgnoreCase)) ? null : typeof(string);
								
									var expr = expressionParser.Parse(currentType, value, expectedType);

									if (expr == null)
									{
										RecordError(errorList, continueOnError, MergeTemplateErrorType.UnableToParseExpression, token, parsed.Type, openRegions);
										continue;
									}

									fieldExpression = expr;
								}
								catch (Exception e)
								{
									RecordError(errorList, continueOnError, MergeTemplateErrorType.UnableToParseExpression, token, parsed.Type, openRegions, e);

									fieldExpression = null;
								}
							}

							var contentField = new Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(token, fieldExpression, parsed.Options);

							if (customGenerator != null)
								contentField.Generator = customGenerator;

							regionToAddFieldTo.AppendChildField(contentField);

							break;

						case TokenType.RepeatableBegin:

							// Record errors (but continue parsing) if the token was parsed as a repeatable,
							// but had no expression or the expression couldn't be parsed.

							TExpression repeatableExpression;
							TSourceType repeatableItemType;

							if (string.IsNullOrEmpty(parsed.Value))
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.MissingExpression, token, parsed.Type, openRegions);

								repeatableExpression = null;
								repeatableItemType = null;
							}
							else if (currentType == null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.UnknownDataType, token, parsed.Type, openRegions);

								repeatableExpression = null;
								repeatableItemType = null;
							}
							else
							{
								try
								{
									var expr = expressionParser.Parse(currentType, parsed.Value, null);

									if (expr == null)
									{
										RecordError(errorList, continueOnError, MergeTemplateErrorType.UnableToParseExpression, token, parsed.Type, openRegions);

										repeatableExpression = null;
										repeatableItemType = null;
									}
									else
									{
										repeatableExpression = expr;
										repeatableItemType = expressionParser.GetResultType(expr);
									}
								}
								catch (Exception e)
								{
									RecordError(errorList, continueOnError, MergeTemplateErrorType.UnableToParseExpression, token, parsed.Type, openRegions, e);

									repeatableExpression = null;
									repeatableItemType = null;
								}
							}

							var newRegion = new Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(token, repeatableExpression, repeatableItemType);

							// Add the region to the stack, even if an error occurs, so that region tokens can be balanced
							// and the error for child fields can be reported as "unknown source type".
							openRegions.Push(newRegion);

							currentType = repeatableItemType;

							break;

						case TokenType.RepeatableEnd:

							if (openRegions.Count == 0)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.UnexpectedRegionEnd, token, parsed.Type, openRegions);
								continue;
							}

							if (!(openRegions.Peek() is Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression>))
							{
								if (openRegions.OfType<Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression>>().Any())
									RecordError(errorList, continueOnError, MergeTemplateErrorType.UnbalancedTokens, token, parsed.Type, openRegions);
								else
									RecordError(errorList, continueOnError, MergeTemplateErrorType.UnexpectedRegionEnd, token, parsed.Type, openRegions);
								continue;
							}

							var repeatableToEnd = (Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)openRegions.Pop();

							CloseRegion(repeatableToEnd, token, true);

							var regionToAddRepeatableTo = GetCurrentContainer(target, openRegions);

							if (regionToAddRepeatableTo == null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.InvalidRegionLocation, token, parsed.Type, openRegions);
								continue;
							}

							regionToAddRepeatableTo.AppendChildRegion(repeatableToEnd);

							currentType = regionToAddRepeatableTo.SourceType;

							break;

						case TokenType.ConditionalBegin:

							openRegions.Push(new Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(token, currentType));

							break;

						case TokenType.ConditionalBeginWithTest:

							var newConditional = new Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(token, currentType);

							openRegions.Push(newConditional);

							// Record errors (but continue parsing) if the token was parsed as a conditional,
							// but had no expression or the expression couldn't be parsed.

							TExpression firstTestExpression;

							if (string.IsNullOrEmpty(parsed.Value))
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.MissingExpression, token, parsed.Type, openRegions);

								firstTestExpression = null;
							}
							else if (currentType == null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.UnknownDataType, token, parsed.Type, openRegions);

								firstTestExpression = null;
							}
							else
							{
								try
								{
									var expr = expressionParser.Parse(currentType, parsed.Value, null);

									if (expr == null)
									{
										RecordError(errorList, continueOnError, MergeTemplateErrorType.UnableToParseExpression, token, parsed.Type, openRegions);

										firstTestExpression = null;
									}
									else
									{
										firstTestExpression = expr;
									}
								}
								catch (Exception e)
								{
									RecordError(errorList, continueOnError, MergeTemplateErrorType.UnableToParseExpression, token, parsed.Type, openRegions, e);

									firstTestExpression = null;
								}
							}

							openRegions.Push(new Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.TestOption(newConditional, token, firstTestExpression, false));

							break;

						case TokenType.ConditionalTest:

							if (openRegions.Count == 0)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.OptionOutsideOfConditional, token, parsed.Type, openRegions);
								continue;
							}

							// Record errors (but continue parsing) if the token was parsed as a conditional test,
							// but had no expression or the expression couldn't be parsed.

							TExpression alternateTestExpression;

							if (string.IsNullOrEmpty(parsed.Value))
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.MissingExpression, token, parsed.Type, openRegions);

								alternateTestExpression = null;
							}
							else
							{
								try
								{
									var expr = expressionParser.Parse(currentType, parsed.Value, null);

									if (expr == null)
									{
										RecordError(errorList, continueOnError, MergeTemplateErrorType.UnableToParseExpression, token, parsed.Type, openRegions);

										alternateTestExpression = null;
									}
									else
									{
										alternateTestExpression = expr;
									}
								}
								catch (Exception e)
								{
									RecordError(errorList, continueOnError, MergeTemplateErrorType.UnableToParseExpression, token, parsed.Type, openRegions, e);

									alternateTestExpression = null;
								}
							}

							var conditionalToAddTo = openRegions.Peek() as Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>;
							if (conditionalToAddTo == null)
							{
								var priorOption = openRegions.Peek() as Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.Option;
								if (priorOption == null)
								{
									RecordError(errorList, continueOnError, MergeTemplateErrorType.OptionOutsideOfConditional, token, parsed.Type, openRegions);
									continue;
								}

								CloseRegion(priorOption, token, false);

								conditionalToAddTo = priorOption.Conditional;
							}

							openRegions.Push(new Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.TestOption(conditionalToAddTo, token, alternateTestExpression, true));

							break;

						case TokenType.ConditionalDefault:

							if (openRegions.Count == 0)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.OptionOutsideOfConditional, token, parsed.Type, openRegions);
								continue;
							}

							var conditionalToDefault = openRegions.Peek() as Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>;
							if (conditionalToDefault != null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.OptionOutsideOfConditional, token, parsed.Type, openRegions);
								continue;
							}

							var optionPriorToDefault = openRegions.Peek() as Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.Option;
							if (optionPriorToDefault == null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.DefaultOptionStartsConditional, token, parsed.Type, openRegions);
								continue;
							}

							CloseRegion(optionPriorToDefault, token, false);

							conditionalToDefault = optionPriorToDefault.Conditional;

							openRegions.Push(new Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.DefaultOption(conditionalToDefault, token));

							break;

						case TokenType.ConditionalEnd:

							if (openRegions.Count == 0)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.UnexpectedRegionEnd, token, parsed.Type, openRegions);
								continue;
							}

							Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> conditionalToEnd = null;

							IBuildableRegion<TToken> nonConditionalBeforeConditional = null;

							while (openRegions.Count > 0)
							{
								var block = openRegions.Pop();

								if (block is Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)
								{
									conditionalToEnd = (Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)block;
									break;
								}

								if (nonConditionalBeforeConditional == null && !(block is Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.Option))
									nonConditionalBeforeConditional = block;
							}

							if (conditionalToEnd == null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.UnexpectedRegionEnd, token, parsed.Type, openRegions);
								continue;
							}

							if (nonConditionalBeforeConditional != null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.UnbalancedTokens, token, parsed.Type, openRegions);
								continue;
							}

							CloseRegion(conditionalToEnd.LastOption, token, false);
							CloseRegion(conditionalToEnd, token, true);

							var regionToAddConditionalTo = GetCurrentContainer(target, openRegions);

							if (regionToAddConditionalTo == null)
							{
								RecordError(errorList, continueOnError, MergeTemplateErrorType.InvalidRegionLocation, token, parsed.Type, openRegions);
								continue;
							}

							regionToAddConditionalTo.AppendChildRegion(conditionalToEnd);

							break;

						default:

							RecordError(errorList, continueOnError, MergeTemplateErrorType.UnknownTokenType, token, parsed.Type, openRegions);

							break;

					}
				}

				errorList.AddRange(openRegions
					// Exclude conditional options b/c they are implicitly closed.
					.Where(r => !(r is Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.Option))
					.Select(r => new MergeTemplateError<TToken>(MergeTemplateErrorType.MissingRegionEnd, r.StartToken, TokenType.Unknown, r))
					);

				errors = errorList.ToArray();
				return true;
			}
			catch (MergeTemplateCompilerStopException e)
			{
				errors = e.Errors.Cast<MergeTemplateError<TToken>>().ToArray();
				return false;
			}
		}

		private static IRegion<TToken> GetCurrentRegion(Stack<IBuildableRegion<TToken>> openRegions)
		{
			if (openRegions.Count == 0)
				return null;

			return openRegions.Peek();
		}

		private static Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> GetCurrentContainer(Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> rootContainer, Stack<IBuildableRegion<TToken>> openRegions)
		{
			var currentRegion = GetCurrentRegion(openRegions);

			if (currentRegion != null)
				return currentRegion as Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression>;

			return rootContainer;
		}

		/// <summary>
		/// Close the region with the given token.
		/// </summary>
		private static void CloseRegion(IBuildableRegion<TToken> region, TToken token, bool ownsToken)
		{
			if (region.EndToken != null)
				throw new InvalidOperationException("The region's end token has already been established.");

			region.End(token, ownsToken);
		}

		private static void RecordError(ICollection<MergeTemplateError<TToken>> errorList, bool continueOnError, MergeTemplateErrorType errorType, TToken token, TokenType tokenType, Stack<IBuildableRegion<TToken>> openRegions, Exception exception = null)
		{
			var error = new MergeTemplateError<TToken>(errorType, token, tokenType, GetCurrentRegion(openRegions), exception);

			errorList.Add(error);

			if (!continueOnError)
				throw new MergeTemplateCompilerStopException(errorList, error);
		}
	}

	/// <summary>
	/// Compiles tokens into a hierarchy of region and field objects.
	/// </summary>
	/// <typeparam name="TTemplate">The type that represents a template.</typeparam>
	/// <typeparam name="TToken">The type of tokens to compile.</typeparam>
	/// <typeparam name="TTarget">The type of object that will be created.</typeparam>
	/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
	/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of expression that tokens are parsed as.</typeparam>
	internal class MergeTemplateCompiler<TTemplate, TToken, TTarget, TElement, TSourceType, TSource, TExpression>
		where TToken : class, IToken
		where TSourceType : class
		where TExpression : class
	{
		/// <summary>
		/// Creates a new merge template compiler.
		/// </summary>
		/// <param name="scanner">An object used to find tokens in the document.</param>
		/// <param name="tokenParser">An object used to parse and interpret field code found in the document.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="continueOnError">Whether or not to continue when an error is encountered.</param>
		public MergeTemplateCompiler(ITemplateScanner<TTemplate, TToken> scanner, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<TTarget, TElement, TSourceType, TSource, TExpression>> generatorFactory, bool continueOnError)
		{
			Scanner = scanner;
			TokenParser = tokenParser;
			ExpressionParser = expressionParser;
			GeneratorFactory = generatorFactory;
			ContinueOnError = continueOnError;
		}

		/// <summary>
		/// Gets an object used to find tokens in the document.
		/// </summary>
		public ITemplateScanner<TTemplate, TToken> Scanner { get; private set; }

		/// <summary>
		/// Gets an object used to parse and interpret field code found in the document.
		/// </summary>
		public ITokenParser<TSourceType> TokenParser { get; private set; }

		/// <summary>
		/// Gets an object used to parse and interpret field code found in the document.
		/// </summary>
		public IExpressionParser<TSourceType, TExpression> ExpressionParser { get; private set; }

		/// <summary>
		/// Gets an object that can assign custom generators.
		/// </summary>
		public IGeneratorFactory<IGenerator<TTarget, TElement, TSourceType, TSource, TExpression>> GeneratorFactory { get; private set; }

		/// <summary>
		/// Gets a value indicating whether or not the compiler should continue when an error is encountered.
		/// </summary>
		public bool ContinueOnError { get; private set; }

		/// <summary>
		/// Compile the given template and return a merge template.
		/// </summary>
		internal MergeTemplate<TTemplate, TToken, TTarget, TElement, TSourceType, TSource, TExpression> Compile(TTemplate template, TSourceType rootType)
		{
			var root = new Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(rootType);

			var tokens = Scanner.GetTokens(template).ToArray();

			MergeTemplateError<TToken>[] errors;
			MergeTemplateCompiler<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.CompileInto(root, TokenParser, ExpressionParser, tokens, GeneratorFactory, ContinueOnError, out errors);

			return new MergeTemplate<TTemplate, TToken, TTarget, TElement, TSourceType, TSource, TExpression>(template, tokens, root, errors);
		}
	}

	/// <summary>
	/// An exception that is raised when compiling stops immediately due to encountering an error.
	/// </summary>
	public class MergeTemplateCompilerStopException : MergeTemplateException
	{
		internal MergeTemplateCompilerStopException(IEnumerable<IMergeError> errors, IMergeError lastError)
			: base("Stopped compiling due to an error" + (lastError.Exception != null ? ": " + lastError.Exception.Message : ""), errors.ToArray())
		{
			LastError = lastError;
		}

		/// <summary>
		/// The last error that was encountered, which triggered compilation to stop.
		/// </summary>
		public IMergeError LastError { get; private set; }
	}
}
