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
			Get["/jkw"] = _ =>
			{
				var files = Directory.GetFiles(@"Static\Agency\img\bg\", "*");
				var rnd = new Random((int)DateTime.Now.Ticks);
				var model = new
				{
					BackGroundFileName = Path.GetFileName(files[rnd.Next(files.Count())])
				};
				return View["jkwHome", model];
			};
		}
	}
}
