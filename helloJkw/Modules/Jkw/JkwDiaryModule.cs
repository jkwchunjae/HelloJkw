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
			Get["/diary/{diaryName?}/{date?}"] = _ =>
			{
				if (!session.IsLogin)
					return View["diary/jkwDiaryRequireLogin", Model];

				// 자신의 다이어리가 있다면 가장 우선적으로 보여준다.
				// 없으면 나의 다이어리를 보여준다.
				string diaryName = _.diaryName != null ? _.diaryName
					: string.IsNullOrEmpty(session.User.DiaryName)
						? "test" : session.User.DiaryName;
				bool withSecure = session.User.DiaryName == diaryName;

				var diaryList = _.date != null
					? DiaryManager.GetDiary(diaryName, ((string)_.date).ToDate(), withSecure)
					: DiaryManager.GetLastDiary(diaryName, withSecure);
				var date = diaryList.Any() ? diaryList.First().Date : DateTime.MinValue;
				var prevDate = DiaryManager.GetPrevDate(diaryName, date, withSecure);
				var nextDate = DiaryManager.GetNextDate(diaryName, date, withSecure);

				Model.Date = date;
				Model.hasPrev = prevDate != DateTime.MinValue;
				Model.hasNext = nextDate != DateTime.MinValue;
				Model.PrevDate = prevDate;
				Model.NextDate = nextDate;
				Model.DiaryName = diaryName;
				Model.DiaryList = diaryList;
				Model.IsMine = session.User.DiaryName == diaryName;
				return View["diary/jkwDiaryHome", Model];
			};

			Get["/diary/write/{diaryName}"] = _ =>
			{
				if (!session.IsLogin)
					return View["diary/jkwDiaryRequireLogin", Model];

				string diaryName = _.diaryName;
				if (session.User.DiaryName != diaryName)
					return View["diary/jkwDiarySomethingWrong", Model];

				Model.Date = DateTime.Today;
				Model.DiaryName = diaryName;
				return View["diary/jkwDiaryWrite", Model];
			};

			Post["/diary/write"] = _ =>
			{
				if (!session.IsLogin)
					return "로그인을 해주세요.";

				string diaryName = Request.Form["diaryName"];
				DateTime date = ((string)Request.Form["date"]).ToDate();
				string text = Request.Form["text"];

				if (session.User.DiaryName != diaryName)
					return "본인 다이어리가 아닙니다.";

				try
				{
					DiaryManager.WriteDiary(diaryName, date, text, isSecure: false);
				}
				catch (Exception ex)
				{
					ex.WriteLog();
					return ex.Message;
				}

				return "success";
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
