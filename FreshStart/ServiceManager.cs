using log4net;
using Microsoft.Win32;
using System;

namespace FreshStart
{
	class ServiceManager
	{
		private readonly ILog log = LogManager.GetLogger(typeof(ServiceManager));
		private readonly RunType runType;

		const string ServicesPath = "SYSTEM\\CurrentControlSet\\Services";

		public ServiceManager(RunType runType) => this.runType = runType;

		public void DisableServices()
		{
			foreach (var service in Program.GetConfig().ServicesToDisable)
			{
				try
				{
					using var key = Registry.LocalMachine.OpenSubKey($"{ServicesPath}\\{service}", true);

					if (key == null)
					{
						log.Debug($"Skipping service: {service}. Not found in registry.");
						continue;
					}

					if ((int)key.GetValue("Start") == 4)
					{
						log.Debug($"Skipping service: {service}. Already disabled.");
						continue;
					}

					if (runType == RunType.Manual && !Confirm.ConfirmServiceDisable(service))
					{
						continue;
					}

					key.SetValue("Start", 4, RegistryValueKind.DWord);

					log.Info($"Service: {service} disabled.");
					Changes.ServicesDisabled++;
				}
				catch (Exception ex)
				{
					log.Error(ex.ToString());
				}
			}
		}
	}
}
