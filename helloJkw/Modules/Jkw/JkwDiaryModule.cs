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
					// 자신의 다이어리가 있다면 가장 우선적으로 보여준다.
					// 없으면 나의 다이어리를 보여준다.
					return View["diary/jkwDiaryHome", Model];
				}
				else
				{
					return View["diary/jkwDiaryRequireLogin", Model];
				}
			};

			Post["/diary/get/{user}/{date}"] = _ =>
			{
				string user = _.user;
				string dateStr = _.date;
			};
		}
	}
}
