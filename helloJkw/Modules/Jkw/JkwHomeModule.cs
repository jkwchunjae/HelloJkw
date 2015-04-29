using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using helloJkw.Utils;
using System.Dynamic;

namespace helloJkw
{
	public class JkwModule : NancyModule
	{
		public dynamic Model = new ExpandoObject();

		public JkwModule()
		{
#if (DEBUG)
			Model.isDebug = true;
#else
			Model.isDebug = false;
#endif
		}
	}

	public class JkwHomeModule : JkwModule
	{
		public JkwHomeModule()
		{
			Get["/"] = _ =>
			{
				HitCounter.Hit("hellojkw home");
				var files = Directory.GetFiles(@"Static/Agency/img/bg/", "*");
				var gameRoot = @"jkw/games";
				var games = Directory.GetDirectories(gameRoot)
					.Select(path => new
					{
						engName = Path.GetFileName(path),
						korName = File.ReadAllText(Path.Combine(path, "info.txt")),
						thumbnail = Path.GetFileName(Directory.GetFiles(path, "*thumbnail*").FirstOrDefault()),
					}.ToExpando());

				Model.BackGroundFileName = Path.GetFileName(files.GetRandom());
				Model.games = games;
				return View["jkwHome", Model];
			};
		}
	}
}
