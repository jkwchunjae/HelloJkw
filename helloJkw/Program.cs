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
			string port = "80";
			if (args != null && args.Count() > 0)
				port = args[0];
			using (var host = new NancyHost(new Uri("http://localhost:" + port)))
			{
				host.Start();
				Console.ReadLine();
			}
		}
	}
}
