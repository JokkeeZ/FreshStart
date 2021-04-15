using System;
using System.Diagnostics;
using System.IO;
using log4net;
using Newtonsoft.Json;

namespace FreshStart
{
	class Program
	{
		static readonly ILog log = LogManager.GetLogger(typeof(Program));

		public static Config Config;

		static void Main()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("Please configure `config.json` file before running this software.\r\n" +
						  "Would you like to continue? (Y / N): ");
			Console.ResetColor();

			var confirm = Console.ReadLine();

			if (confirm is not "y" and not "Y")
			{
				return;
			}

			Console.WriteLine();

			Config = LoadConfiguration("config.json");

			if (Config == null)
			{
				return;
			}

			var packageRemover = new PackageRemover();
			packageRemover.RemovePackages();

			var reg = new RegistryCleaner();
			reg.PerformCleanup();
			reg.RemoveSuggestedApps();

			if (AskForRestart())
			{
				// Restart in 10 seconds.
				Process.Start("shutdown.exe", "-r -t 10");
				return;
			}
		}

		static bool AskForRestart()
		{
			Console.Write("Would you like to restart your system now? (Y / N): ");
			var input = Console.ReadLine();

			if (input is "Y" or "N" or "y" or "n")
			{
				return input is "Y" or "y";
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
