using System.Linq.Expressions;
using ExoMerge.Analysis;
using ExoModel;
using System;

namespace ExoMerge.ModelExpressions
{
	/// <summary>
	/// Parses expression text as a <see cref="ModelExpression"/>.
	/// </summary>
	public class ModelExpressionParser : IExpressionParser<ModelType, ModelExpression>
	{
		protected virtual ModelProperty UnpackDynamicMemberAccess(UnaryExpression expression)
		{
			return null;
		}

		public ModelExpression Parse(ModelType sourceType, string text, Type resultType)
		{
			if (sourceType == null)
				return null;

			return sourceType.GetExpression(resultType, text);
		}

		public virtual ModelType GetResultType(ModelExpression expression)
		{
			return expression.GetResultModelType(UnpackDynamicMemberAccess);
		}
	}
}
