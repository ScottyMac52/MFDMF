namespace MFDMF_Services
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public static class ServiceTimer
	{
		private static Stopwatch stopWatch;

		static ServiceTimer()
		{
			if(stopWatch == null)
			{
				stopWatch = new Stopwatch();
			}
		}

		public static void Start()
		{
			stopWatch.Start();
		}

		public static TimeSpan StopAndGetDuration()
		{
			if(!stopWatch.IsRunning)
			{
				return new TimeSpan();
			}
			stopWatch.Stop();
			return stopWatch.Elapsed;
		}
	}
}
