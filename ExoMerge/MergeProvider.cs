using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExoMerge.Analysis;
using ExoMerge.DataAccess;
using ExoMerge.Helpers;
using ExoMerge.Rendering;
using ExoMerge.Structure;
using JetBrains.Annotations;

namespace ExoMerge
{
	/// <summary>
	/// An object which can be used to produce a result from a given template and data source.
	/// </summary>
	/// <typeparam name="TTarget">The type of object that will be merged.</typeparam>
	/// <typeparam name="TToken">The type that represents a token.</typeparam>
	/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
	/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public abstract class MergeProvider<TTarget, TToken, TElement, TSourceType, TSource, TExpression>
		where TToken : class, IToken<TElement, TElement>
		where TSourceType : class
		where TExpression : class
		where TTarget : class
	{
		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="scanner">An object used to find tokens in the template.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		/// <param name="writer">An object used to manipulate the result during merging.</param>
		protected MergeProvider(ITemplateScanner<TTarget, TToken> scanner, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<TTarget, TElement, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider, IMergeWriter<TTarget, TElement, TToken> writer)
		{
			Scanner = scanner;
			TokenParser = tokenParser;
			ExpressionParser = expressionParser;
			DataProvider = dataProvider;
			Writer = writer;
			GeneratorFactory = generatorFactory;
		}

		/// <summary>
		/// Gets the scanner, used to find tokens in the template.
		/// </summary>
		public ITemplateScanner<TTarget, TToken> Scanner { get; private set; }

		/// <summary>
		/// Gets the parser, used to parse and interpret field code found in the template.
		/// </summary>
		public ITokenParser<TSourceType> TokenParser { get; private set; }

		/// <summary>
		/// Gets the parser, used to parse a raw text expression as an expression object.
		/// </summary>
		public IExpressionParser<TSourceType, TExpression> ExpressionParser { get; private set; }

		/// <summary>
		/// Gets the data provider, used to access source data while merging.
		/// </summary>
		public IDataProvider<TSourceType, TSource, TExpression> DataProvider { get; private set; }

		/// <summary>
		/// Gets the adapter, used to manipulate the result during merging.
		/// </summary>
		public IMergeWriter<TTarget, TElement, TToken> Writer { get; private set; }

		/// <summary>
		/// Gets an object that can assign custom generators.
		/// </summary>
		public IGeneratorFactory<IGenerator<TTarget, TElement, TSourceType, TSource, TExpression>> GeneratorFactory { get; private set; }

		/// <summary>
		/// Attempt to compile the given template.
		/// </summary>
		/// <param name="target">The object to compile.</param>
		/// <param name="rootType">The type of the root source.</param>
		/// <param name="errorAction">The desired behavior when and error is encountered.</param>
		/// <param name="root">The root template container.</param>
		/// <param name="errors">The errors that were encountered, if any.</param>
		/// <returns>Whether or not the compile was successful.</returns>
		public bool TryCompile([NotNull] TTarget target, TSourceType rootType, MergeErrorAction errorAction, out Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> root, out MergeTemplateError<TToken>[] errors)
		{
			try
			{
				var compiler = new MergeTemplateCompiler<TTarget, TToken, TTarget, TElement, TSourceType, TSource, TExpression>(Scanner, TokenParser, ExpressionParser, GeneratorFactory, errorAction != MergeErrorAction.Stop);

				var mergeTemplate = compiler.Compile(target, rootType);

				root = mergeTemplate.Root;
				errors = mergeTemplate.Errors.ToArray();
				return true;
			}
			catch (MergeTemplateCompilerStopException e)
			{
				root = null;
				errors = e.Errors.Cast<MergeTemplateError<TToken>>().ToArray();
				return false;
			}
		}

		/// <summary>
		/// Attempt to merge the given template and data source.
		/// </summary>
		/// <param name="target">The target object to merge.</param>
		/// <param name="source">The root data source.</param>
		/// <param name="errorAction">The desired behavior when and error is encountered.</param>
		/// <param name="errors">The errors that were encountered, if any.</param>
		/// <returns>The result of merging the template and data source.</returns>
		public bool TryMerge(TTarget target, TSource source, MergeErrorAction errorAction, out IMergeError[] errors)
		{
			Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> root;
			MergeTemplateError<TToken>[] compilationErrors;

			var rootType = DataProvider.GetSourceType(source);

			if (!TryCompile(target, rootType, errorAction, out root, out compilationErrors))
			{
				errors = compilationErrors.Cast<IMergeError>().ToArray();
				return false;
			}

			if (errorAction == MergeErrorAction.Stop && compilationErrors.Length > 0)
			{
				errors = compilationErrors.Cast<IMergeError>().ToArray();
				return false;
			}

			try
			{
				var errorList = new List<MergeError<TToken>>();

				var context = DataContext<TSource, TExpression>.CreateRoot(source);

				MergeChildren(target, root, context, errorAction, errorList);

				errors = compilationErrors.Concat(errorList).Cast<IMergeError>().ToArray();

				if (errorAction != MergeErrorAction.Ignore && errors.Length > 0)
					throw new MergeException(errors);

				return true;
			}
			catch (MergeStopException e)
			{
				errors = e.Errors;
				return false;
			}
		}

		/// <summary>
		/// Merge the given template and data source.
		/// </summary>
		/// <param name="target">The target object to merge.</param>
		/// <param name="source">The root data source.</param>
		/// <param name="continueOnError">Whether or not the merge should continue when an error is encountered.</param>
		/// <returns>The result of merging the template and data source.</returns>
		public void Merge([NotNull] TTarget target, [NotNull] TSource source, bool continueOnError = false)
		{
			IMergeError[] errors;

			if (!TryMerge(target, source, continueOnError ? MergeErrorAction.Continue : MergeErrorAction.Stop, out errors))
				throw new MergeException(errors);
		}

		/// <summary>
		/// Record the given error, and throw an exception if the error action is 'Stop'.
		/// </summary>
		protected static void RecordError(ICollection<MergeError<TToken>> errorList, MergeErrorAction errorAction, MergeError<TToken> error)
		{
			errorList.Add(error);

			if (errorAction == MergeErrorAction.Stop)
				throw new MergeStopException(errorList, error);
		}

		/// <summary>
		/// Remove the given region from the target.
		/// </summary>
		protected internal virtual void RemoveRegion(TTarget target, IRegion<TToken> region)
		{
			if (region.OwnsStartToken && region.OwnsEndToken)
				Writer.RemoveRegionNodes(target, region.StartToken, region.EndToken, RegionNodes.Start | RegionNodes.End | RegionNodes.Content);
			else if (region.OwnsStartToken)
				Writer.RemoveRegionNodes(target, region.StartToken, region.EndToken, RegionNodes.Start | RegionNodes.Content);
			else if (region.OwnsEndToken)
				Writer.RemoveRegionNodes(target, region.StartToken, region.EndToken, RegionNodes.End | RegionNodes.Content);
			else
				Writer.RemoveRegionNodes(target, region.StartToken, region.EndToken, RegionNodes.Content);
		}

		/// <summary>
		/// Remove the given region's tags from the target.
		/// </summary>
		protected internal virtual void RemoveRegionTags(TTarget target, IRegion<TToken> region)
		{
			if (region.OwnsStartToken && region.OwnsEndToken)
				Writer.RemoveRegionNodes(target, region.StartToken, region.EndToken, RegionNodes.Start | RegionNodes.End);
			else if (region.OwnsStartToken)
				Writer.RemoveRegionNodes(target, region.StartToken, region.EndToken, RegionNodes.Start);
			else if (region.OwnsEndToken)
				Writer.RemoveRegionNodes(target, region.StartToken, region.EndToken, RegionNodes.End);
		}

		/// <summary>
		/// Converts the given object into an array of items of type `TSource`.
		/// </summary>
		protected virtual TSource[] GetItems(object value)
		{
			if (value == null)
				return null;

			if (value is IEnumerable && !(value is string))
				return ((IEnumerable) value).Cast<TSource>().ToArray();

			return new[] {(TSource) value};
		}

		/// <summary>
		/// Merge a repeatable region.
		/// </summary>
		protected virtual void MergeRepeatable(TTarget target, Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression> repeatable, DataContext<TSource, TExpression> context, MergeErrorAction errorAction, ICollection<MergeError<TToken>> errorList)
		{
			TSource[] items;

			if (repeatable.Expression == null)
			{
				RecordError(errorList, errorAction, new RepeatableMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(repeatable, MergeErrorType.DataAccess));
				return;
			}

			try
			{
				var value = DataProvider.GetValue(context, repeatable.Expression);

				// Convert the result of the expression to an array. This may result in evaulating an enumerable,
				// which could reveal an error in the expression that hadn't been encountered yet.
				items = GetItems(value);
			}
			catch (Exception e)
			{
				RecordError(errorList, errorAction, new RepeatableMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(repeatable, MergeErrorType.DataAccess, e));
				return;
			}

			if (items == null || items.Length == 0)
			{
				// If there are no items to repeat, then remove the repeatable in its entirety.
				RemoveRegion(target, repeatable);
			}
			else
			{
				for (var i = 0; i < items.Length; i++)
				{
					var item = items[i];
					var isLast = i == items.Length - 1;

					Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression> targetRepeatable;

					if (isLast)
						targetRepeatable = repeatable;
					else
					{
						var existingTokens = new Tuple<TToken, TToken[], TToken>(repeatable.StartToken, repeatable.GetInnerTokens().ToArray(), repeatable.EndToken);

						Tuple<TToken, TToken[], TToken> newTokens;

						try
						{
							newTokens = Writer.CloneRegion(target, existingTokens);
						}
						catch (MergeStopException)
						{
							throw;
						}
						catch (Exception e)
						{
							RecordError(errorList, errorAction, new RepeatableMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(repeatable, MergeErrorType.Manipulation, e));
							continue;
						}

						targetRepeatable = new Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(newTokens.Item1, newTokens.Item3, repeatable.Expression, repeatable.SourceType);

						// Compile the inner cloned tokens and target the repeatable as the
						// container for the regions and/or fields that they represent.
						MergeTemplateError<TToken>[] errors;
						MergeTemplateCompiler.CompileInto(targetRepeatable, TokenParser, ExpressionParser, newTokens.Item2, GeneratorFactory, true, out errors);
					}

					var itemContext = context.CreateListItem(repeatable.Expression, items, item, i);

					MergeChildren(target, targetRepeatable, itemContext, errorAction, errorList);

					try
					{
						RemoveRegionTags(target, targetRepeatable);
					}
					catch (MergeStopException)
					{
						throw;
					}
					catch (Exception e)
					{
						RecordError(errorList, errorAction, new RepeatableMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(repeatable, MergeErrorType.Manipulation, e));
					}
				}
			}
		}

		/// <summary>
		/// Determines if a conditional region should be rendered based on the given value.
		/// </summary>
		protected virtual bool ShouldRender(object value)
		{
			return ValueConverter.ToBoolean(value);
		}

		/// <summary>
		/// Merge a conditional region.
		/// </summary>
		protected virtual void MergeConditional(TTarget target, Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> conditional, DataContext<TSource, TExpression> context, MergeErrorAction errorAction, ICollection<MergeError<TToken>> errorList)
		{
			var satisfied = false;
			var encounteredError = false;

			for (var currentOption = conditional.FirstOption; currentOption != null; currentOption = currentOption.Next)
			{
				bool shouldRender;
				TExpression expression;

				if (currentOption is Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.TestOption)
				{
					var conditionalTest = (Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.TestOption)currentOption;

					if (conditionalTest.Expression == null)
					{
						encounteredError = true;
						RecordError(errorList, errorAction, new ConditionalTestMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(currentOption, MergeErrorType.DataAccess));
						break;
					}

					try
					{
						var value = DataProvider.GetValue(context, conditionalTest.Expression);

						// Calling 'ShouldRender' may result in evaulating an enumerable, which could
						// reveal an error in the expression that hadn't been encountered yet.
						shouldRender = ShouldRender(value);

						expression = conditionalTest.Expression;
					}
					catch (Exception e)
					{
						encounteredError = true;
						RecordError(errorList, errorAction, new ConditionalTestMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(currentOption, MergeErrorType.DataAccess, e));
						break;
					}
				}
				else if (currentOption is Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.DefaultOption)
				{
					shouldRender = true;
					expression = null;
				}
				else
				{
					shouldRender = false;
					expression = null;
				}

				if (shouldRender)
				{
					satisfied = true;

					MergeChildren(target, currentOption, context.Continue(expression), errorAction, errorList);

					try
					{
						// Remove all options other than the current (satisfying) one.
						for (var option = conditional.FirstOption; option != null; option = option.Next)
						{
							if (option == currentOption)
								RemoveRegionTags(target, option);
							else
								RemoveRegion(target, option);
						}

						// Remove the end token for the conditional.
						RemoveRegionTags(target, conditional);
					}
					catch (MergeStopException)
					{
						throw;
					}
					catch (Exception e)
					{
						RecordError(errorList, errorAction, new ConditionalMergeError<TTarget,TElement,TToken,TSourceType,TSource,TExpression>(conditional, MergeErrorType.Manipulation, e));
					}

					break;
				}
			}

			// If a matching option wasn't found, then remove the region if no error was encountered.
			// Otherwise, the tokens will be left in place so that a caller can decide what to do with it.
			if (!satisfied && !encounteredError)
			{
				try
				{
					RemoveRegion(target, conditional);
				}
				catch (MergeStopException)
				{
					throw;
				}
				catch (Exception e)
				{
					RecordError(errorList, errorAction, new ConditionalMergeError<TTarget,TElement,TToken,TSourceType,TSource,TExpression>(conditional, MergeErrorType.Manipulation, e));
				}
			}
		}

		/// <summary>
		/// Merge a regions children regions.
		/// </summary>
		internal virtual void MergeChildren(TTarget target, Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression> container, DataContext<TSource, TExpression> context, MergeErrorAction errorAction, ICollection<MergeError<TToken>> errorList)
		{
			foreach (var child in container.Children)
			{
				if (child is Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)
				{
					var repeatable = (Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)child;

					try
					{
						MergeRepeatable(target, repeatable, context, errorAction, errorList);
					}
					catch (MergeStopException)
					{
						throw;
					}
					catch (Exception e)
					{
						RecordError(errorList, errorAction, new RepeatableMergeError<TTarget,TElement,TToken,TSourceType,TSource,TExpression>(repeatable, e));
					}
				}
				else if (child is Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)
				{
					var conditional = (Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)child;

					try
					{
						MergeConditional(target, conditional, context, errorAction, errorList);
					}
					catch (MergeStopException)
					{
						throw;
					}
					catch (Exception e)
					{
						RecordError(errorList, errorAction, new ConditionalMergeError<TTarget,TElement,TToken,TSourceType,TSource,TExpression>(conditional, e));
					}
				}
				else if (child is Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)
				{
					var field = (Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)child;

					// Bypass fields with no expression, to avoid an unneccessary NullReferenceException.
					// If the expression is null, then a template error would have already been recorded.
					if (field.Expression != null)
					{
						if (field.Generator != null)
						{
							try
							{
								MergeGeneratorField(target, (Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)child, context, errorAction, errorList);
							}
							catch (MergeStopException)
							{
								throw;
							}
							catch (Exception e)
							{
								RecordError(errorList, errorAction, new GeneratorMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(field, e));
							}
						}
						else
						{
							try
							{
								MergeStandardField(target, (Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)child, context, errorAction, errorList);
							}
							catch (MergeStopException)
							{
								throw;
							}
							catch (Exception e)
							{
								RecordError(errorList, errorAction, new ContentMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(field, e));
							}
						}
					}
				}
				else
					throw new Exception(string.Format("Unexpected child of type '{0}'.", child.GetType().Name));
			}
		}

		/// <summary>
		/// Merge a generator field.
		/// </summary>
		protected virtual void MergeGeneratorField(TTarget target, Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> field, DataContext<TSource, TExpression> context, MergeErrorAction errorAction, ICollection<MergeError<TToken>> errorList)
		{
			var generatedContent = field.Generator.GenerateContent(target, DataProvider, context, field.Expression, field.Options);

			try
			{
				if (generatedContent != null)
					Writer.ReplaceToken(target, field.Token, generatedContent.ToArray());
				else
					Writer.RemoveToken(target, field.Token);
			}
			catch (MergeStopException)
			{
				throw;
			}
			catch (Exception e)
			{
				RecordError(errorList, errorAction, new GeneratorMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(field, MergeErrorType.Manipulation, e));
			}
		}

		/// <summary>
		/// Merge a standard field.
		/// </summary>
		protected virtual void MergeStandardField(TTarget target, Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> field, DataContext<TSource, TExpression> context, MergeErrorAction errorAction, ICollection<MergeError<TToken>> errorList)
		{
			string textValue;

			try
			{
				textValue = GetStandardFieldValue(context, field.Expression, field.Options);
			}
			catch (MergeStopException)
			{
				throw;
			}
			catch(Exception e)
			{
				RecordError(errorList, errorAction, new ContentMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(field, MergeErrorType.DataAccess, e));
				return;
			}

			try
			{
				MergeStandardFieldValue(target, field, textValue);
			}
			catch (MergeStopException)
			{
				throw;
			}
			catch (Exception e)
			{
				RecordError(errorList, errorAction, new ContentMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression>(field, MergeErrorType.Manipulation, e));
				return;
			}
		}

		/// <summary>
		/// Get the merged value for the given expression.
		/// </summary>
		/// <remarks>
		/// Typically, the result of an expression would be entirely dependent on the data provider
		/// and the current data context, but there are cases where the document itself dictates
		/// the value in some way, and this logic should not polute the data provider.
		/// </remarks>
		protected virtual string GetStandardFieldValue(DataContext<TSource, TExpression> context, TExpression expression, KeyValuePair<string, string>[] options)
		{
			string format = null;

			if (options != null)
			{
				var formatOption = options.SingleOrDefault(o => o.Key.Equals("format", StringComparison.InvariantCultureIgnoreCase));

				format = formatOption.Value;
			}

			return DataProvider.GetFormattedValue(context, expression, format, null);
		}

		/// <summary>
		/// Generate nodes for the given value to replace the given standard field.
		/// </summary>
		protected abstract TElement[] GenerateStandardFieldContent(TTarget result, Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> field, string textValue);

		/// <summary>
		/// Merge a standard value field with the given value.
		/// </summary>
		protected virtual void MergeStandardFieldValue(TTarget target, Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> field, string textValue)
		{
			var content = GenerateStandardFieldContent(target, field, textValue);

			Writer.ReplaceToken(target, field.Token, content);
		}
	}

