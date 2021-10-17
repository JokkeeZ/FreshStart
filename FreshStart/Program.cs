using System;
using System.Configuration;
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

		private static Config config;
		private static Changes changes;

		static void Main()
		{
			var cfgFile = ConfigurationManager.AppSettings["configFile"];
			config = LoadConfiguration(cfgFile);

			if (config == null)
			{
				return;
			}

			log.Info($"Loaded configuration: {cfgFile}");

			var runType = GetRunType();

			changes = new Changes();

			if (config.Packages.ToRemove.Count > 0)
			{
				var packageRemover = new PackageRemover(runType);
				packageRemover.RemovePackages();
			}
			else
			{
				log.Info("Skipping packages.. Config doesn't contain any.");
			}

			if (config.Registry.Count > 0)
			{
				var reg = new RegistryCleaner(runType);
				reg.PerformCleanup();
				reg.RemoveSuggestedApps();
			}
			else
			{
				log.Info("Skipping registry keys.. Config doesn't contain any.");
			}

			if (config.ServicesToDisable.Count > 0)
			{
				var serviceMngr = new ServiceManager(runType);
				serviceMngr.DisableServices();
			}
			else
			{
				log.Info("Skipping services.. Config doesn't contain any.");
			}

			log.Info("Restarting explorer.exe for these changes to take effect on Windows.");
			RestartExplorer();

			changes.LogChanges(log);

			if (AskForRestart())
			{
				// Restart in 10 seconds.
				Process.Start("shutdown.exe", "-r -t 10");
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
			log.Debug("Asking for system restart...");

			Console.ForegroundColor = ConsoleColor.Red;

			Console.Write("Would you like to restart your system now? (Y / N): ");
			Console.ResetColor();

			var input = Console.ReadLine().ToLower();

			if (input is "y" or "n")
			{
				log.Debug(input is "y" ? "Restarting system in 10 seconds." : "Restart declined.");
				return input is "y";
			}

			Console.WriteLine("Invalid input. (Y or N) expected.");
			return AskForRestart();
		}

		static void RestartExplorer()
		{
			using var process = new Process();

			process.StartInfo = new ProcessStartInfo
			{
				FileName = "taskkill.exe",
				Arguments = "-f -im explorer.exe",
				WindowStyle = ProcessWindowStyle.Hidden
			};

			process.Start();
			process.WaitForExit();

			Process.Start(Path.Combine(Environment.GetEnvironmentVariable("windir"), "explorer.exe"));
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

		public static Config GetConfig() => config;
		public static Changes GetChanges() => changes;
	}
}
