namespace ExoMerge.DataAccess
{
	/// <summary>
	/// An object that represents access to source data during the merge process.
	/// </summary>
	/// <typeparam name="TSource">The type of source data.</typeparam>
	/// <typeparam name="TExpression">The type of expression.</typeparam>
	public class DataContext<TSource, TExpression>
		where TExpression : class
	{
		internal DataContext()
		{
		}

		/// <summary>
		/// Gets an object that provides a reference to the parent data context, if one exists.
		/// </summary>
		public Relationship Parent { get; private set; }

		/// <summary>
		/// Gets the index of the value in it's list, if it is an item in a list.
		/// </summary>
		public int? Index { get; private set; }

		/// <summary>
		/// Gets the count of items in the list, if the value is an item in a list.
		/// </summary>
		public int? Count { get; private set; }

		/// <summary>
		/// Gets the source object.
		/// </summary>
		public TSource Source { get; private set; }

		/// <summary>
		/// An enumeration of types of relationships between data contexts.
		/// </summary>
		public enum RelationshipType
		{
			/// <summary>
			/// Indicates that the data context is simply a continuation of the
			/// prior data context. For example, a conditional region evaluated
			/// to true, so the current data context is simply continued.
			/// </summary>
			Continuation,

			/// <summary>
			/// Indicates that the data context represents an item in a list
			/// of data in its parent context.
			/// </summary>
			ListItem
		}

		/// <summary>
		/// Represents a relationship to another data contexts.
		/// </summary>
		public class Relationship
		{
			internal Relationship()
			{
			}

			/// <summary>
			/// The other data context.
			/// </summary>
			public DataContext<TSource, TExpression> Context { get; internal set; }

			/// <summary>
			/// The type of relationship to the other data context.
			/// </summary>
			public RelationshipType RelationshipType { get; internal set; }

			/// <summary>
			/// The expression that was evaluated in the other context
			/// which resulted in the derived data context.
			/// </summary>
			public TExpression Expression { get; internal set; }
		}

		/// <summary>
		/// Create a root data context with the given object.
		/// </summary>
		/// <param name="root">The root data object.</param>
		/// <returns>A data context that represents root-level data.</returns>
		public static DataContext<TSource, TExpression> CreateRoot(TSource root)
		{
			return new DataContext<TSource, TExpression>
			{
				Source = root,
			};
		}

		/// <summary>
		/// Continue the current data context as a result of evaluting the given expression.
		/// </summary>
		/// <param name="expression">The expression that was evaluated.</param>
		/// <returns>A continuation data context.</returns>
		internal DataContext<TSource, TExpression> Continue(TExpression expression)
		{
			return new DataContext<TSource, TExpression>
			{
				Parent = new Relationship
				{
					Context = this,
					RelationshipType = RelationshipType.Continuation,
					Expression = expression,
				},
				Count = Count,
				Index = Index,
				Source = Source,
			};
		}

		/// <summary>
		/// Creates a list item data context.
		/// </summary>
		/// <param name="expression">The expression that was evaluated which resulted in the list.</param>
		/// <param name="items">The list of items.</param>
		/// <param name="item">The item that the context should represent.</param>
		/// <param name="index">The index of the item that the context should represent.</param>
		/// <returns>A list item data context.</returns>
		internal DataContext<TSource, TExpression> CreateListItem(TExpression expression, TSource[] items, TSource item, int index)
		{
			return new DataContext<TSource, TExpression>
			{
				Parent = new Relationship
				{
					Context = this,
					RelationshipType = RelationshipType.ListItem,
					Expression = expression,
				},
				Count = items.Length,
				Index = index,
				Source = item,
			};
		}
	}
}
