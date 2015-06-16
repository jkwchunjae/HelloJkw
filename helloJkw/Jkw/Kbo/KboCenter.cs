using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Extensions;

namespace helloJkw
{
	#region Classes
	public class Match
	{
		public int Date { get; set; }
		public string Away { get; set; }
		public string Home { get; set; }
		public int AwayScore { get; set; }
		public int HomeScore { get; set; }
	}

	public class TeamMatch
	{
		public int Season { get; set; }
		public int Date { get; set; }
		public string Team { get; set; }
		public int Score { get; set; }
		public string OtherTeam { get; set; }
		public int OtherScore { get; set; }
		public bool IsHome { get; set; }
		public bool IsWin { get { return Score > OtherScore; } }
		public bool IsLose { get { return Score < OtherScore; } }
		public bool IsDraw { get { return Score == OtherScore; } }
	}
	#endregion

	public static class KboCenter
	{
		static string _filepathMatchHistory = @"jkw/project/kbo/kbochart/matchHistory.txt";
		static string _filepathSeasonInfo = @"jkw/project/kbo/kbochart/seasonInfo.txt";

		public static List<Match> AllMatchList;
		public static List<Season> SeasonList;

		public static int RecentSeason { get { return SeasonList.Max(t => t.Year); } }
		public static DateTime LastUpdateTime { get; private set; }

		static KboCenter()
		{
			AllMatchList = KboDataManager.LoadMatchList(_filepathMatchHistory);
			SeasonList = KboDataManager.LoadSeasonList(_filepathSeasonInfo);
			foreach (var season in SeasonList)
			{
				var teamSet = season.LastSeasonRank.Split(',').ToHashSet();
				season.MatchList = AllMatchList
					.Where(e => e.Date.Year() == season.Year)
					.Where(e => e.Date.IsBetween(season.BeginDate, season.EndDate))
					.Where(e => teamSet.Contains(e.Home) && teamSet.Contains(e.Away))
					.OrderBy(e => e.Date)
					.ToList();
				season.MakeTeamMatchList();
				season.CalcStanding();
				season.MakeChartObject();
			}
		}

		public static void Load() { }

		public static ChartObject GetChartObject(int year)
		{
			if (!SeasonList.Select(e => e.Year).Contains(year))
				year = RecentSeason;
			return SeasonList.Where(e => e.Year == year).First().MakeChartObject();
		}

		#region Update (경기 결과가 변경되면 파일 저장한다.)
		static object _updateLock = new object();
		public static void Update(int minute = 5)
		{
			lock (_updateLock)
			{
				if (minute > 0 && DateTime.Now.Subtract(LastUpdateTime).TotalMinutes < minute)
					return;
				LastUpdateTime = DateTime.Now;

				var beginDate = AllMatchList.Max(t => t.Date);
				var endDate = DateTime.Today.ToInt();
				Season updateSeason = null;
				int updateDate = 0;
				foreach (var date in beginDate.DateRange(endDate))
				{
					var season = SeasonList.Where(e => e.Year == date.Year()).FirstOrDefault();
					if (season == null) continue;
					if (date < season.BeginDate || date > season.EndDate) continue;

					// kbo website 에서 데이터 가져온다.
					var matchList = KboDataManager.CrawlMatchList(date);
					// 두 MatchList 가 같다는 뜻은 경기 결과가 변한게 없다는 뜻.
					var currentMatchList = AllMatchList.Where(t => t.Date == date).ToList();
					if (currentMatchList.EqualMatchList(matchList)) continue;

					foreach (var match in currentMatchList)
					{
						Logger.Log("Remove match {Date}, {Away}, {Home}".WithVar(match));
						AllMatchList.Remove(match);
					}
					AllMatchList.AddRange(matchList);
					updateSeason = season;
					updateDate = updateDate == 0 ? date : Math.Min(updateDate, date);
				}

				// MatchList 가 뭔가 바뀐 경우다!
				if (updateSeason != null)
				{
					Logger.Log("Update after {updateDate}".WithVar(new { updateDate }));
					var teamSet = updateSeason.LastSeasonRank.Split(',').ToHashSet();
					updateSeason.MatchList = AllMatchList
						.Where(e => e.Date.Year() == updateSeason.Year)
						.Where(e => e.Date.IsBetween(updateSeason.BeginDate, updateSeason.EndDate))
						.Where(e => teamSet.Contains(e.Home) && teamSet.Contains(e.Away))
						.OrderBy(e => e.Date)
						.ToList();
					updateSeason.MakeTeamMatchList();
					updateSeason.CalcStanding(updateDate);
					updateSeason.chartObject = null;
					KboDataManager.SaveMatchList(_filepathMatchHistory, AllMatchList);
				}
			}
		}

		#region EqualMatchList (두 Match 비교)
		public static bool EqualMatchList(this List<Match> match1, List<Match> match2)
		{
			// match1 은 항상 not null 이겠지만 그냥 이렇게 구현 함.
			if (match1 == null && match2 == null) return true;
			if (match1 == null || match2 == null) return false;
			if (match1.Count() != match2.Count()) return false;

			var orderedMatch1 = match1.OrderBy(e => e.Date).ThenBy(e => e.Home).ThenBy(e => e.Away);
			var orderedMatch2 = match2.OrderBy(e => e.Date).ThenBy(e => e.Home).ThenBy(e => e.Away);
			foreach (var pair in orderedMatch1.Zip(orderedMatch2, (m1, m2) => new { m1, m2 }))
			{
				if (pair.m1.Date != pair.m2.Date) return false;
				if (pair.m1.Home != pair.m2.Home) return false;
				if (pair.m1.Away != pair.m2.Away) return false;
				if (pair.m1.HomeScore != pair.m2.HomeScore) return false;
				if (pair.m1.AwayScore != pair.m2.AwayScore) return false;
			}

			return true;
		}
		#endregion
		#endregion
	}
}
