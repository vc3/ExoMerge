using ExoModel;
using ExoModel.Json;

namespace ExoMerge.ModelExpressions.UnitTests.Models.Movies
{
	[ModelFormat("[FirstName] [LastName]")]
	public class Person : JsonEntity
	{
		public string FirstName { get; set; }

		public string MiddleName { get; set; }

		public string LastName { get; set; }

		public string PhotoUrl { get; set; }
	}
}
