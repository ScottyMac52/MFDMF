using System;
using System.Diagnostics;
using System.Reflection;

namespace MFDMFApp
{
	/// <summary>
	/// Main entry
	/// </summary>
	public static class Program
	{
		[STAThread()]
		public static int Main(string[] args)
		{
			var runningInstance = RunningInstance();
			runningInstance?.Kill();
			var mainApp = new MainApp(args);
			mainApp.Run();
			return 0;
		}

		/// <summary>
		/// Detects if another instance is running and kills it
		/// </summary>
		/// <returns></returns>
		public static Process RunningInstance()
		{
			Process current = Process.GetCurrentProcess();
			Process[] processes = Process.GetProcessesByName(current.ProcessName);

			//Loop through the running processes in with the same name
			foreach (Process process in processes)
			{
				//Ignore the current process
				if (process.Id != current.Id)
				{
					var currentLocation = Assembly.GetExecutingAssembly()?.Location.Replace(".dll", ".exe", StringComparison.InvariantCultureIgnoreCase);
					//Make sure that the process is running from the exe file.
					if (currentLocation?.Equals(current.MainModule.FileName, StringComparison.InvariantCultureIgnoreCase) ?? false)
					{
						//Return the other process instance.
						return process;
					}
				}
			}
			//No other instance was found, return null.
			return null;
		}

	}
}
