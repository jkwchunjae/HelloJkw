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
	public class JkwKboChartModule : JkwModule
	{
		public JkwKboChartModule()
		{
			Get["/kbochart/{year?default}"] = _ =>
			{
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

				int year = (yearStr == "default" || !yearStr.IsInt()) ? KboMatch.RecentSeason :yearStr.ToInt();
				if (!KboMatch.SeasonList.Select(e => e.Year).Contains(year)) year = KboMatch.RecentSeason;
				HitCounter.Hit("kbochart/chart/" + year.ToString());
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
				if (!KboMatch.SeasonList.Select(e => e.Year).Contains(year)) year = KboMatch.RecentSeason;
				var season = KboMatch.SeasonList.Where(e => e.Year == year).FirstOrDefault();
				int date = dateStr == "default" ? season.StandingList.Max(t => t.Date) : dateStr.ToInt();
				HitCounter.Hit("kbochart/standing/" + date.ToString());

				var standingList = season.GetStandingList(date);
				standingList.CalcDiffRank(KboMatch.SeasonList);
				standingList.CalcLast10(season.TeamMatchList);
				standingList.CalcSTRK(season.TeamMatchList);

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
						new JProperty("GB", e.GB.ToString("0.0")),
						new JProperty("Last10", e.Last10),
						new JProperty("STRK", e.STRK),
						new JProperty("Home", e.HomeResult),
						new JProperty("Away", e.AwayResult),

						new JProperty("Diff1d", e.Diff1d),
						new JProperty("Diff3d", e.Diff3d),
						new JProperty("Diff7d", e.Diff7d),
						new JProperty("Diff2w", e.Diff2w),
						new JProperty("Diff1m", e.Diff1m),
						new JProperty("Diff2m", e.Diff2m),
						new JProperty("Diff1y", e.Diff1y),
						new JProperty("Diff2y", e.Diff2y),

						new JProperty("tmp", "tmp")
						));

				return new JObject(
					new JProperty("standing", standingJsonArray),
					new JProperty("updateTime", KboMatch.LastUpdateTime.ToString("yyyy-MM-dd HH:mm:ss"))
					).ToString();
			};
		}
	}
}
