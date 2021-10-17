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
			}

			return installedPackages;
		}

		public List<Package> GetProvisionedPackages()
		{
			var provisionedPackages = new List<Package>();

			foreach (var pck in Program.GetConfig().Packages.ToRemove)
			{
				var packages = packageManager.FindProvisionedPackages().Where(x => ComparePackageName(x, pck));

				if (packages?.Count() > 0)
				{
					provisionedPackages.AddRange(provisionedPackages);
				}
			}

			return provisionedPackages;
		}

		public void RemovePackages()
		{
			var installedPackages = GetInstalledPackages();
			var provisionedPackages = GetProvisionedPackages();

			if (installedPackages.Count <= 0 && provisionedPackages.Count <= 0)
			{
				log.Info($"Package removal completed. Didn't found packages to remove/deprovision.");
				return;
			}

			log.Info("Starting to remove and deprovision packages..");

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

			foreach (var package in provisionedPackages)
			{
				if (runType == RunType.Manual && !Confirm.ConfirmPackageRemoval(package.Id.Name))
				{
					continue;
				}

				if (DeprovisionPackage(package))
				{
					Changes.PackagesUninstalled++;
				}
			}

			log.Info($"Package removal/deprovision completed.");
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

		private bool DeprovisionPackage(Package package)
		{
			using var completedEvent = new AutoResetEvent(false);

			var operation = packageManager.DeprovisionPackageForAllUsersAsync(package.Id.FullName);

			operation.Completed = (_, _) =>
			{
				log.Info($"Deprovisioned {package.Id.FullName}");
				completedEvent.Set();
			};

			completedEvent.WaitOne();

			if (operation.Status != AsyncStatus.Completed)
			{
				var result = operation.GetResults();

				log.Error($"--- Package deprovision failed? Result below ---");
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
