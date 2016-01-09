using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using helloJkw.Utils;
using System.Dynamic;
using Newtonsoft.Json;

namespace helloJkw
{
	public class DateInfo
	{
		public DateTime Date;
		public string DayKr { get { return Date.GetWeekday(DateLanguage.KR, WeekdayFormat.D); } }
		public string DayEn { get { return Date.GetWeekday(DateLanguage.EN, WeekdayFormat.D); } }
	}

	public class SchedulerModule : JkwModule
	{
		public SchedulerModule()
		{
			#region Get /scheduler
			Get["/scheduler"] = _ =>
			{
				var date = new DateTime(2016, 1, 11);
				var dateList = (new DateTime[] { date, date.AddDays(1), date.AddDays(2), date.AddDays(3), date.AddDays(4) })
					.Select(x => new DateInfo { Date = x }).ToList();

				Model.DateList = dateList;
				return View["others/scheduler", Model];
			};
			#endregion

			#region Post /schedule/add
			Post["/schedule/add"] = _ =>
			{
				var date = ((string)Request.Form["date"]).ToInt();
				var time = ((string)Request.Form["time"]).ToInt();
				var duration = ((string)Request.Form["duration"]).ToInt();
				var title = (string)Request.Form["title"];

				var schedule = new Schedule
				{
					UserId = "1",
					Date = date,
					Time = time,
					Duration = duration,
					Title = title
				};

				dynamic obj = new ExpandoObject();

				if (SchedulerManager.AddSchedule(schedule))
				{
					obj.Result = "success";
					obj.ScheduleId = schedule.Id;
					obj.Color = "orange";
				}
				else
				{
					obj.Result = "fail";
					obj.Message = "";
				}

				return JsonConvert.SerializeObject(obj);
			};
			#endregion
		}
	}
}
