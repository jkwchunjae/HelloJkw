using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.Collections.Concurrent;

namespace helloJkw
{
	public class TelestrationsModule : JkwModule
	{
		public TelestrationsModule()
		{
			Get["/games/Telestrations"] = _ =>
			{
				Model.KeywordList = Telestrations.GetKeyword(session, 3);
				return View["Games/Telestrations/telestrationsMain.cshtml", Model];
			};

			Post["/games/Telestrations/reset"] = _ =>
			{
				Telestrations.Reset();
				return "";
			};
		}
	}

	public static class Telestrations
	{
		static List<string> _keywordList;
		static Dictionary<string, List<string>> _sessionKeywords;
		static HashSet<string> _usedKeywords;

		static Telestrations()
		{
			_keywordList = File.ReadAllLines(@"jkw/games/Telestrations/keywords.txt", Encoding.UTF8).ToList();
			_sessionKeywords = new Dictionary<string, List<string>>();
			_usedKeywords = new HashSet<string>();
		}

		public static void Reset()
		{
			lock (_keywordList)
			{
				_sessionKeywords.Clear();
			}
		}

		public static List<string> GetKeyword(Session session, int count)
		{
			lock (_keywordList)
			{
				if (_usedKeywords.Count() > _keywordList.Count() * 0.5)
				{
					_usedKeywords.Clear();
					_sessionKeywords.Clear();
				}

				if (!_sessionKeywords.ContainsKey(session.SessionId))
				{
					_sessionKeywords.Add(session.SessionId, new List<string>());
				}

				while (_sessionKeywords[session.SessionId].Count() < count)
				{
					var keyword = _keywordList.GetRandom();
					if (_usedKeywords.Contains(keyword))
						continue;
					_sessionKeywords[session.SessionId].Add(keyword);
					_usedKeywords.Add(keyword);
				}
			}
			return _sessionKeywords[session.SessionId];
		}
	}
}
