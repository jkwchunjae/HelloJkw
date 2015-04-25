using Nancy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace helloJkw.Modules.Jkw
{
	public class JkwKboChartModule : NancyModule
	{
		public JkwKboChartModule()
		{
			Get["/kbochart/{year?default}"] = _ =>
			{
				dynamic Model = new ExpandoObject();
				string yearStr = _.year;
				if (yearStr == "reload")
				{
					KboMatch.Reload();
				}
#if (DEBUG)
				KboMatch.Update(1000);
#else
				KboMatch.Update();
#endif

				int year = (yearStr == "default" || !yearStr.IsInt()) ? KboMatch.RecentSeason : yearStr.ToInt();
				var chartObject = KboMatch.GetChartObject(year);
				Model.chartObject = chartObject;
				Model.DateCount = chartObject.DateList.Split(',').Count();
				Model.YearList = KboMatch.SeasonList.Select(e => e.Year).OrderByDescending(e => e).ToList();
				Model.Title = "jkw's KBO Chart {Year}".WithVar(new { chartObject.Year });
				Model.Desc = "KBO {Year} 시즌 게임차 그래프".WithVar(new { chartObject.Year });
				
				return View["jkwKboChart", Model];
			};

			Post["/kbochart/standing/{year?default}/{date?default}"] = _ =>
			{
				string yearStr = _.year;
				string dateStr = _.date;
				int year = yearStr == "default" ? KboMatch.RecentSeason : yearStr.ToInt();
				int date = dateStr == "default" ? KboMatch.SeasonList.Where(e => e.Year == year).Select(e => e.StandingList.Max(t => t.Date)).Max() : dateStr.ToInt();

				var standingList = KboMatch.SeasonList.Where(e => e.Year == date.Year())
					.SelectMany(e => e.StandingList.Where(t => t.Date == date))
					.ToList();

				var standingJsonArray = standingList
					.OrderBy(e => e.Rank)
					.Select(e => new JObject(
						new JProperty("Rank", e.Rank),
						new JProperty("Team", e.Team),
						new JProperty("Game", e.Win + e.Draw + e.Lose),
						new JProperty("Win", e.Win),
						new JProperty("Draw", e.Draw),
						new JProperty("Lose", e.Lose),
						new JProperty("PCT", e.PCT.Round(3).ToString("0.000")),
						new JProperty("GB", e.GB),
						new JProperty("Last10", e.Last10),
						new JProperty("STRK", e.STRK),
						new JProperty("Home", e.HomeResult),
						new JProperty("Away", e.AwayResult)
						));

				return new JObject(
					new JProperty("standing", standingJsonArray),
					new JProperty("updateTime", KboMatch.LastUpdateTime.ToString("yyyy-MM-dd hh:mm:ss"))
					).ToString();
			};
		}
	}
}
