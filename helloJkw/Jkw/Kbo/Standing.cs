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

		public int Diff1d { get; set; } // 1일 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int Diff3d { get; set; } // 3일 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int Diff7d { get; set; } // 7일 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int Diff2w { get; set; } // 2주 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int Diff1m { get; set; } // 1달 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int Diff2m { get; set; } // 2달 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int Diff3m { get; set; } // 3달 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int Diff1y { get; set; } // 1년 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
		public int Diff2y { get; set; } // 2년 전 순위 비교 (-1 : 그때보다 1등 올랐다.)
	}

	public static class ExtStanding
	{
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
				standing.CalcDiffRank(seasonList);
		}
	}
}
