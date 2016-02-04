using System;
using System.ComponentModel;
using System.Reflection;
using ExoMerge.DataAccess;

namespace ExoMerge.UnitTests.Common
{
	public class SimpleDataProvider : IDataProvider<Type, object, string>
	{
		public static object Evaluate(object source, string text)
		{
			var value = source;

			foreach (var step in text.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (value == null)
					return null;

				string customClassName = null;

				if (value is ICustomTypeDescriptor)
				{
					customClassName = ((ICustomTypeDescriptor)value).GetClassName();

					PropertyDescriptor customProperty = null;
					foreach (PropertyDescriptor property in ((ICustomTypeDescriptor)value).GetProperties())
					{
						if (property.Name == step)
						{
							customProperty = property;
							break;
						}
					}

					if (customProperty != null)
					{
						value = customProperty.GetValue(value);
						continue;
					}
				}

				var builtInProperty = value.GetType().GetProperty(step, BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public);
				if (builtInProperty == null)
					throw new Exception("Unknown property '" + step + "' of type '" + (customClassName ?? value.GetType().Name) + "'.");

				value = builtInProperty.GetValue(value, null);
			}

			return value;
		}

		public Type GetSourceType(object source)
		{
			return source.GetType();
		}

		public object GetValue(DataContext<object, string> context, string expression)
		{
			return Evaluate(context.Source, expression);
		}

		public string GetFormattedValue(DataContext<object, string> context, string expression, string format, IFormatProvider provider, out object rawValue)
		{
			rawValue = Evaluate(context.Source, expression);

			if (rawValue == null)
				return string.Empty;

			if (format != null)
			{
				var formattable = rawValue as IFormattable;
				if (formattable == null)
					throw new Exception(string.Format("Objects of type '{0}' are not formattable.", rawValue.GetType().Name));

				return formattable.ToString(format, null);
			}

			return Convert.ToString(rawValue);
		}
	}
}
