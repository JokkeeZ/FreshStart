using System;
using System.IO;

namespace FreshStart
{
	class Logger : IDisposable
	{
		private readonly StreamWriter stream;
		private readonly string LOG_FILE = $"{DateTime.Now:dd.MM.yyyy}-complete-log.log";

		public Logger() => stream = new(LOG_FILE, true);

		public void Write(string caller, string text)
		{
			stream.WriteLine($"[{DateTime.Now}][{caller}()] ~~> {text}");
			Console.WriteLine($"[{caller}] ~~> {text}");
		}

		public void Dispose() => stream?.Dispose();
	}
}
