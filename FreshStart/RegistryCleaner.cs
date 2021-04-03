using System;
using Microsoft.Win32;

namespace FreshStart
{
	class RegistryCleaner
	{
		const string SUGGESTED_APPS_REG = @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\SuggestedApps";

		public void PerformCleanup()
		{
			foreach (var reg in Program.Config.Registry)
			{
				foreach (var key in reg.Keys)
				{
					try
					{
						var oldValue = Registry.GetValue(reg.Path, key.Key, null);
						Registry.SetValue(reg.Path, key.Key, key.Value, key.Type);
					}
					catch (Exception ex)
					{

					}
				}
			}
		}

		public void RemoveSuggestedApps()
		{
			var apps = GetSuggestedApps();

			if (apps.Length <= 0)
			{
				return;
			}

			Registry.CurrentUser.DeleteSubKey(SUGGESTED_APPS_REG);
		}

		private string[] GetSuggestedApps()
		{
			var sub = Registry.CurrentUser.OpenSubKey(SUGGESTED_APPS_REG);
			return sub == null ? Array.Empty<string>() : sub.GetValueNames();
		}
	}
}