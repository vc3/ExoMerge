using ExoMerge.Analysis;
using ExoModel;

namespace ExoMerge.ModelExpressions
{
	/// <summary>
	/// Parses expression text as a <see cref="ModelExpression"/>.
	/// </summary>
	public class ModelExpressionParser : IExpressionParser<ModelType, ModelExpression>
	{
		protected virtual ModelType GetResultModelType(ModelType sourceType, ModelExpression expression)
		{
			return expression.GetResultModelType();
		}

		public ModelExpression Parse(ModelType sourceType, string text)
		{
			if (sourceType == null)
				return null;

			return sourceType.GetExpression(null, text);
		}

		public ModelType GetResultType(ModelExpression expression)
		{
			return GetResultModelType(expression.RootType, expression);
		}
	}
}
