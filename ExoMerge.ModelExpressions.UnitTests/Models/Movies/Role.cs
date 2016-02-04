using System.ComponentModel.DataAnnotations;
using ExoModel;
using ExoModel.Json;

namespace ExoMerge.ModelExpressions.UnitTests.Models.Movies
{
	[ModelFormat("[Actor] played [Name] in [Movie]")]
	public class Role : JsonEntity
	{
		public Actor Actor { get; set; }
		
		public Movie Movie { get; set; }

		public string Name { get; set; }

		public int Order { get; set; }

		public bool Star { get; set; }

		public bool Lead { get; set; }

		[DisplayFormat(DataFormatString = "C")]
		public double Earnings { get; set; }
	}
}
