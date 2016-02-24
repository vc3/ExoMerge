using System.Linq.Expressions;
using ExoMerge.Analysis;
using ExoModel;

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

		public ModelExpression Parse(ModelType sourceType, string text)
		{
			if (sourceType == null)
				return null;

			return sourceType.GetExpression(null, text);
		}

		public virtual ModelType GetResultType(ModelExpression expression)
		{
			return expression.GetResultModelType(UnpackDynamicMemberAccess);
		}
	}
}
