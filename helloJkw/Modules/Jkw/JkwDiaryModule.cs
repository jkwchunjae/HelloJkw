using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Extensions;
using helloJkw.Utils;
using System.Diagnostics;
using System.IO;

namespace helloJkw
{
	public class JkwDiaryModule : JkwModule
	{
		public JkwDiaryModule()
		{
			Get["/diary"] = _ =>
			{
				if (session.IsLogin)
				{
					return View["diary/jkwDiaryHome", Model];
				}
				else
				{
					return View["diary/jkwDiaryRequireLogin", Model];
				}
			};
		}
	}
}
