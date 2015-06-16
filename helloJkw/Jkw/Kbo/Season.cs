using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw
{
	public class ChartObject
	{
		public int Year { get; set; }
		public string DateList { get; set; }
		public List<Tuple<string, string>> TeamGBInfo { get; set; }
		public double MaximumGB { get; set; }
	}

	public class Season
	{
		public int Year { get; set; }
		public int BeginDate { get; set; }
		public int EndDate { get; set; }
		public string LastSeasonRank { get; set; }
		public ChartObject chartObject = null;
		public List<Standing> StandingList = new List<Standing>();
		public List<Match> MatchList;
		public List<TeamMatch> TeamMatchList;

		public Season() { }

		public Season(List<Match> matchList)
		{
			MatchList = matchList;
			MakeTeamMatchList();
		}

		#region 순위표 계산
		public List<Standing> CalcStanding(int updateDate = 0)
		{
			Season season = this;
			var teamList = TeamMatchList.Select(e => e.Team).Distinct().ToList();
			var dateList = TeamMatchList.Select(e => e.Date).Distinct().Where(e => season == null || season.StandingList == null || e >= updateDate).OrderBy(e => e).ToList();

			#region updateDate 이후 데이터 삭제
			var deleteList = StandingList.Where(e => e.Date >= updateDate).ToList();
			foreach (var standing in deleteList)
			{
				Logger.Log("Remove {Date}, {Team}".WithVar(standing));
				StandingList.Remove(standing);
			}
			#endregion

			foreach (var date in dateList)
			{
				var currStandingList = new List<Standing>();

				var yesterdayList = StandingList.OrderBy(e => e.Date).GroupBy(e => e.Team).Select(e => e.Last());
				var currList = TeamMatchList.Where(e => e.Date == date);

				#region 승, 무, 패, 승률
				foreach (var team in teamList)
				{
					var standing = new Standing { Date = date, Team = team };

					// 가장 최근 순위 기록을 구한다.
					var yesterday = yesterdayList.Where(e => e.Team == team).FirstOrDefault();
					standing.Win = yesterday != null ? yesterday.Win : 0;
					standing.Draw = yesterday != null ? yesterday.Draw : 0;
					standing.Lose = yesterday != null ? yesterday.Lose : 0;

					// 최근 기록에서 해당 경기 결과를 반영한다.
					// foreach 인 이유는 더블헤더 경기가 있기 때문에!
					foreach (var curr in currList.Where(t => t.Team == team))
					{
						standing.Win += (curr != null && curr.IsWin) ? 1 : 0;
						standing.Draw += (curr != null && curr.IsDraw) ? 1 : 0;
						standing.Lose += (curr != null && curr.IsLose) ? 1 : 0;
					}

					// 승률 = 승 / (승 + 패)  // 무승부는 무시! (2009년 무승부는 '패' 처리)
					standing.PCT = (standing.Win == 0) ? 0 : ((double)standing.Win / (standing.Win + standing.Lose + (date.Year() == 2009 ? standing.Draw : 0)));

					currStandingList.Add(standing);
				}
				#endregion

				#region 순위
				foreach (var team in currStandingList)
				{
					team.Rank = currStandingList.Where(e => e.PCT > team.PCT).Count() + 1;
				}
				#endregion

				#region 승차 (게임차)
				var rank1 = currStandingList.Where(e => e.Rank == 1).First();
				foreach (var team in currStandingList)
				{
					if (date.Year() == 2009)
					{
						team.GB = team.Rank == 1 ? 0 : ((rank1.Win - team.Win) - ((rank1.Lose + rank1.Draw) - (team.Lose + team.Draw))) / 2.0;
					}
					else
					{
						team.GB = team.Rank == 1 ? 0 : ((rank1.Win - team.Win) - (rank1.Lose - team.Lose)) / 2.0;
					}
				}
				#endregion
				StandingList.AddRange(currStandingList);
			}

			StandingList = StandingList.OrderBy(e => e.Date).ThenBy(e => e.Rank).ToList();
			var lastDate = StandingList.Last().Date;
			var tmpStanding = StandingList.Where(e => e.Date >= lastDate.AddDays(-7));
			tmpStanding.CalcDiffRank(KboCenter.SeasonList);
			tmpStanding.CalcLast10(season.TeamMatchList);
			tmpStanding.CalcSTRK(season.TeamMatchList);
			return StandingList;
		}
		#endregion

		#region Make TeamMatch
		public void MakeTeamMatchList()
		{
			TeamMatchList = new List<TeamMatch>();
			foreach (var match in MatchList)
			{
				TeamMatchList.Add(new TeamMatch
				{
					Date = match.Date,
					Team = match.Away,
					Score = match.AwayScore,
					OtherTeam = match.Home,
					OtherScore = match.HomeScore,
					IsHome = false
				});
				TeamMatchList.Add(new TeamMatch
				{
					Date = match.Date,
					Team = match.Home,
					Score = match.HomeScore,
					OtherTeam = match.Away,
					OtherScore = match.AwayScore,
					IsHome = true
				});
			}
		}
		#endregion

		#region ChartObject
		private DateTime _lastChartObjectTime = DateTime.Now.AddDays(-1);
		public ChartObject MakeChartObject(int beginDate = 0, int endDate = 0)
		{
			// chartObject 가 null 이거나, 계산한지 3분이상 지났을경우
			if (chartObject == null || DateTime.Now.Subtract(_lastChartObjectTime).TotalMinutes > 3.0)
			{
				_lastChartObjectTime = DateTime.Now;
				var teamOrder = LastSeasonRank.Split(',')
					.Select((e, i) => new { Team = e, Order = i })
					.ToDictionary(e => e.Team, e => e.Order);
				var teamList = StandingList.Select(e => e.Team).Distinct().Where(e => teamOrder.ContainsKey(e)).OrderBy(e => teamOrder[e]);

				chartObject = new ChartObject();
				chartObject.TeamGBInfo = new List<Tuple<string, string>>();
				foreach (var team in teamList)
				{
					chartObject.TeamGBInfo.Add(
						Tuple.Create(team,
						StandingList.Where(e => e.Team == team).OrderBy(e => e.Date).Select(e => e.GB.ToString()).StringJoin(",")
						));
				}
				chartObject.DateList = StandingList.OrderBy(e => e.Date).Select(e => e.Date).Distinct().Select(e => "'{0}/{1}'".With(e.Month(), e.Day())).StringJoin(",");
				chartObject.Year = Year;
				chartObject.MaximumGB = StandingList.Max(t => t.GB);
			}
			return chartObject;
		}
		#endregion

		public IEnumerable<Standing> GetLastStanding(int date)
		{
			date = StandingList.Where(e => e.Date <= date).OrderByDescending(e => e.Date).FirstOrDefault().Date;
			if (date < BeginDate) date = BeginDate;
			return StandingList.Where(e => e.Date == date);
		}
	}

	//public static class ExtSeason
	//{

	//	public static IEnumerable<Standing> GetStandingList(this Season season, int date)
	//	{
	//		season.GetStandingList();
	//		// 경기 없는 날은 최근 날짜로 처리
	//		date = season.StandingList.Where(e => e.Date <= date).Select(e => e.Date).OrderBy(e => e).LastOrDefault();
	//		if (date < season.BeginDate) date = season.BeginDate;

	//		return season.StandingList.Where(e => e.Date == date);
	//	}

	//	public static List<Standing> GetStandingList(this Season season, bool isUpdate = false, int updateDate = 0)
	//	{
	//		if (season.StandingList == null || isUpdate)
	//		{
	//			season.StandingList = season.GetTeamMatchList(isUpdate).GetStandingList(updateDate);
	//		}
	//		return season.StandingList;
	//	}

	//	public static List<TeamMatch> GetTeamMatchList(this Season season, bool isUpdate = false)
	//	{
	//		if (season.TeamMatchList == null || isUpdate)
	//		{
	//			season.TeamMatchList = season.GetMatchList(isUpdate).GetTeamMatchList();
	//		}
	//		return season.TeamMatchList;
	//	}

	//	public static List<Match> GetMatchList(this Season season, bool isUpdate = false)
	//	{
	//		if (season.MatchList == null || isUpdate)
	//		{
	//			var teamSet = season.LastSeasonRank.Split(',').Select(e => e.Trim()).ToHashSet();
	//			season.MatchList = KboMatch._matchList
	//				.Where(e => e.Date >= season.BeginDate && e.Date <= season.EndDate)
	//				.Where(e => teamSet.Contains(e.Home) && teamSet.Contains(e.Away))
	//				.OrderByDescending(e => e.Date)
	//				.ToList();
	//		}
	//		return season.MatchList;
	//	}
	//}
}
