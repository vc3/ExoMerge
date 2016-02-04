using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ExoMerge.Structure
{
	/// <summary>
	/// Represents a conditional region in a document.
	/// </summary>
	/// <typeparam name="TTarget">The type of object that the conditional's rendered content will apply to.</typeparam>
	/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
	/// <typeparam name="TToken">The type that represents tokens in the document.</typeparam>
	/// <typeparam name="TSourceType">The type that identifies the type of the data source, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the data source that the conditional's content will be rendered for, e.g. 'Type'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public class Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> : IBuildableRegion<TToken>
		where TToken : class
		where TExpression : class
	{
		private readonly LinkedList<Option> options = new LinkedList<Option>();

		internal Conditional([NotNull] TToken startToken, [CanBeNull] TSourceType contextType)
		{
			StartToken = startToken;
			OwnsStartToken = true;
			ContextType = contextType;
		}

		/// <summary>
		/// The token that marks the start of the conditional.
		/// </summary>
		[NotNull] 
		public TToken StartToken { get; private set; }

		/// <summary>
		/// Indicates whether the region owns its start token.
		/// </summary>
		public bool OwnsStartToken { get; private set; }

		/// <summary>
		/// The context type that conditional expressions will be evaluated against.
		/// </summary>
		[CanBeNull]
		public TSourceType ContextType { get; private set; }

		/// <summary>
		/// The token that marks the end of the conditional.
		/// </summary>
		public TToken EndToken { get; private set; }

		/// <summary>
		/// Indicates whether the region owns its end token.
		/// </summary>
		public bool OwnsEndToken { get; private set; }

		/// <summary>
		/// Gets the set of options in the conditonal.
		/// </summary>
		public IEnumerable<Option> Options
		{
			get { return options; }
		}

		/// <summary>
		/// Gets the first option in the conditional.
		/// </summary>
		public Option FirstOption
		{
			get { return options.First.Value; }
		}

		/// <summary>
		/// Gets the last option in the conditional.
		/// </summary>
		public Option LastOption
		{
			get { return options.Last.Value; }
		}

		/// <summary>
		/// Sets the token that marks the end of the region.
		/// </summary>
		void IBuildableRegion<TToken>.End(TToken token, bool ownsToken)
		{
			EndToken = token;
			OwnsEndToken = ownsToken;
		}

		internal void AddOption(Option option)
		{
			if (options.Count > 0)
			{
				if (LastOption is DefaultOption)
					throw new InvalidOperationException("Cannot add a conditional option after the default option.");

				var previousOption = LastOption;
				option.Previous = previousOption;
				previousOption.Next = option;
			}
			else if (!(option is TestOption))
				throw new InvalidOperationException("The first conditional option must be a test option.");

			options.AddLast(option);
		}

		/// <summary>
		/// Gets the tokens between the region's start and end tokens.
		/// </summary>
		public IEnumerable<TToken> GetInnerTokens()
		{
			foreach (var c in options)
			{
				if (c.StartToken != StartToken)
					yield return c.StartToken;

				foreach (var childToken in c.GetInnerTokens())
					yield return childToken;

				if (c.Next == null)
				{
					if (c.EndToken != EndToken)
						yield return c.EndToken;
				}
				else if (c.EndToken != c.Next.StartToken)
					yield return c.EndToken;
			}
		}

		public abstract class Option : Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression>, IBuildableRegion<TToken>
		{
			protected Option([NotNull] Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> conditional, [NotNull] TToken token, bool ownsToken)
				: base(conditional.ContextType)
			{
				StartToken = token;
				OwnsStartToken = ownsToken;
				Conditional = conditional;
				Conditional.AddOption(this);
			}

			/// <summary>
			/// The token that marks the start of the option.
			/// </summary>
			[NotNull]
			public TToken StartToken { get; private set; }

			/// <summary>
			/// Indicates whether the region owns its start token.
			/// </summary>
			public bool OwnsStartToken { get; private set; }

			/// <summary>
			/// The token that marks the end of the option.
			/// </summary>
			public TToken EndToken { get; private set; }

			/// <summary>
			/// Indicates whether the region owns its end token.
			/// </summary>
			public bool OwnsEndToken { get; private set; }

			/// <summary>
			/// Gets the parent conditional.
			/// </summary>
			[NotNull]
			public Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> Conditional { get; private set; }

			/// <summary>
			/// Gets the previous option in the conditional.
			/// </summary>
			public Option Previous { get; internal set; }

			/// <summary>
			/// Gets the next option in the conditional.
			/// </summary>
			public Option Next { get; internal set; }

			/// <summary>
			/// Sets the token that marks the end of the region.
			/// </summary>
			void IBuildableRegion<TToken>.End(TToken token, bool ownsToken)
			{
				EndToken = token;
				OwnsEndToken = ownsToken;
			}
		}

		public class TestOption : Option
		{
			internal TestOption([NotNull] Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> conditional, [NotNull] TToken token, [CanBeNull] TExpression expression, bool ownsToken)
				: base(conditional, token, ownsToken)
			{
				Expression = expression;
			}

			/// <summary>
			/// Gets the conditional expression to test.
			/// </summary>
			[CanBeNull]
			public TExpression Expression { get; private set; }
		}

		public class DefaultOption : Option
		{
			internal DefaultOption([NotNull] Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression> conditional, [NotNull] TToken token)
				: base(conditional, token, true)
			{
			}
		}
	}
}
