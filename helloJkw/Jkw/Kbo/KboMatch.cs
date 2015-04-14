using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.Net;
using HtmlAgilityPack;

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
		public double GB { get; set; }
		public string STRK { get; set; }
		public string Last10 { get; set; }
		public string HomeResult { get; set; }
		public string AwayResult { get; set; }
	}

	public class Season
	{
		public int Year { get; set; }
		public int BeginDate { get; set; }
		public int EndDate { get; set; }
		public string LastSeasonRank { get; set; }
	}

	public class ChartObject
	{
		//public List<string> Team { get; set; }
		//public List<string> GBList { get; set; }
		public List<Tuple<string, string>> TeamGBInfo { get; set; }
		public string DateList { get; set; }
		public int Year { get; set; }
	}
	#endregion

	public static class KboMatch
	{
		static DateTime _lastUpdateTime;
		static List<Season> _seasonList;
		static List<Match> _matchList;
		static List<TeamMatch> _teamMatchList;
		static Dictionary<int, List<Standing>> _standingList = new Dictionary<int, List<Standing>>();

		static string _filepathMatchHistory = @"jkw/project/kbo/kbochart/matchHistory.txt";
		static string _filepathSeasonInfo = @"jkw/project/kbo/kbochart/seasonInfo.txt";

		public static int RecentSeason { get { return _matchList.Max(t => t.Date).Year(); } }

		static KboMatch()
		{
			Reload();
		}

		public static void Reload()
		{
			_seasonList = GetSeasonList(_filepathSeasonInfo);
			_matchList = GetMatchList(_filepathMatchHistory);
			_lastUpdateTime = DateTime.Now;
			Update(0, true);
		}

		public static void Update(int minute = 5, bool updateOldSeason = false)
		{
			if (minute > 0 && DateTime.Now.Subtract(_lastUpdateTime).TotalMinutes < minute)
				return;
			Logger.Log("KboMatch Update begin");
			var today = DateTime.Today.ToInt();
			var beginDate = _matchList.Max(t => t.Date);
			var endDate = today;
			foreach (var date in beginDate.DateRange(endDate))
			{
				var matchList = GetMatchList(date);
				var todayMatchList = _matchList.Where(t => t.Date == date).ToList();
				foreach (var match in todayMatchList)
					_matchList.Remove(match);
				_matchList.AddRange(matchList);
			}
			_teamMatchList = _matchList.GetTeamMatchList();

			if (updateOldSeason)
			{
				_standingList.Clear();
				foreach (var season in _seasonList)
				{
					var teamSet = _seasonList.Where(e => e.Year == season.Year).First().LastSeasonRank.Split(',').ToHashSet();
					_standingList[season.Year] = _teamMatchList.Where(e => e.Date >= season.BeginDate && e.Date <= season.EndDate)
						.Where(e => teamSet.Contains(e.Team))
						.GetStandingList();
				};
			}
			else
			{
				var season = _seasonList.OrderBy(e => e.Year).Last();
				var teamSet = _seasonList.Where(e => e.Year == season.Year).First().LastSeasonRank.Split(',').ToHashSet();
				_standingList[season.Year] = _teamMatchList.Where(e => e.Date >= season.BeginDate && e.Date <= season.EndDate)
					.Where(e => teamSet.Contains(e.Team))
					.GetStandingList();
			}

			SaveMatchList(_filepathMatchHistory);
			Logger.Log("KboMatch Update end");
		}

		public static List<Season> GetSeasonList(string filepath)
		{
			var seasonInfoJson = JObject.Parse(File.ReadAllText(filepath, Encoding.UTF8));

			return seasonInfoJson["season"].Children()
				.Select(e => JsonConvert.DeserializeObject<Season>(e.ToString()))
				.ToList();
		}

		#region Save MatchList
		/// <summary>
		/// MatchList 를 json 형태로 저장
		/// </summary>
		/// <param name="filepath"></param>
		static void SaveMatchList(string filepath)
		{
			var matchJsonArray = _matchList
				.OrderByDescending(e => e.Date)
				.Select(e => new JObject(
					new JProperty("Date", e.Date),
					new JProperty("Away", e.Away),
					new JProperty("Home", e.Home),
					new JProperty("AwayScore", e.AwayScore),
					new JProperty("HomeScore", e.HomeScore)
					));

			var matchJsonObject = new JObject(new JProperty("history", matchJsonArray));

			var resultString = matchJsonObject.ToString()
				.RegexReplace(@"\r", "")
				.RegexReplace(@",\n      ", ", ")
				.RegexReplace(@"\{\n      ", "{")
				.RegexReplace(@"\n    }", "}");

			File.WriteAllText(filepath, resultString);
		}
		#endregion

		#region ChartObject
		/// <summary>
		/// 차트를 그리기 위한 객체
		/// 다 string 형태로 바꿔서 전달한다.
		/// </summary>
		/// <param name="season"></param>
		/// <returns></returns>
		public static ChartObject GetChartObject(int season)
		{
			//var teamOrder = "삼성,넥센,NC,LG,SK,두산,롯데,KIA,한화,kt".Split(',')
			if (!_seasonList.Where(e => e.Year == season).Any()) return null;
			var teamOrder = _seasonList.Where(e => e.Year == season).First().LastSeasonRank.Split(',')
				.Select((e, i) => new { Team = e, Order = i })
				.ToDictionary(e => e.Team, e => e.Order);
			var chartObject = new ChartObject();
			var standing = _standingList[season];
			var teamList = standing.Select(e => e.Team).Distinct().Where(e => teamOrder.ContainsKey(e)).OrderBy(e => teamOrder[e]);
			chartObject.TeamGBInfo = new List<Tuple<string, string>>();
			foreach (var team in teamList)
			{
				chartObject.TeamGBInfo.Add(
					Tuple.Create(team,
					standing.Where(e => e.Team == team).OrderBy(e => e.Date).Select(e => e.GB.ToString()).StringJoin(",")
					));
			}
			chartObject.DateList = standing.OrderBy(e => e.Date).Select(e => e.Date).Distinct().Select(e => "'{0}/{1}'".With(e.Month(), e.Day())).StringJoin(",");
			chartObject.Year = season;
			return chartObject;
		}
		#endregion

		#region Match (from json file)
		/// <summary>
		/// json 형식으로 구성된 file 에서 게임(Match) 정보를 읽어온다.
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		public static List<Match> GetMatchList(string filepath)
		{
			var matchHistoryJson = JObject.Parse(File.ReadAllText(filepath, Encoding.UTF8));

			return matchHistoryJson["history"].Children()
				.Select(e => JsonConvert.DeserializeObject<Match>(e.ToString()))
				.OrderBy(e => e.Date)
				.ToList();
		}
		#endregion

		#region Match (from kbo site)
		/// <summary>
		/// kbo 홈페이지에서 해당일자에 해당하는 게임정보를 크롤링해온다.
		/// </summary>
		/// <param name="date"></param>
		/// <returns></returns>
		public static List<Match> GetMatchList(int date)
		{
			var matchList = new List<Match>();
			try
			{
				#region ReadHtml
				var url = "http://www.koreabaseball.com/Schedule/ScoreBoard/ScoreBoard.aspx?gameDate={0}".With(date);
				var webRequest = WebRequest.Create(url);
				var response = webRequest.GetResponse();
				string html;
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					html = reader.ReadToEnd();
				}
				#endregion

				var htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(html);

				// 경기가 없으면 스킵!
				if (htmlDoc.DocumentNode.SelectNodes("//div[@class='smsScore']") == null)
					return matchList;

				matchList = htmlDoc.DocumentNode.SelectNodes("//div[@class='smsScore']")
					.Select(e =>
					{
						var away = e.SelectSingleNode("div[@class='score_wrap']/p[@class='leftTeam']");
						var home = e.SelectSingleNode("div[@class='score_wrap']/p[@class='rightTeam']");
						return new Match
						{
							Date = date,
							Away = away.SelectSingleNode("strong").InnerText,
							AwayScore = away.SelectSingleNode("em").InnerText.ToInt(),
							Home = home.SelectSingleNode("strong").InnerText,
							HomeScore = home.SelectSingleNode("em").InnerText.ToInt(),
						};
					})
					.ToList();
			}
			catch
			{
			}
			return matchList;
		}
		#endregion

		#region TeamMatch
		/// <summary>
		/// 각 팀별 게임을 구한다.
		/// </summary>
		/// <param name="matchList"></param>
		/// <returns></returns>
		static List<TeamMatch> GetTeamMatchList(this IEnumerable<Match> matchList)
		{
			var teamMatchList = new List<TeamMatch>();
			foreach (var match in matchList)
			{
				teamMatchList.Add(new TeamMatch
				{
					Date = match.Date,
					Team = match.Away,
					Score = match.AwayScore,
					OtherTeam = match.Home,
					OtherScore = match.HomeScore,
					IsHome = false
				});
				teamMatchList.Add(new TeamMatch
				{
					Date = match.Date,
					Team = match.Home,
					Score = match.HomeScore,
					OtherTeam = match.Away,
					OtherScore = match.AwayScore,
					IsHome = true
				});
			}
			return teamMatchList;
		}
		#endregion

		#region Standing
		/// <summary>
		/// 팀 순위, 승률, 게임차 등을 구한다.
		/// </summary>
		/// <param name="teamMatchList"></param>
		/// <returns></returns>
		static List<Standing> GetStandingList(this IEnumerable<TeamMatch> teamMatchList)
		{
			var teamList = teamMatchList.Select(e => e.Team).Distinct().ToList();
			var dateList = teamMatchList.Select(e => e.Date).Distinct().OrderBy(e => e);
			var standingList = new List<Standing>();

			foreach (var date in dateList)
			{
				// 승, 무, 패, 승률
				foreach (var team in teamList)
				{
					var standing = new Standing { Date = date, Team = team };

					var yesterday = standingList.Where(t => t.Team == team && t.Date < date).OrderBy(e => e.Date).LastOrDefault();
					standing.Win = yesterday != null ? yesterday.Win : 0;
					standing.Draw = yesterday != null ? yesterday.Draw : 0;
					standing.Lose = yesterday != null ? yesterday.Lose : 0;

					var today = teamMatchList.Where(t => t.Team == team && t.Date == date).OrderBy(e => e.Date).LastOrDefault();
					standing.Win += (today != null && today.IsWin) ? 1 : 0;
					standing.Draw += (today != null && today.IsDraw) ? 1 : 0;
					standing.Lose += (today != null && today.IsLose) ? 1 : 0;

					standing.PCT = ((double)standing.Win / (standing.Win + standing.Lose));

					standingList.Add(standing);
				}

				// 순위
				foreach (var team in standingList.Where(e => e.Date == date))
				{
					team.Rank = standingList.Where(e => e.Date == date && e.PCT > team.PCT).Count() + 1;
				}

				// 승차 (게임차)
				var rank1 = standingList.Where(e => e.Date == date && e.Rank == 1).First();
				foreach (var team in standingList.Where(e => e.Date == date))
				{
					team.GB = team.Rank == 1 ? 0 : ((rank1.Win - team.Win) - (rank1.Lose - team.Lose)) / 2.0;
				}
			}

			return standingList.OrderBy(e => e.Date).ThenBy(e => e.Rank).ToList();
		}
		#endregion
	}
}
