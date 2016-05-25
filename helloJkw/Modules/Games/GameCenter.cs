using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw
{
	public static class GameCenter
	{
		static string _rootPath = "jkw/games";
		static string _staticPath = "jkw/static/games/";
		static Dictionary<string /* engName = dirName */, GameInfo> _cacheGameInfo;
		static DateTime _lastUpdate;

		static GameCenter()
		{
			_lastUpdate = DateTime.Now;
			_cacheGameInfo = new Dictionary<string, GameInfo>();
			UpdateGameList(0);
		}

		public static IEnumerable<GameInfo> GetGameList()
		{
			UpdateGameList();
			return _cacheGameInfo.Select(e => e.Value);
		}

		static void UpdateGameList(int expiredMinute = 99999999)
		{
			lock (_cacheGameInfo)
			{
				if (expiredMinute != 0) return;
				if (_cacheGameInfo.Count() > 0) return;
				var games = Directory.GetDirectories(_rootPath)
					.Select(path => new { engName = Path.GetFileName(path), path })
					.Select(e => new { e.engName, json = (dynamic)JsonConvert.DeserializeObject(File.ReadAllText(Path.Combine(e.path, "info.txt"))) })
					.Select(e => new { e.engName, korName = e.json.korName, isPublish = (bool)e.json.isPublish, e.json, thumbnail = Directory.GetFiles(Path.Combine(_staticPath, e.engName), "*thumbnail*").GetRandom().ReplaceToSlash().ReplaceJkwStatic() })
					.Where(x => x.isPublish)
					.ToList();

				_cacheGameInfo = games.ToDictionary(e => e.engName, e => new GameInfo() { EngName = e.engName, KorName = e.korName, Thumbnail = e.thumbnail, JsonObj = e.json });
			}
		}
	}

	public class GameInfo
	{
		public string EngName;
		public string KorName;
		public string Thumbnail;
		public dynamic JsonObj;
	}
}
