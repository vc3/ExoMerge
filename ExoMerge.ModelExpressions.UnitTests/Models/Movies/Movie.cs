using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExoModel;
using ExoModel.Json;

namespace ExoMerge.ModelExpressions.UnitTests.Models.Movies
{
	[ModelFormat("[Name] ([Year])")]
	public class Movie : JsonEntity
	{
		public string Name { get; set; }

		[DisplayFormat(DataFormatString = "g")]
		public int Year { get; set; }

		public long YearsInPlanning { get; set; }

		public float YearsInProduction { get; set; }

		public decimal Budget { get; set; }

		public string Rated { get; set; }

		[DisplayFormat(DataFormatString = "d")]
		public DateTime Started { get; set; }

		[DisplayFormat(DataFormatString="d")]
		public DateTime Released { get; set; }

		public ICollection<Genre> Genres { get; set; }

		public Director Director { get; set; }

		public ICollection<Role> Roles { get; set; }

		public string PosterUrl { get; set; }
	}
}
