using Microsoft.Win32;

namespace FreshStart;

class Config
{
	public ConfigPackages Packages { get; set; } = new();
	public List<ConfigRegistryKey> RegistryKeys { get; set; } = [];
	public List<string> ServicesToDisable { get; set; } = [];
}

class ConfigPackages
{
	public bool RemoveFromAllUsers { get; set; }
	public List<string> ToRemove { get; set; } = [];
}

class ConfigRegistryKey
{
	public string Path { get; set; }
	public List<ConfigRegistryKeyValue> Values { get; set; } = [];
}

class ConfigRegistryKeyValue
{
	public string Name { get; set; }
	public object Value { get; set; }
	public RegistryValueKind Type { get; set; }
	public string Summary { get; set; }
}

sealed class Settings
{
	public required string ConfigFile { get; set; } = null!;
}
