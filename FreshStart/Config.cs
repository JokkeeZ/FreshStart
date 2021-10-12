using System.Collections.Generic;
using Microsoft.Win32;

namespace FreshStart
{
	class Config
	{
		public ConfigPackages Packages { get; set; }
		public List<ConfigRegistryLocation> Registry { get; set; }
		public List<string> ServicesToDisable { get; set; }
	}

	class ConfigPackages
	{
		public bool RemoveFromAllUsers { get; set; }
		public List<string> ToRemove { get; set; }
	}

	class ConfigRegistryLocation
	{
		public string Path { get; set; }
		public List<ConfigRegistryKey> Keys { get; set; }
	}

	class ConfigRegistryKey
	{
		public string Key { get; set; }
		public object Value { get; set; }
		public RegistryValueKind Type { get; set; }
		public string Summary { get; set; }
	}
}
