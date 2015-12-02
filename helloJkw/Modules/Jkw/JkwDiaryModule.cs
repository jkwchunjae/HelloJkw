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

				Model.Date = date.ToString("yyyy.MM.dd");
				Model.hasPrev = prevDate != DateTime.MinValue;
				Model.hasNext = nextDate != DateTime.MinValue;
				Model.PrevDate = prevDate.ToString("yyyyMMdd");
				Model.NextDate = nextDate.ToString("yyyyMMdd");
				Model.DiaryName = diaryName;
				Model.DiaryList = diaryList;
				Model.IsMine = session.User.DiaryName == diaryName;
				return View["diary/jkwDiaryHome", Model];
			};

			//Get["/diary/{diaryName}/{date}"] = _ =>
			//{
			//	if (!session.IsLogin)
			//		return View["diary/jkwDiaryRequireLogin", Model];

			//	string diaryName = _.diaryName;
			//	DateTime date = ((string)_.date).ToDate();
			//	bool withSecure = session.User.DiaryName == diaryName;
			//	var diaryList = DiaryManager.GetDiary(diaryName, date, withSecure);
			//	Model.diaryList = diaryList;
			//	return View["diary/jkwDiaryHome", Model];
			//};

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
