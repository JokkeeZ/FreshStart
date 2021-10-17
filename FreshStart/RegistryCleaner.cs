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

			foreach (var reg in Program.GetConfig().Registry)
			{
				log.Debug($"[{reg.Path}]");

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

		public void ChangeRegistryKeyValue(ConfigRegistryLocation reg, ConfigRegistryKey key)
		{
			try
			{
				var (baseKey, path) = GetBaseKey(reg.Path);
				using var oldSubKey = baseKey.OpenSubKey(path, true);

				if (oldSubKey == null)
				{
					using var newSubKey = baseKey.CreateSubKey(path, true);
					newSubKey.SetValue(key.Key, key.Value, key.Type);

					log.Info($"CREATED A NEW SUBKEY: {path}: {key.Key}={key.Value} | Type = {key.Type}");
					Changes.RegistryKeysMade++;
				}
				else
				{
					var oldValue = oldSubKey.GetValue(key.Key, null);
					var oldType = oldSubKey.GetValueKind(key.Key);

					if (oldValue.ToString() == key.Value.ToString() && oldType == key.Type)
					{
						log.Debug($"Skipping: {key.Key} --> Value and Type already matches.");
						return;
					}

					oldSubKey.SetValue(key.Key, key.Value, key.Type);
					log.Info($"{key.Key}={key.Value} | Type = {key.Type} (OLD: {key.Key}={oldValue} | Type = {oldType})");
					Changes.RegistryValuesChanged++;
				}
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
			var suggestedApps = GetSuggestedApps();

			if (suggestedApps.Length <= 0)
			{
				log.Debug("No suggested apps found.");
				return;
			}

			Registry.CurrentUser.DeleteSubKey(SUGGESTED_APPS_REG);

			log.Info($"Deleted suggested apps reg key: HKCU:\\{SUGGESTED_APPS_REG}");
		}

		private string[] GetSuggestedApps()
		{
			var subKey = Registry.CurrentUser.OpenSubKey(SUGGESTED_APPS_REG);
			return subKey == null ? Array.Empty<string>() : subKey.GetValueNames();
		}
	}
}