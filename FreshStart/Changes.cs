using log4net;
using System;

namespace FreshStart
{
	class Changes
	{
		private int registryValuesChanged;
		private int registryKeysMade;
		private int servicesDisabled;
		private int packagesUninstalled;

		public void LogChanges(ILog log)
		{
			log.Info(string.Join(Environment.NewLine, new[]
			{
				Environment.NewLine,
				"*******************************************",
				$"Registry values changed: {registryValuesChanged}",
				$"Registry keys made: {registryKeysMade}",
				$"Services disabled: {servicesDisabled}",
				$"Packages uninstalled: {packagesUninstalled}",
				"*******************************************",
				Environment.NewLine
			}));
		}

		public void IncreaseRegistryValueChange() => registryValuesChanged++;
		public void IncreaseRegistryKeysMade() => registryKeysMade++;
		public void IncreaseServicesDisabled() => servicesDisabled++;
		public void IncreasePackagesUninstalled() => packagesUninstalled++;
	}
}