	/// <summary>
	/// Represents an error in rendering a repeatable region.
	/// </summary>
	public class RepeatableMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression> : MergeError<TToken>
		where TToken : class, IToken
		where TExpression : class
	{
		public RepeatableMergeError(Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression> repeatable, Exception exception = null)
			: base(MergeErrorType.Unspecified, repeatable.StartToken, exception)
		{
		}

		public RepeatableMergeError(Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression> repeatable, MergeErrorType errorType, Exception exception = null)
			: base(errorType, repeatable.StartToken, exception)
		{
			Repeatable = repeatable;
		}

		/// <summary>
		/// The repeatable where the error was encountered.
		/// </summary>
		public Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression> Repeatable { get; private set; }
	}

	/// <summary>
	/// Represents an error in rendering a conditional region.
	/// </summary>
	public class ConditionalMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression> : MergeError<TToken>
		where TToken : class, IToken
		where TExpression : class
	{
		public ConditionalMergeError(Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> conditional, Exception exception = null)
			: base(MergeErrorType.Unspecified, conditional.StartToken, exception)
		{
			Conditional = conditional;
		}

		public ConditionalMergeError(Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> conditional, MergeErrorType errorType, Exception exception = null)
			: base(errorType, conditional.StartToken, exception)
		{
			Conditional = conditional;
		}

		/// <summary>
		/// The conditional where the error was encountered.
		/// </summary>
		public Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> Conditional { get; private set; }
	}

