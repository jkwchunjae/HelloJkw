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
using Newtonsoft.Json;

namespace helloJkw
{
	public class JkwDiaryModule : JkwModule
	{
		public JkwDiaryModule()
		{
			Get["/diary"] = _ =>
			{
				if (!session.IsLogin)
					return View["diary/jkwDiaryRequireLogin", Model];

				// 자신의 다이어리가 있다면 가장 우선적으로 보여준다.
				// 없으면 나의 다이어리를 보여준다.
				var diaryName = string.IsNullOrEmpty(session.User.DiaryName)
					? "jkwchunjae" : session.User.DiaryName;
				bool withSecure = session.User.DiaryName == diaryName;
				var diaryList = new[] { DiaryManager.GetLastDiary(diaryName, withSecure) };
				Model.DiaryName = diaryName;
				Model.diaryList = diaryList;
				return View["diary/jkwDiaryHome", Model];
			};

			Get["/diary/{diaryName}/{date}"] = _ =>
			{
				if (!session.IsLogin)
					return View["diary/jkwDiaryRequireLogin", Model];

				string diaryName = _.diaryName;
				DateTime date = ((string)_.date).ToDate();
				bool withSecure = session.User.DiaryName == diaryName;
				var diaryList = DiaryManager.GetDiary(diaryName, date, withSecure);
				Model.diaryList = diaryList;
				return View["diary/jkwDiaryHome", Model];
			};

			Post["/diary/get"] = _ =>
			{
				if (!session.IsLogin)
					return string.Empty;

				string diaryName = Request.Form["diaryName"];
				DateTime date = ((string)Request.Form["date"]).ToDate();
				bool withSecure = session.User.DiaryName == diaryName;
				var diaryList = DiaryManager.GetDiary(diaryName, date, withSecure);
				var json = JsonConvert.SerializeObject(diaryList);
				return json;
			};
		}
	}
}
