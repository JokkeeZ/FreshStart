using System.Text;

namespace FreshStart;

enum ChangeType
{
	RegistryValueChanged,
	RegistryKeyMade,
	RegistryKeyValueMade,
	ServiceDisabled,
	PackageUninstalled,
	PackageDeprovisioned
}

class Changes
{
	private readonly Dictionary<ChangeType, int> changes = new()
	{
		[ChangeType.RegistryValueChanged] = 0,
		[ChangeType.RegistryKeyMade] = 0,
		[ChangeType.RegistryKeyValueMade] = 0,
		[ChangeType.ServiceDisabled] = 0,
		[ChangeType.PackageUninstalled] = 0,
		[ChangeType.PackageDeprovisioned] = 0
	};

	public void ReportChange(ChangeType changeType) => changes[changeType]++;

	public bool AnyChangesDone() => changes.Any(x => x.Value > 0);

	public void LogChangesMade()
	{
		var sb = new StringBuilder(Environment.NewLine);

		foreach (var change in changes)
		{
			sb.AppendLine($"{change.Key}: {change.Value}");
		}

		Console.WriteLine(sb.ToString());
	}
}
