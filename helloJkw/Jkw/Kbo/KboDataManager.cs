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

		#region StandingList (caching)
		public static List<Standing> LoadStandingList(string filepath)
		{
			var json = File.ReadAllText(filepath, Encoding.UTF8);
			return JsonConvert.DeserializeObject<List<Standing>>(json);
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
				var data = new Dictionary<string, string>();
				data["__ASYNCPOST"] = @"true";
				data["__EVENTARGUMENT"] = @"";
				data["__EVENTTARGET"] = @"ctl00%24ctl00%24cphContainer%24cphContents%24btnCalendarSelect";
				data["__EVENTVALIDATION"] = @"%2FwEdAAb%2BoIlBCtNTRD6wl%2BbqGE%2FpEG4FQbqeMQMc81CqbOAKuzLjeDrPSz9QBWoiuGg97q%2FJoS5f1T9NhbVQDKOc%2BSILE4seC6JQTftOIuy7Cku2br0XPyMYYAm84yfpHD9ObZqPnKjnezLQAQzYIQeH57lZuNKhlEJd6KGbi9me1bHt3A%3D%3D";
				data["__VIEWSTATE"] = @"%2FwEPDwULLTEwNjQ3MjM0NTAPZBYCZg9kFgJmD2QWAgIDD2QWBgIBDxYCHgRUZXh0BQoy7JuUIDEz7J28ZAICDxYCHgtfIUl0ZW1Db3VudAIBFgJmD2QWAmYPFQQKMjAxMy0wMi0xMwIxMxcyMDEz64WEIO2TqOyymOyKpOumrC4uLiI07JuUIDLsnbwg6rCc66eJLiDrtoHrtoDrpqzqt7ggLi4uZAIDD2QWAgIDD2QWAgIDD2QWAmYPZBYGAgEPDxYCHwAFDzIwMTYuMDIuMTMo7YagKWRkAgMPFgIfAWZkAgQPDxYCHgdWaXNpYmxlZ2RkGAEFHl9fQ29udHJvbHNSZXF1aXJlUG9zdEJhY2tLZXlfXxYCBS9jdGwwMCRjdGwwMCRjcGhDb250YWluZXIkY3BoQ29udGVudHMkYnRuUHJlRGF0ZQUwY3RsMDAkY3RsMDAkY3BoQ29udGFpbmVyJGNwaENvbnRlbnRzJGJ0bk5leHREYXRlv9eyPfeyr7XQvXeI89UEILGbhWDknZTv6j7044gPr%2B0%3D";
				data["__VIEWSTATEGENERATOR"] = @"50BA479B";
				data["ctl00%24ctl00%24cphContainer%24cphContents%24hfSearchDate"] = date.ToString();
				data["ctl00%24ctl00%24cphContainer%24ScriptManager1"] = @"ctl00%24ctl00%24cphContainer%24cphContents%24udpRecord%7Cctl00%24ctl00%24cphContainer%24cphContents%24btnCalendarSelect";
				data["ctl00%24ctl00%24txtSearchWord"] = @"";

				var param = data.Select(x => "{0}={1}".With(x.Key, x.Value)).StringJoin("&");
				var paramBytes = Encoding.ASCII.GetBytes(param);

				var request = WebRequest.CreateHttp(@"http://www.koreabaseball.com/Schedule/ScoreBoard/ScoreBoard.aspx");

				request.Method = "POST";
				request.Accept = @"*/*";
				//request.Connection = "Keep-Alive";
				request.ContentType = @"application/x-www-form-urlencoded; charset=utf-8";
				request.Host = @"www.koreabaseball.com";
				request.CookieContainer = new CookieContainer();
				request.CookieContainer.Add(new Cookie("ASP.NET_SessionId", "qnfu3aumr5wiwtcwkgglw5ui") { Domain = request.Host });
				request.CookieContainer.Add(new Cookie("_ga", "GA1.2.610003585.1455356327") { Domain = request.Host });
				request.CookieContainer.Add(new Cookie("_gat", "1") { Domain = request.Host });
				request.UserAgent = @"Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 10.0; Win64; x64; Trident/8.0; .NET4.0C; .NET4.0E; .NET CLR 2.0.50727; .NET CLR 3.0.30729; .NET CLR 3.5.30729; SLCC2; Media Center PC 6.0; Tablet PC 2.0)";
				request.Referer = @"http://www.koreabaseball.com/Schedule/ScoreBoard/ScoreBoard.aspx";

				using (var stream = request.GetRequestStream())
				{
					stream.Write(paramBytes, 0, paramBytes.Length);
				}

				var response = (HttpWebResponse)request.GetResponse();

				var html = new StreamReader(response.GetResponseStream()).ReadToEnd();
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
#else
			lock (_updateLock)
			{
				var resultString = JsonConvert.SerializeObject(matchList.OrderByDescending(e => e.Date))
					.RegexReplace(@"\}\,", "},\r\n  ");

				File.WriteAllText(filepath, resultString, Encoding.UTF8);
			}
#endif
		}
		#endregion
	}
}
