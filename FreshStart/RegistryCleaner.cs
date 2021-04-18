using System;
using System.Linq;
using log4net;
using Microsoft.Win32;

namespace FreshStart
{
	class RegistryCleaner
	{
		private readonly ILog log = LogManager.GetLogger(typeof(RegistryCleaner));
		private readonly RunType runType;
		const string SUGGESTED_APPS_REG = @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SuggestedApps";

		public RegistryCleaner(RunType runType) => this.runType = runType;

		public void PerformCleanup()
		{
			log.Info("Starting to change registry values..");

			foreach (var reg in Program.Config.Registry)
			{
				if (runType == RunType.Full)
				{
					log.Debug($"[{reg.Path}]");
				}

				foreach (var key in reg.Keys)
				{
					if (runType == RunType.Manual && !Confirm.ConfirmRegistryChanges(key))
					{
						continue;
					}

					ChangeRegistryKeyValue(reg, key);
				}
			}

			log.Info($"Done with the registry.");
		}

		public void ChangeRegistryKeyValue(Reg reg, RegKey key)
		{
			try
			{
				var (baseKey, path) = GetBaseKey(reg.Path);

				var oldValue = baseKey.OpenSubKey(path, true).GetValue(key.Key, null);
				var oldType = baseKey.OpenSubKey(path, true).GetValueKind(key.Key);

				if (oldValue.ToString() == key.Value.ToString() && oldType == key.Type)
				{
					if (runType == RunType.Full)
					{
						log.Debug($"Skipping: {key.Key} --> Value and Type already matches.");
					}

					return;
				}

				baseKey.OpenSubKey(path, true).SetValue(key.Key, key.Value, key.Type);

				if (oldType == key.Type)
				{
					if (runType == RunType.Full)
					{
						log.Debug($"{key.Key}={key.Value} | Type = {key.Type} (OLD: {key.Key}={oldValue} | Type = {key.Type})");
					}

					return;
				}

				log.Warn(string.Join("\r\n", new[]
				{
					" ",
					"------------------- START OF WARNING -------------------",
					$"Path [{reg.Path}]",
					$"Value type changed: {key.Key}",
					$"Changed value type from {oldType} --> {key.Type}",
					"-------------------- END OF WARNING --------------------",
					" "
				}));
			}
			catch (Exception ex)
			{
				log.Error(ex.ToString());
			}
		}

		private (RegistryKey baseKey, string path) GetBaseKey(string path)
		{
			var splitted = path.Split('\\');

			var baseKey = splitted.First() switch
			{
				"HKEY_CLASSES_ROOT" => Registry.ClassesRoot,
				"HKEY_CURRENT_USER" => Registry.CurrentUser,
				"HKEY_LOCAL_MACHINE" => Registry.LocalMachine,
				"HKEY_USERS" => Registry.Users,
				"HKEY_CURRENT_CONFIG" => Registry.CurrentConfig,
				_ => throw new ArgumentException("Trying to access non existing registry location.")
			};

			return (baseKey, string.Join("\\", splitted.Skip(1)));
		}

		public void RemoveSuggestedApps()
		{
			var apps = GetSuggestedApps();

			if (apps.Length <= 0)
			{
				if (runType == RunType.Full)
				{
					log.Debug("No suggested apps found.");
				}

				return;
			}

			Registry.CurrentUser.DeleteSubKey(SUGGESTED_APPS_REG);

			if (runType == RunType.Full)
			{
				log.Debug($"Deleted suggested apps reg key: HKCU:\\{SUGGESTED_APPS_REG}");
			}
		}

		private string[] GetSuggestedApps()
		{
			var sub = Registry.CurrentUser.OpenSubKey(SUGGESTED_APPS_REG);
			return sub == null ? Array.Empty<string>() : sub.GetValueNames();
		}
	}
}