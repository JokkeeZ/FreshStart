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

		public RegistryCleaner(RunType runType) => this.runType = runType;

		public void PerformCleanup()
		{
			log.Info("Starting to change registry values..");

			foreach (var key in Program.GetConfig().RegistryKeys)
			{
				log.Debug($"[{key.Path}]");

				foreach (var value in key.Values)
				{
					if (IsKeySkippable(key, value))
					{
						log.Debug($"Skipping key: {key.Path} Value: {value.Name}... Reason: everything already matches with config.");
						continue;
					}

					if (runType == RunType.Manual && !Confirm.ConfirmRegistryChanges(value))
					{
						continue;
					}

					ChangeRegistryKeyValue(key, value);
				}
			}

			log.Info($"Done with the registry.");
		}

		private void ChangeRegistryKeyValue(ConfigRegistryKey key, ConfigRegistryKeyValue keyValue)
		{
			var (baseKey, path) = GetBaseKey(key.Path);
			using var subKey = baseKey.OpenSubKey(path, true);

			// SubKey does not exist.
			if (subKey == null)
			{
				CreateNewSubKey(baseKey, path, keyValue);
				return;
			}

			var oldValue = subKey.GetValue(keyValue.Name, null);

			// SubKey does not have value we're looking for.
			if (oldValue == null)
			{
				subKey.SetValue(keyValue.Name, keyValue.Value, keyValue.Type);
				log.Info($"CREATED A NEW VALUE FOR SUBKEY: {path}: {keyValue.Name}={keyValue.Value} | Type = {keyValue.Type}");

				Program.GetChanges().ReportChange(ChangeType.RegistryKeyValueMade);
				return;
			}

			try
			{
				var oldType = subKey.GetValueKind(keyValue.Name);

				if (ValueAndTypeMatches(oldValue, keyValue, oldType))
				{
					log.Debug($"Skipping: {keyValue.Name} --> Value and Type already matches.");
					return;
				}

				subKey.SetValue(keyValue.Name, keyValue.Value, keyValue.Type);
				log.Info($"{keyValue.Name}={keyValue.Value} | Type = {keyValue.Type} (OLD: {keyValue.Name}={oldValue} | Type = {oldType})");
				Program.GetChanges().ReportChange(ChangeType.RegistryValueChanged);
			}
			catch (Exception ex)
			{
				log.Error("Error", ex);
			}
		}

		private void CreateNewSubKey(RegistryKey baseKey, string path, ConfigRegistryKeyValue keyValue)
		{
			using var newSubKey = baseKey.CreateSubKey(path, true);
			newSubKey.SetValue(keyValue.Name, keyValue.Value, keyValue.Type);

			log.Info($"CREATED A NEW SUBKEY WITH VALUE: {path}: {keyValue.Name}={keyValue.Value} | Type = {keyValue.Type}");
			Program.GetChanges().ReportChange(ChangeType.RegistryKeyMade);
			Program.GetChanges().ReportChange(ChangeType.RegistryKeyValueMade);
		}

		private bool IsKeySkippable(ConfigRegistryKey key, ConfigRegistryKeyValue keyValue)
		{
			var (baseKey, path) = GetBaseKey(key.Path);

			using var subKey = baseKey.OpenSubKey(path, true);

			if (subKey == null)
			{
				return false;
			}

			var subKeyValue = subKey.GetValue(keyValue.Name, null);

			if (subKeyValue == null)
			{
				return false;
			}

			var subKeyType = subKey.GetValueKind(keyValue.Name);

			if (subKeyType != keyValue.Type)
			{
				return false;
			}

			if (!ValueAndTypeMatches(subKeyValue, keyValue, subKeyType))
			{
				return false;
			}

			return true;
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
			const string suggestedAppsPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SuggestedApps";
			var suggestedApps = GetSuggestedApps(suggestedAppsPath);

			if (suggestedApps.Length <= 0)
			{
				log.Debug("No suggested apps found.");
				return;
			}

			Registry.CurrentUser.DeleteSubKey(suggestedAppsPath);

			log.Info($"Deleted suggested apps reg key: HKCU:\\{suggestedAppsPath}");
		}

		private string[] GetSuggestedApps(string path)
		{
			using var subKey = Registry.CurrentUser.OpenSubKey(path);
			return subKey == null ? Array.Empty<string>() : subKey.GetValueNames();
		}

		private bool ValueAndTypeMatches(object value, ConfigRegistryKeyValue keyValue, RegistryValueKind type)
			=> value.ToString() == keyValue.Value.ToString() && type == keyValue.Type;
	}
}