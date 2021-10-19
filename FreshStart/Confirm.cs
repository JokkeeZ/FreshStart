using System;

namespace FreshStart
{
	static class Confirm
	{
		public static bool ConfirmPackageRemoval(string packageName)
		{
			Console.Write($"Remove package: {packageName}? (Y / N): ");
			var input = Console.ReadLine().ToLower();

			if (input is "y" or "n")
			{
				return input is "y";
			}

			Console.WriteLine("Invalid input. (Y or N) expected.");
			return ConfirmPackageRemoval(packageName);
		}

		public static bool ConfirmPackageDeprovision(string packageName)
		{
			Console.Write($"Deprovision package: {packageName}? (Y / N): ");
			var input = Console.ReadLine().ToLower();

			if (input is "y" or "n")
			{
				return input is "y";
			}

			Console.WriteLine("Invalid input. (Y or N) expected.");
			return ConfirmPackageDeprovision(packageName);
		}

		public static bool ConfirmServiceDisable(string service)
		{
			Console.Write($"Disable service: {service}? (Y / N): ");
			var input = Console.ReadLine().ToLower();

			if (input is "y" or "n")
			{
				return input is "y";
			}

			Console.WriteLine("Invalid input. (Y or N) expected.");
			return ConfirmServiceDisable(service);
		}

		public static bool ConfirmRegistryChanges(ConfigRegistryKeyValue key)
		{
			Console.Write($"{key.Summary}? (Y / N): ");
			var input = Console.ReadLine().ToLower();

			if (input is "y" or "n")
			{
				return input is "y";
			}

			Console.WriteLine("Invalid input. (Y or N) expected.");
			return ConfirmRegistryChanges(key);
		}
	}
}
