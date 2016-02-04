using System;
using System.Collections.Generic;

namespace ExoMerge.Aspose.UnitTests.Extensions
{
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Take all items, up to and including the given target item.
		/// </summary>
		/// <typeparam name="T">The type of items in the enumerable.</typeparam>
		/// <param name="source">The enumerable to take items from.</param>
		/// <param name="target">The item to stop iterating after.</param>
		/// <returns>An <see cref="IEnumerable&lt;T&gt;"/>.</returns>
		public static IEnumerable<T> TakeUpToItem<T>(this IEnumerable<T> source, T target)
			where T : class
		{
			var foundEnd = false;

			foreach (var item in source)
			{
				yield return item;

				if (item == target)
				{
					foundEnd = true;
					break;
				}
			}

			if (!foundEnd)
				throw new ArgumentOutOfRangeException("target");
		}
	}
}
