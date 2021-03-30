using System;
using Microsoft.Win32;

namespace FreshStart
{
	class RegistryCleaner
	{
		public void PerformCleanup()
		{
			using var logger = new Logger();

			logger.Write("PerformCleanup", $"Starting to perform registry privacy cleanup.");

			foreach (var reg in Program.Config.Registry)
			{
				logger.Write("PerformCleanup", $"[{reg.Path}]");

				foreach (var key in reg.Keys)
				{
					try
					{
						var oldValue = Registry.GetValue(reg.Path, key.Key, null);

						Registry.SetValue(reg.Path, key.Key, key.Value, key.Type);
						logger.Write("PerformCleanup", $"{key.Key}={key.Value} ({ (oldValue == null ? "Key didn't exist earlier." : $"Key original state: {key.Key}={oldValue}") })");
					}
					catch (Exception ex)
					{
						logger.Write("PerformCleanup", string.Join(Environment.NewLine, new[]
						{
							string.Empty,
							"************************** EXCEPTION INFORMATION **************************",
							$"PATH : {reg.Path}",
							$"OPERATION  : {key.Key}={key.Value}",
							$"TYPE: {key.Type}",
							$"EXCEPTION: {ex}",
							"********************************************************************************",
							string.Empty
						}));
					}
				}
			}

			logger.Write("PerformCleanup", "Registry cleanup ended.");
		}
	}
}