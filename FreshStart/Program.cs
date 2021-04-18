using System;
using System.Diagnostics;
using System.IO;
using log4net;
using Newtonsoft.Json;

namespace FreshStart
{
	enum RunType
	{
		Full,
		Manual
	}

	class Program
	{
		static readonly ILog log = LogManager.GetLogger(typeof(Program));

		public static Config Config;

		static void Main()
		{
			var runType = GetRunType();
			Config = LoadConfiguration("config.json");

			if (Config == null)
			{
				return;
			}

			var packageRemover = new PackageRemover(runType);
			packageRemover.RemovePackages();

			var reg = new RegistryCleaner(runType);
			reg.PerformCleanup();
			reg.RemoveSuggestedApps();

			if (AskForRestart())
			{
				// Restart in 10 seconds.
				Process.Start("shutdown.exe", "-r -t 10");
				return;
			}
		}

		static RunType GetRunType()
		{
			Console.Write("Would you like to manually select, which features will be disabled/changed? (Y / N): ");

			var input = Console.ReadLine().ToLower();

			if (input is "y" or "n")
			{
				return input is "y" ? RunType.Manual : RunType.Full;
			}

			Console.WriteLine("Invalid input. (Y or N) expected.");
			return GetRunType();
		}

		static bool AskForRestart()
		{
			Console.ForegroundColor = ConsoleColor.Red;

			Console.Write("Would you like to restart your system now? (Y / N): ");
			Console.ResetColor();

			var input = Console.ReadLine().ToLower();

			if (input is "y" or "n")
			{
				return input is "y";
			}

			Console.WriteLine("Invalid input. (Y or N) expected.");
			return AskForRestart();
		}

		static Config LoadConfiguration(string file)
		{
			if (!File.Exists(file))
			{
				throw new FileNotFoundException($"File: {file} does not exist.");
			}

			try
			{
				return JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
			}
			catch (JsonException ex)
			{
				log.Error(ex.ToString());
				return null;
			}
		}
	}
}
