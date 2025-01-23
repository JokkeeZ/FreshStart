using Microsoft.Win32;

namespace FreshStart;

class ServiceManager(RunType runType)
{
	private readonly RunType runType = runType;

	const string ServicesPath = "SYSTEM\\CurrentControlSet\\Services";

	public void DisableServices()
	{
		foreach (var service in Program.GetConfig().ServicesToDisable)
		{
			try
			{
				using var key = Registry.LocalMachine.OpenSubKey($"{ServicesPath}\\{service}", true);

				if (key == null)
				{
					Console.WriteLine($"[ServiceManager] - Skipping service: {service}. Not found in registry.");
					continue;
				}

				if ((int)key.GetValue("Start") == 4)
				{
					Console.WriteLine($"[ServiceManager] - Skipping service: {service}. Already disabled.");
					continue;
				}

				if (runType == RunType.Manual && !Confirm.ConfirmServiceDisable(service))
				{
					continue;
				}

				key.SetValue("Start", 4, RegistryValueKind.DWord);

				Console.WriteLine($"[ServiceManager] - Service: {service} disabled.");
				Program.GetChanges().ReportChange(ChangeType.ServiceDisabled);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[ServiceManager] - Error: {ex.Message}");
			}
		}
	}
}
