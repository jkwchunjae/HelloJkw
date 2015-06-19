using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

using Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Collections.Concurrent;

namespace helloJkw
{
	public static class KboDataManager
	{
		#region Season (from json file)
		public static List<Season> _seasonList = null;
		public static List<Season> LoadSeasonList(string filepath)
		{
			if (_seasonList == null)
			{
				var seasonInfoJson = File.ReadAllText(filepath, Encoding.UTF8);
				_seasonList = JsonConvert.DeserializeObject<List<Season>>(seasonInfoJson);
			}
			return _seasonList;
		}
		#endregion

		#region Match (from json file)
		public static List<Match> _matchList = null;
		public static List<Match> LoadMatchList(string filepath)
		{
			if (_matchList == null)
			{
				var matchHistoryJson = File.ReadAllText(filepath, Encoding.UTF8);
				_matchList = JsonConvert.DeserializeObject<List<Match>>(matchHistoryJson);
			}
			return _matchList;
		}
		#endregion

		#region Match (from kbo site)
		public static ConcurrentDictionary<int, List<Match>> _kboMatchCache = new ConcurrentDictionary<int, List<Match>>();
		public static DateTime _lastUpdateTime = DateTime.Now.AddMinutes(-100);
		public static List<Match> CrawlMatchList(int date)
		{
			List<Match> matchList;

			#region Caching
			// 3분 이내는 캐싱에서 불러온다.
			if (DateTime.Now.Subtract(_lastUpdateTime).TotalMinutes < 3.0)
			{
				// 캐싱해놓은 데이터가 있으면 그거 쓴다.
				if (_kboMatchCache.TryGetValue(date, out matchList))
				{
					return matchList;
				}
			}
			#endregion

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
				{
					matchList = new List<Match>();
					_kboMatchCache.TryAdd(date, matchList);
					return matchList;
				}

				matchList = htmlDoc.DocumentNode.SelectNodes("//div[@class='smsScore']")
					.Where(e => e.SelectSingleNode("div/strong[@class='flag']/span").InnerText != "경기전")
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
				_kboMatchCache.TryAdd(date, matchList);
				return matchList;
			}
			catch
			{
			}
			return (new List<Match>());
		}
		#endregion

		#region Save Match
		static object _updateLock = new object();
		public static void SaveMatchList(string filepath, List<Match> matchList)
		{
#if (DEBUG)
			return;
#endif
			lock (_updateLock)
			{
				var resultString = JsonConvert.SerializeObject(matchList.OrderByDescending(e => e.Date))
					.RegexReplace(@"\}\,", "},\r\n  ");

				File.WriteAllText(filepath, resultString, Encoding.UTF8);
			}
		}
		#endregion
	}
}
