using System;
using Microsoft.Win32;

namespace FreshStart
{
	class RegistryCleaner
	{
		const string SUGGESTED_APPS_REG = @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SuggestedApps";

		public void PerformCleanup()
		{
			using var logger = new Logger();

			logger.Write("PerformCleanup", $"Starting to perform registry cleanup.");

			foreach (var reg in Program.Config.Registry)
			{
				logger.Write("PerformCleanup", $"[{reg.Path}]");

				foreach (var key in reg.Keys)
				{
					try
					{
						var oldValue = Registry.GetValue(reg.Path, key.Key, null);

						Registry.SetValue(reg.Path, key.Key, key.Value, key.Type);
						logger.Write("PerformCleanup", $"{key.Key}={key.Value} ({ (oldValue == null ? "Key didn't exist earlier." : $"Key original state: {key.Key}={oldValue}") })");
					}
					catch (Exception ex)
					{
						logger.Write("PerformCleanup", string.Join(Environment.NewLine, new[]
						{
							string.Empty,
							"************************** EXCEPTION INFORMATION **************************",
							$"PATH : {reg.Path}",
							$"OPERATION  : {key.Key}={key.Value}",
							$"TYPE: {key.Type}",
							$"EXCEPTION: {ex}",
							"***************************************************************************",
							string.Empty
						}));
					}
				}
			}

			logger.Write("PerformCleanup", "Registry cleanup ended.");
		}

		public void RemoveSuggestedApps()
		{
			using var logger = new Logger();

			logger.Write("RemoveSuggestedApps", "Looking for suggested apps... ");

			var apps = GetSuggestedApps();

			if (apps.Length <= 0)
			{
				logger.Write("RemoveSuggestedApps", "Didn't find any suggested apps!");
				return;
			}

			logger.Write("RemoveSuggestedApps", string.Join(Environment.NewLine, new[]
			{
				$"Found {apps.Length} suggested apps!",
				"APPS: ",
				string.Join(Environment.NewLine, apps)
			}));

			Registry.CurrentUser.DeleteSubKey(SUGGESTED_APPS_REG);

			logger.Write("RemoveSuggestedApps", $"Removed registry key: HKCU\\{SUGGESTED_APPS_REG}");
		}

		private string[] GetSuggestedApps()
		{
			var sub = Registry.CurrentUser.OpenSubKey(SUGGESTED_APPS_REG);
			return sub == null ? Array.Empty<string>() : sub.GetValueNames();
		}
	}
}