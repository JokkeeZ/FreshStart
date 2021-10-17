using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Deployment;

namespace FreshStart
{
	class PackageRemover
	{
		private readonly ILog log = LogManager.GetLogger(typeof(PackageRemover));
		private readonly PackageManager packageManager;
		private readonly RunType runType;

		public PackageRemover(RunType runType) => (packageManager, this.runType) = (new(), runType);

		public List<Package> GetInstalledPackages()
		{
			var installedPackages = new List<Package>();

			foreach (var pck in Program.GetConfig().Packages.ToRemove)
			{
				var packages = packageManager.FindPackages().Where(x => ComparePackageName(x, pck));

				if (packages?.Count() > 0)
				{
					installedPackages.AddRange(packages);
				}

				var provisionedPackages = packageManager.FindProvisionedPackages().Where(x => ComparePackageName(x, pck));

				if (provisionedPackages?.Count() > 0)
				{
					installedPackages.AddRange(provisionedPackages);
				}
			}

			return installedPackages.Distinct().ToList();
		}

		public void RemovePackages()
		{
			var installedPackages = GetInstalledPackages();
			
			if (installedPackages?.Count() <= 0)
			{
				log.Info($"Package removal completed. Didn't found packages to remove.");
				return;
			}

			log.Info("Starting to remove packages..");

			foreach (var package in installedPackages)
			{
				if (runType == RunType.Manual && !Confirm.ConfirmPackageRemoval(package.Id.Name))
				{
					continue;
				}

				if (RemovePackage(package))
				{
					Changes.PackagesUninstalled++;
				}
			}

			log.Info($"Package removal completed.");
		}

		private bool RemovePackage(Package package)
		{
			using var completedEvent = new AutoResetEvent(false);

			var operation = packageManager.RemovePackageAsync(package.Id.FullName,
					Program.GetConfig().Packages.RemoveFromAllUsers
					? RemovalOptions.RemoveForAllUsers
					: RemovalOptions.None);

			operation.Completed = (_, _) =>
			{
				log.Info($"Removed {package.Id.FullName}");
				completedEvent.Set();
			};

			completedEvent.WaitOne();

			if (operation.Status != AsyncStatus.Completed)
			{
				var result = operation.GetResults();

				log.Error($"--- Package removal failed? Result below ---");
				log.Error($"ErrorText: {result.ErrorText}");
				log.Error($"ExtendedErrorCode: {result.ExtendedErrorCode}");

				return false;
			}

			return true;
		}

		private bool ComparePackageName(Package package, string cfgName)
			=> package.Id.FullName.IndexOf(cfgName, StringComparison.OrdinalIgnoreCase) >= 0;
	}
}
