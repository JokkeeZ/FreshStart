using System.Collections.Generic;
using Microsoft.Win32;

namespace FreshStart
{
	class Config
	{
		public Packages Packages { get; set; }
		public List<Reg> Registry { get; set; }
	}

	class Packages
	{
		public bool RemoveFromAllUsers { get; set; }
		public List<string> ToRemove { get; set; }
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
