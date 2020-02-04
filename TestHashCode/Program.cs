using System;

namespace TestHashCode
{
	class Program
	{
		static void Main(string[] args)
		{
			string s = "Hello World!";
			Console.WriteLine($"{s} --> {s.GetHashCode()}");
			Console.ReadKey();
		}
	}
}
