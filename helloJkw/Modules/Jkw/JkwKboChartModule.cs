using Nancy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Extensions;
using helloJkw.Utils;

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
				HitCounter.Hit("kbochart/chart/" + year.ToString());
				Logger.Log("viewLog - kbochart/chart/" + year.ToString());
				var chartObject = KboMatch.GetChartObject(year);
				Model.chartObject = chartObject;
				Model.DateCount = chartObject.DateList.Split(',').Count();
				Model.LastDate = KboMatch.SeasonList.Where(e => e.Year == year).Select(e => e.StandingList.Max(t => t.Date)).First().ToDate().ToString("yyyy-MM-dd");
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
				var season = KboMatch.SeasonList.Where(e => e.Year == year).FirstOrDefault();
				int date = dateStr == "default" ? season.StandingList.Max(t => t.Date) : dateStr.ToInt();
				HitCounter.Hit("kbochart/standing/" + date.ToString());
				Logger.Log("viewLog - kbochart/standing/" + date.ToString());
				// 경기 없는 날은 최근 날짜로 처리
				date = season.StandingList.Where(e => e.Date <= date).Select(e => e.Date).OrderBy(e => e).LastOrDefault();
				if (date < season.BeginDate) date = season.BeginDate;

				var standingList = season.StandingList.Where(e => e.Date == date).ToList();

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
					new JProperty("updateTime", KboMatch.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"))
					).ToString();
			};
		}
	}
}