	/// <summary>
	/// Represents an error in evaluating a conditional test option.
	/// </summary>
	public class ConditionalTestMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression> : MergeError<TToken>
		where TToken : class, IToken
		where TExpression : class
	{
		public ConditionalTestMergeError(Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.Option option, MergeErrorType errorType, Exception exception = null)
			: base(errorType, option.StartToken, exception)
		{
			Option = option;
		}

		/// <summary>
		/// The conditional test option where the error was encountered.
		/// </summary>
		public Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>.Option Option { get; private set; }
	}

	/// <summary>
	/// Represents an error in merging a generator field.
	/// </summary>
	public class GeneratorMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression> : MergeError<TToken>
		where TToken : class, IToken
		where TExpression : class
	{
		public GeneratorMergeError(Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> field, Exception exception = null)
			: base(MergeErrorType.Unspecified, field.Token, exception)
		{
			Field = field;
		}

		public GeneratorMergeError(Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> field, MergeErrorType errorType, Exception exception = null)
			: base(errorType, field.Token, exception)
		{
			Field = field;
		}

		/// <summary>
		/// The field where the error was encountered.
		/// </summary>
		public Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> Field { get; private set; }
	}

	/// <summary>
	/// Represents an error in merging a content field.
	/// </summary>
	public class ContentMergeError<TTarget, TElement, TToken, TSourceType, TSource, TExpression> : MergeError<TToken>
		where TToken : class, IToken
		where TExpression : class
	{
		public ContentMergeError(Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> field, Exception exception = null)
			: base(MergeErrorType.Unspecified, field.Token, exception)
		{
		}

		public ContentMergeError(Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> field, MergeErrorType errorType, Exception exception = null)
			: base(errorType, field.Token, exception)
		{
			Field = field;
		}

		/// <summary>
		/// The field where the error was encountered.
		/// </summary>
		public Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> Field { get; private set; }
	}

	/// <summary>
	/// An exception that is raised when merging stops immediately due to encountering an error.
	/// </summary>
	public class MergeStopException : MergeException
	{
		internal MergeStopException(IEnumerable<IMergeError> errors, IMergeError lastError)
			: base("Stopped merging due to an error" + (lastError.Exception != null ? ": " + lastError.Exception.Message : ""), errors.ToArray())
		{
			LastError = lastError;
		}

		/// <summary>
		/// The last error that was encountered, which triggered merging to stop.
		/// </summary>
		public IMergeError LastError { get; private set; }
	}
}
