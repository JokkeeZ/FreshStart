using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace FreshStart;

enum RunType
{
	Full,
	Manual
}

class Program
{
	static Config config;
	static Changes changes;

	static void Main()
	{
		var cfg = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();

		var settings = cfg.GetRequiredSection("Settings").Get<Settings>();

		config = LoadConfiguration(settings.ConfigFile);

		if (config == null)
		{
			return;
		}

		Console.WriteLine($"[Program] - Loaded configuration: {settings.ConfigFile}");

		changes = new();

		var runType = GetRunType();

		if (config.Packages.ToRemove.Count > 0)
		{
			var packageRemover = new PackageRemover(runType);
			packageRemover.RemovePackages();
		}
		else
		{
			Console.WriteLine($"[Program] - Skipping packages.. Config doesn't contain any.");
		}

		if (config.RegistryKeys.Count > 0)
		{
			var reg = new RegistryCleaner(runType);
			reg.PerformCleanup();
			RegistryCleaner.RemoveSuggestedApps();
		}
		else
		{
			Console.WriteLine($"[Program] - Skipping registry keys.. Config doesn't contain any.");
		}

		if (config.ServicesToDisable.Count > 0)
		{
			var serviceMngr = new ServiceManager(runType);
			serviceMngr.DisableServices();
		}
		else
		{
			Console.WriteLine($"[Program] - Skipping services.. Config doesn't contain any.");
		}

		changes.LogChangesMade();

		if (!changes.AnyChangesDone())
		{
			Console.WriteLine($"[Program] - Completed. Press any key to continue...");

			// Just to keep program alive.
			Console.ReadLine();
			return;
		}

		Console.WriteLine($"[Program] - Restarting explorer.exe for these changes to take effect on Windows.");
		RestartExplorer();

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
		Console.WriteLine($"[Program] - Asking for system restart...");

		Console.ForegroundColor = ConsoleColor.Red;

		Console.Write("Would you like to restart your system now? (Y / N): ");
		Console.ResetColor();

		var input = Console.ReadLine().ToLower();

		if (input is "y" or "n")
		{
			Console.WriteLine(input is "y" ? "[Program] - Restarting system in 10 seconds." : "[Program] - Restart declined.");
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

	static readonly JsonSerializerOptions options = new()
	{
		ReadCommentHandling = JsonCommentHandling.Skip
	};

	static Config LoadConfiguration(string file)
	{
		if (!File.Exists(file))
		{
			Console.WriteLine($"[Program] - Config file: {file} does not exist.");
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<Config>(File.ReadAllText(file), options);
		}
		catch (JsonException ex)
		{
			Console.WriteLine($"[Program] - Error: {ex.Message}");
			return null;
		}
	}

	public static Config GetConfig() => config;

	public static Changes GetChanges() => changes;
}
