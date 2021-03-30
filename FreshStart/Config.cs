using System.Collections.Generic;
using Microsoft.Win32;

namespace FreshStart
{
	class Config
	{
		public UnwantedPackage UnwantedPackage { get; set; }
		public List<Reg> Registry { get; set; }
	}

	class UnwantedPackage
	{
		public bool RemoveFromAllUsers { get; set; }
		public List<string> Packages { get; set; }
	}

	class Reg
	{
		public string Path { get; set; }
		public List<RegKey> Keys { get; set; }
	}

	class RegKey
	{
		public string Key { get; set; }
		public object Value { get; set; }
		public RegistryValueKind Type { get; set; }
	}
}
