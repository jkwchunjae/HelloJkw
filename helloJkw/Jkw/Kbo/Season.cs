using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw
{
	public class Season
	{
		public int Year { get; set; }
		public int BeginDate { get; set; }
		public int EndDate { get; set; }
		public string LastSeasonRank { get; set; }
		public ChartObject chartObject { get; set; }
		public List<Standing> StandingList { get; set; }
		public List<Match> MatchList { get; set; }
		public List<TeamMatch> TeamMatchList { get; set; }
	}

	public static class ExtSeason
	{
		public static IEnumerable<Standing> GetStandingList(this Season season, int date)
		{
			season.GetStandingList();
			// 경기 없는 날은 최근 날짜로 처리
			date = season.StandingList.Where(e => e.Date <= date).Select(e => e.Date).OrderBy(e => e).LastOrDefault();
			if (date < season.BeginDate) date = season.BeginDate;

			return season.StandingList.Where(e => e.Date == date);
		}

		public static List<Standing> GetStandingList(this Season season, bool isUpdate = false, int updateDate = 0)
		{
			if (season.StandingList == null || isUpdate)
			{
				season.StandingList = season.GetTeamMatchList(isUpdate).GetStandingList(updateDate);
			}
			return season.StandingList;
		}

		public static List<TeamMatch> GetTeamMatchList(this Season season, bool isUpdate = false)
		{
			if (season.TeamMatchList == null || isUpdate)
			{
				season.TeamMatchList = season.GetMatchList(isUpdate).GetTeamMatchList();
			}
			return season.TeamMatchList;
		}

		public static List<Match> GetMatchList(this Season season, bool isUpdate = false)
		{
			if (season.MatchList == null || isUpdate)
			{
				var teamSet = season.LastSeasonRank.Split(',').Select(e => e.Trim()).ToHashSet();
				season.MatchList = KboMatch._matchList
					.Where(e => e.Date >= season.BeginDate && e.Date <= season.EndDate)
					.Where(e => teamSet.Contains(e.Home) && teamSet.Contains(e.Away))
					.ToList();
			}
			return season.MatchList;
		}

		public static ChartObject GetChartObject(this Season season, int beginDate = 0, int endDate = 0)
		{
			if (season.chartObject == null)
			{
				var teamOrder = season.LastSeasonRank.Split(',')
					.Select((e, i) => new { Team = e, Order = i })
					.ToDictionary(e => e.Team, e => e.Order);
				var standing = season.GetStandingList();
				var teamList = standing.Select(e => e.Team).Distinct().Where(e => teamOrder.ContainsKey(e)).OrderBy(e => teamOrder[e]);

				var chartObject = new ChartObject();
				chartObject.TeamGBInfo = new List<Tuple<string, string>>();
				foreach (var team in teamList)
				{
					chartObject.TeamGBInfo.Add(
						Tuple.Create(team,
						standing.Where(e => e.Team == team).OrderBy(e => e.Date).Select(e => e.GB.ToString()).StringJoin(",")
						));
				}
				chartObject.DateList = standing.OrderBy(e => e.Date).Select(e => e.Date).Distinct().Select(e => "'{0}/{1}'".With(e.Month(), e.Day())).StringJoin(",");
				chartObject.Year = season.Year;
				season.chartObject = chartObject;
			}
			return season.chartObject;
		}
	}
}
