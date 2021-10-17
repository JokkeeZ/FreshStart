using log4net;
using System;

namespace FreshStart
{
	static class Changes
	{
		public static int RegistryValuesChanged;
		public static int RegistryKeysMade;
		public static int ServicesDisabled;
		public static int PackagesUninstalled;

		public static void LogChanges(ILog log)
		{
			log.Info(string.Join(Environment.NewLine, new[]
			{
				Environment.NewLine,
				"*******************************************",
				$"Registry values changed: {RegistryValuesChanged}",
				$"Registry keys made: {RegistryKeysMade}",
				$"Services disabled: {ServicesDisabled}",
				$"Packages uninstalled: {PackagesUninstalled}",
				"*******************************************",
				Environment.NewLine
			}));
		}

		public static bool ContainsAny() =>
			RegistryValuesChanged > 0
			|| RegistryKeysMade > 0
			|| ServicesDisabled > 0
			|| PackagesUninstalled > 0;
	}
}
