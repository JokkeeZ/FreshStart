using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace FreshStart
{
	class Program
	{
		public static Config Config;

		[STAThread]
		static void Main()
		{
			Config = LoadConfiguration();

			if (Config == null)
			{
				return;
			}

			var packageRemover = new PackageRemover();
			packageRemover.RemovePackages();

			var reg = new RegistryCleaner();
			reg.PerformCleanup();
			reg.RemoveSuggestedApps();

			using var logger = new Logger();

			logger.Write("Main", "Completed. Your OS now has an fresh start.");
			logger.Write("Main", "To complete cleanup and stop running services, which has been disabled, system restart is required.");
			logger.Write("Main", "Asking for restart... Waiting for user input.");

			if (AskForRestart())
			{
				// Restart in 10 seconds.
				Process.Start("shutdown.exe", "-r -t 10");

				logger.Write("Main", "Restarting system in 10 seconds. Application will exit now.");
				return;
			}

			logger.Write("Main", "Restart denied. Application will exit now.");
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

		static Config LoadConfiguration()
		{
			var file = "config.json";

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
				Console.WriteLine(ex.Message);
				return null;
			}
		}
	}
}
