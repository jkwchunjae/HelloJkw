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
				if (session.IsLogin)
				{
					// 자신의 다이어리가 있다면 가장 우선적으로 보여준다.
					// 없으면 나의 다이어리를 보여준다.
					var viewDiaryUser = string.IsNullOrEmpty(session.User.DiaryName)
						? UserManager.GetUser("112902876433833556239") : session.User;
					Model.DiaryUserName = viewDiaryUser.Name;
					Model.DiaryUserId = viewDiaryUser.Id;
					Model.DiaryUserImage = viewDiaryUser.ImageUrl;
					return View["diary/jkwDiaryHome", Model];
				}
				else
				{
					return View["diary/jkwDiaryRequireLogin", Model];
				}
			};

			Post["/diary/get"] = _ =>
			{
				string userId = Request.Form["userId"];
				var user = UserManager.GetUser(userId);
				string dateStr = Request.Form["date"];
				var date = dateStr.ToDate();
				var diaryList = DiaryManager.GetDiary(user, date);
				var json = JsonConvert.SerializeObject(diaryList);
				return json;
			};
		}
	}
}
