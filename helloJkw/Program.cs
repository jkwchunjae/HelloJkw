using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.Conventions;

namespace helloJkw
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Port: ");
			var port = Console.ReadLine();
			using (var host = new NancyHost(new Uri("http://localhost:" + port)))
			{
				Console.WriteLine("Start Lucia Shop");
				host.Start();
				Console.ReadLine();
			}
		}
	}
}
