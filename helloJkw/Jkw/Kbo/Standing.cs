using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw
{
	public class Standing
	{
		public int Season { get; set; }
		public int Date { get; set; }
		public string Team { get; set; }
		public int Rank { get; set; }
		public int Win { get; set; }
		public int Draw { get; set; }
		public int Lose { get; set; }
		public double PCT { get; set; } // 승률
		public double GB { get; set; } // 게임차
		public string Last10 { get; set; } // 최근10경기
		public string STRK { get; set; } // 연속
		public string HomeResult { get; set; } // 홈 결과
		public string AwayResult { get; set; } // 원정 결과

		public int? Diff1d { get; set; } // 1일 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int? Diff3d { get; set; } // 3일 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int? Diff7d { get; set; } // 7일 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int? Diff2w { get; set; } // 2주 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int? Diff1m { get; set; } // 1달 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int? Diff2m { get; set; } // 2달 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int? Diff3m { get; set; } // 3달 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int? Diff1y { get; set; } // 1년 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int? Diff2y { get; set; } // 2년 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
	}

	public static class ExtStanding
	{
		#region 순위 비교
		public static int CalcDiffRank(this Standing standing, IEnumerable<Season> seasonList, int diffDate)
		{
			try
			{
				var diffSeason = seasonList.Where(e => e.Year == diffDate.Year()).First();
				var diffStanding = diffSeason.GetStandingList(diffDate).Where(e => e.Team == standing.Team).First();
				return standing.Rank - diffStanding.Rank;
			}
			catch
			{
				return 0;
			}
		}

		public static void CalcDiffRank(this Standing standing, IEnumerable<Season> seasonList)
		{
			standing.Diff1d = standing.CalcDiffRank(seasonList, standing.Date.AddDays(-1));
			standing.Diff3d = standing.CalcDiffRank(seasonList, standing.Date.AddDays(-3));
			standing.Diff7d = standing.CalcDiffRank(seasonList, standing.Date.AddDays(-7));
			standing.Diff2w = standing.CalcDiffRank(seasonList, standing.Date.AddWeeks(-2));
			standing.Diff1m = standing.CalcDiffRank(seasonList, standing.Date.AddMonths(-1));
			standing.Diff2m = standing.CalcDiffRank(seasonList, standing.Date.AddMonths(-2));
			standing.Diff3m = standing.CalcDiffRank(seasonList, standing.Date.AddMonths(-3));
			standing.Diff1y = standing.CalcDiffRank(seasonList, standing.Date.AddYears(-1));
			standing.Diff2y = standing.CalcDiffRank(seasonList, standing.Date.AddYears(-2));
		}

		public static void CalcDiffRank(this IEnumerable<Standing> standingList, IEnumerable<Season> seasonList)
		{
			foreach (var standing in standingList)
			{
				if (standing.Diff1d != null) continue;
				standing.CalcDiffRank(seasonList);
			}
		}
		#endregion

		#region 최근 10경기
		public static void CalcLast10(this Standing standing, IEnumerable<TeamMatch> teamMatchList)
		{
			var result = teamMatchList.Reverse().Take(10)
				.GroupBy(e => 1)
				.Select(e => new { Win = e.Sum(t => (t.IsWin ? 1 : 0)), Draw = e.Sum(t => (t.IsDraw ? 1 : 0)), Lose = e.Sum(t => (t.IsLose ? 1 : 0)) })
				.First();

			var last10 = new List<string>();
			standing.Last10 = "";
			if (result.Win > 0) standing.Last10 += " {0}승".With(result.Win);
			if (result.Draw > 0) standing.Last10 += " {0}무".With(result.Draw);
			if (result.Lose > 0) standing.Last10 += " {0}패".With(result.Lose);

			standing.Last10 = standing.Last10.Trim();
		}

		public static void CalcLast10(this IEnumerable<Standing> standingList, IEnumerable<TeamMatch> teamMatchList)
		{
			foreach (var standing in standingList)
			{
				if (standing.Last10 != null) continue;
				standing.CalcLast10(teamMatchList.Where(e => e.Team == standing.Team));
			}
		}
		#endregion

		#region 연속
		public static void CalcSTRK(this Standing standing, IEnumerable<TeamMatch> teamMatchList)
		{
			var lastMatch = teamMatchList.Last();
			int cnt = 0;
			foreach (var match in teamMatchList.Reverse())
			{
				if (!(lastMatch.IsWin == match.IsWin && lastMatch.IsDraw == match.IsDraw && lastMatch.IsLose == match.IsLose))
					break;
				cnt++;
			}
			standing.STRK = "{cnt}{result}".WithVar(new { cnt, result = (lastMatch.IsWin ? "승" : (lastMatch.IsDraw ? "무" : "패")) });
		}

		public static void CalcSTRK(this IEnumerable<Standing> standingList, IEnumerable<TeamMatch> teamMatchList)
		{
			foreach (var standing in standingList)
			{
				if (standing.STRK != null) continue; // cache 사용
				standing.CalcSTRK(teamMatchList.Where(e => e.Team == standing.Team));
			}
		}
		#endregion
	}
}
