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
using Newtonsoft.Json.Linq;

namespace helloJkw
{
	public class YearGroup
	{
		public int Year;
		public List<MonthGroup> MonthList;
	}

	public class MonthGroup
	{
		public int Month;
		public List<DateTime> DateList;
	}
	public class JkwDiaryModule : JkwModule
	{
		public JkwDiaryModule()
		{
			#region Show Diary
			Get["/diary/{diaryName?}/{date?}"] = _ =>
			{
				if (!session.IsLogin)
					return View["diary/jkwDiaryRequireLogin", Model];

				// 자신의 다이어리가 있다면 가장 우선적으로 보여준다.
				// 없으면 나의 다이어리를 보여준다.
				string defaultDiaryName = UserManager.GetUser("112902876433833556239").DiaryName;
				string diaryName = _.diaryName != null ? _.diaryName
					: string.IsNullOrEmpty(session.User.DiaryName)
						? defaultDiaryName : session.User.DiaryName;
				if (!DiaryManager.IsValidDiaryName(diaryName))
					diaryName = defaultDiaryName;

				bool withSecure = session.User.DiaryName == diaryName;

				var diaryList = _.date != null
					? DiaryManager.GetDiary(diaryName, ((string)_.date).ToDate(), withSecure)
					: DiaryManager.GetLastDiary(diaryName, withSecure);
				var date = diaryList.Any() ? diaryList.First().Date : DateTime.MinValue;
				var prevDate = DiaryManager.GetPrevDate(diaryName, date, withSecure);
				var nextDate = DiaryManager.GetNextDate(diaryName, date, withSecure);

				HitCounter.Hit("diary-{diaryName}-{date}".WithVar(new { diaryName, date = date.ToInt() }));

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

			Get["/diary/home"] = _ =>
			{
				if (!session.IsLogin)
					return View["diary/jkwDiaryRequireLogin", Model];

				return null;
			};

			Get["/diary/showdates/{diaryName}"] = _ =>
			{
				if (!session.IsLogin)
					return View["diary/jkwDiaryRequireLogin", Model];

				string diaryName = _.diaryName;
				bool withSecure = session.User.DiaryName == diaryName;
				var dateList = DiaryManager.GetAllDates(diaryName, withSecure);

				var dateGroup = dateList
					.GroupBy(x => x.Year)
					.Select(x => new YearGroup
					{
						Year = x.Key,
						MonthList = x.GroupBy(e => e.Month)
									.Select(e => new MonthGroup{ Month = e.Key, DateList = e.Select(t => t).ToList() })
									.OrderByDescending(e => e.Month)
									.ToList()
					})
					.OrderByDescending(x => x.Year)
					.ToList();

				Model.DiaryName = diaryName;
				Model.DateGroup = dateGroup;
				return View["diary/jkwDiaryShowDates", Model];
			};
			#endregion

			#region Write Diary
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
			#endregion

			#region Modify Diary
			Get["/diary/modify/{diaryName}/{date}"] = _ =>
			{
				if (!session.IsLogin)
					return View["diary/jkwDiaryRequireLogin", Model];

				string diaryName = _.diaryName;
				DateTime date = ((string)_.date).ToDate();

				if (session.User.DiaryName != diaryName)
					return View["diary/jkwDiarySomethingWrong", Model];

				var diaryList = DiaryManager.GetDiary(diaryName, date, withSecure: true);

				Model.DiaryName = diaryName;
				Model.Date = date;
				Model.DiaryList = diaryList;

				return View["diary/jkwDiaryModify", Model];
			};

			Post["/diary/modify"] = _ =>
			{
				if (!session.IsLogin)
					return "로그인을 해주세요.";

				string diaryName = Request.Form["diaryName"];
				DateTime date = ((string)Request.Form["date"]).ToDate();

				if (session.User.DiaryName != diaryName)
					return "본인 다이어리가 아닙니다.";

				try
				{
					string json = Request.Form["diaryList"];
					var diaryList = ((JArray)JsonConvert.DeserializeObject(json))
						.Select(x => new
						{
							Index = ((string)((dynamic)x).Index).ToInt(),
							Text = (string)((dynamic)x).Text
						});

					foreach (var diary in diaryList)
					{
						if (string.IsNullOrEmpty(diary.Text.Trim()))
						{
							DiaryManager.DeleteDiary(diaryName, date, diary.Index);
						}
						else
						{
							DiaryManager.ModifyDiary(diaryName, date, diary.Index, diary.Text);
						}
					}
				}
				catch (Exception ex)
				{
					ex.WriteLog();
					return ex.Message;
				}

				return "success";
			};
			#endregion
		}
	}
}
