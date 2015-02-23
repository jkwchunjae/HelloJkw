using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw.Modules.Jkw
{
	public class JkwHomeModule : NancyModule
	{
		public JkwHomeModule()
		{
			Get["/"] = _ =>
			{
				var files = Directory.GetFiles(@"Static/Agency/img/bg/", "*");
				var gameRoot = @"_Jkw/Games";
				var games = Directory.GetDirectories(gameRoot)
					.Select(path => new
					{
						EngName = Path.GetFileName(path),
						KorName = File.ReadAllText(Path.Combine(path, "info.txt")),
						Thumbnail = Path.GetFileName(Directory.GetFiles(path, "*thumbnail*").FirstOrDefault()),
					});

				var rnd = new Random((int)DateTime.Now.Ticks);
				var model = new
				{
					BackGroundFileName = Path.GetFileName(files[rnd.Next(files.Count())])
					, Games = games
				};
				return View["jkwHome", model];
			};
		}
	}
}
