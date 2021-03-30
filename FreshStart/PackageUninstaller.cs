using System.Collections.Generic;
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

		private List<Package> GetInstalledPackages()
		{
			var packages = new List<Package>();

			foreach (var pck in Program.Config.UnwantedPackage.Packages)
			{
				var package = manager.FindPackages().FirstOrDefault(x => x.Id.FullName.ToLower().Contains(pck.ToLower()));

				if (package != null)
				{
					packages.Add(package);
				}
			}

			return packages;
		}

		public void RemovePackages()
		{
			using var logger = new Logger();

			var manager = new PackageManager();

			var packages = GetInstalledPackages();

			logger.Write("RemovePackages", $"{packages.Count} unwanted packages found!");

			if (packages.Count == 0)
			{
				logger.Write("RemovePackages", "Good news! Didn't find any unwanted packages!");
				return;
			}

			logger.Write("RemovePackages", $"Starting to remove unwanted packages.");

			foreach (var package in GetInstalledPackages())
			{
				RemovePackage(package, logger);
			}
		}

		private void RemovePackage(Package package, Logger logger)
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
				logger.Write("RemovePackages", $"Package: {package.Id.Name} removed.");
				return;
			}

			var result = operation.GetResults();

			logger.Write("RemovePackages", $"Package: {package.Id.Name} removal failed.");
			logger.Write("RemovePackages", $"ErrorText (if available): {result.ErrorText}");
			logger.Write("RemovePackages", $"ExtendedErrorCode (if available): {result.ExtendedErrorCode}");
		}
	}
}
