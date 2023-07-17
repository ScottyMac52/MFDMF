namespace MFDMFApp
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// Main entry
    /// </summary>
    class Program
	{
		[STAThread()]
        [RequiresAssemblyFiles("Calls MFDMFApp.Program.RunningInstance()")]
        static int Main(string[] args)
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
        [RequiresAssemblyFiles("Calls System.Reflection.Assembly.Location")]
        public static Process? RunningInstance()
		{
			Process current = Process.GetCurrentProcess();
			Process[] processes = Process.GetProcessesByName(current.ProcessName);

			Console.WriteLine($"Detected {processes?.Length ?? 0} running processes named {current.ProcessName}");

            //Loop through the running processes in with the same name
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            foreach (Process process in processes)
			{
				//Ignore the current process
				if (process.Id != current?.Id)
				{
					var currentLocation = Assembly.GetExecutingAssembly()?.Location.Replace(".dll", ".exe", StringComparison.InvariantCultureIgnoreCase);
					//Make sure that the process is running from the exe file.
					if (currentLocation?.Equals(current?.MainModule?.FileName, StringComparison.InvariantCultureIgnoreCase) ?? false)
					{
						//Return the other process instance.
						return process;
					}
				}
			}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                              //No other instance was found, return null.
            return null;
		}

	}
}
