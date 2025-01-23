using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Management.Deployment;

namespace FreshStart;

class PackageRemover(RunType runType)
{
	private readonly PackageManager packageManager = new();
	private readonly RunType runType = runType;

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
			Console.WriteLine($"[PackageUninstaller] - Package removal completed. Didn't found packages to remove/deprovision.");
			return;
		}

		Console.WriteLine("[PackageUninstaller] - Starting to remove and deprovision packages..");

		foreach (var package in installedPackages)
		{
			if (runType == RunType.Manual && !Confirm.ConfirmPackageRemoval(package.Id.Name))
			{
				continue;
			}

			if (RemovePackage(package))
			{
				Program.GetChanges().ReportChange(ChangeType.PackageUninstalled);
			}
		}

		foreach (var package in provisionedPackages)
		{
			if (runType == RunType.Manual && !Confirm.ConfirmPackageDeprovision(package.Id.Name))
			{
				continue;
			}

			if (DeprovisionPackage(package))
			{
				Program.GetChanges().ReportChange(ChangeType.PackageDeprovisioned);
			}
		}

		Console.WriteLine("[PackageUninstaller] - Package removal/deprovision completed.");
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
			Console.WriteLine("[PackageUninstaller] - Removed {package.Id.FullName}");
			completedEvent.Set();
		};

		completedEvent.WaitOne();

		if (operation.Status != AsyncStatus.Completed)
		{
			var result = operation.GetResults();

			Console.WriteLine($"--- Package removal failed? Result below ---");
			Console.WriteLine($"ErrorText: {result.ErrorText}");
			Console.WriteLine($"ExtendedErrorCode: {result.ExtendedErrorCode}");

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
			Console.WriteLine($"[PackageUninstaller] - Deprovisioned {package.Id.FullName}");
			completedEvent.Set();
		};

		completedEvent.WaitOne();

		if (operation.Status != AsyncStatus.Completed)
		{
			var result = operation.GetResults();

			Console.WriteLine($"--- Package deprovision failed? Result below ---");
			Console.WriteLine($"ErrorText: {result.ErrorText}");
			Console.WriteLine($"ExtendedErrorCode: {result.ExtendedErrorCode}");

			return false;
		}

		return true;
	}

	private static bool ComparePackageName(Package package, string cfgName)
		=> package.Id.FullName.Contains(cfgName, StringComparison.OrdinalIgnoreCase);
}
