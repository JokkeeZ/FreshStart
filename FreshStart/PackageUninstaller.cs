using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Deployment;

namespace FreshStart
{
	class PackageRemover
	{
		private readonly PackageManager manager;

		public PackageRemover() => manager = new();

		public List<Package> GetInstalledPackages()
		{
			var installedPackages = new List<Package>();

			foreach (var pck in Program.Config.UnwantedPackage.Packages)
			{
				var packages = manager.FindPackages().Where(x => ComparePackageName(x, pck));

				if (packages != null && packages.Count() > 0)
				{
					installedPackages.AddRange(packages);
				}

				var provisionedPackages = manager.FindProvisionedPackages().Where(x => ComparePackageName(x, pck));

				if (provisionedPackages != null && provisionedPackages.Count() > 0)
				{
					installedPackages.AddRange(provisionedPackages);
				}
			}

			return installedPackages.Distinct().ToList();
		}

		public void RemovePackages()
		{
			var packages = GetInstalledPackages();

			if (packages.Count == 0)
			{
				return;
			}

			foreach (var package in GetInstalledPackages())
			{
				RemovePackage(package);
			}
		}

		private void RemovePackage(Package package)
		{
			using var completedEvent = new AutoResetEvent(false);

			var operation = manager.RemovePackageAsync(package.Id.FullName,
					Program.Config.UnwantedPackage.RemoveFromAllUsers
					? RemovalOptions.RemoveForAllUsers
					: RemovalOptions.None);

			operation.Completed = (_, _) => completedEvent.Set();

			completedEvent.WaitOne();

			if (operation.Status == AsyncStatus.Completed)
			{
				return;
			}

			var result = operation.GetResults();
		}

		private bool ComparePackageName(Package package, string cfgName)
			=> package.Id.FullName.IndexOf(cfgName, StringComparison.OrdinalIgnoreCase) >= 0;
	}
}
