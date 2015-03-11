using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using helloJkw.Extensions;

namespace helloJkw.Modules.Jkw
{
	public class JkwHomeModule : NancyModule
	{
		public JkwHomeModule()
		{
			Get["/"] = _ =>
			{
				var files = Directory.GetFiles(@"Static/Agency/img/bg/", "*");
				var gameRoot = @"jkw/Games";
				var games = Directory.GetDirectories(gameRoot)
					.Select(path => new
					{
						engName = Path.GetFileName(path),
						korName = File.ReadAllText(Path.Combine(path, "info.txt")),
						thumbnail = Path.GetFileName(Directory.GetFiles(path, "*thumbnail*").FirstOrDefault()),
					}.ToExpando());

				var rnd = new Random((int)DateTime.Now.Ticks);
				var model = new
				{
					BackGroundFileName = Path.GetFileName(files[rnd.Next(files.Count())]), 
					games = games
				};
				return View["jkwHome", model];
			};
		}
	}
}
