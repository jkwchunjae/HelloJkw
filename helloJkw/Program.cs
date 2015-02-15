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
			using (var host = new NancyHost(new Uri("http://localhost:80")))
			{
				host.Start();
				Console.ReadLine();
			}
		}
	}
}
