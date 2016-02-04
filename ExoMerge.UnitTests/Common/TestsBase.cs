using System;
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.UnitTests.Common
{
	public abstract class TestsBase
	{
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		public Guid Id { get; private set; }

		protected virtual void OnBeforeTest()
		{
		}

		[TestInitialize]
		public void BeforeTest()
		{
			Id = Guid.NewGuid();

			OnBeforeTest();
		}

		[NotNull]
		protected static string GetProjectDirectory()
		{
			var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
			var assemblyDirectory = Path.GetDirectoryName(new Uri(assembly.CodeBase).AbsolutePath);
			if (assemblyDirectory == null)
				throw new Exception("Could not determine the location of assembly '" + assembly.GetName() + "'.");

			var assemblyDirectoryName = Path.GetFileName(assemblyDirectory);

			var assemblyParentDirectory = Path.GetDirectoryName(assemblyDirectory);
			if (assemblyParentDirectory == null)
				throw new Exception("Executing assembly in unexpected location '" + assemblyDirectory + "'.");

			var assemblyParentDirectoryName = Path.GetFileName(assemblyParentDirectory);

			if (assemblyParentDirectoryName == "bin")
			{
				// i.e. "\bin\Debug\*.dll"
				var dir = Path.GetDirectoryName(assemblyParentDirectory);
				if (dir == null)
					throw new Exception("Executing assembly in unexpected location '" + assemblyDirectory + "'.");
				return dir;
			}

			if (assemblyDirectoryName == "bin")
			{
				// i.e. "\bin\*.dll"
				return assemblyParentDirectory;
			}

			var assemblyGrandparentDirectory = Path.GetDirectoryName(assemblyParentDirectory);
			var assemblyGrandparentDirectoryName = Path.GetFileName(assemblyGrandparentDirectory);

			if (assemblyDirectoryName == "Out" && assemblyGrandparentDirectoryName == "TestResults")
			{
				// \TestResults\Deploy_username yyyy-MM-dd hh_mm_ss\Out\*.dll
				var appDirectory = Path.GetDirectoryName(assemblyGrandparentDirectory);
				if (appDirectory == null)
					throw new Exception("Found test files in unexpected location '" + assemblyDirectory + "'.");

				return Path.Combine(appDirectory, assembly.GetName().Name);
			}

			throw new Exception("Executing assembly in unexpected location '" + assemblyDirectory + "'.");
		}
	}
}
