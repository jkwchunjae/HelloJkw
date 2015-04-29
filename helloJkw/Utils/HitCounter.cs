using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Extensions;

namespace helloJkw.Utils
{
	class HitClass
	{
		public string Key { get; set; }
		public long Hit { get; set; }
	}

	static class HitCounter
	{
		static DateTime _lastSaveTime;
		static string _path = "jkw/db/hit.txt";
		static Dictionary<string, long> _hitDic;

		static HitCounter()
		{
			_lastSaveTime = DateTime.Now;
			var hitJson = JObject.Parse(File.ReadAllText(_path, Encoding.UTF8));

			_hitDic = hitJson["hit"].Children()
				.Select(e => JsonConvert.DeserializeObject<HitClass>(e.ToString()))
				.GroupBy(e => new { e.Key })
				.ToDictionary(e => e.Key.Key, e => e.Max(t => t.Hit));
		}

		public static void Hit(string key)
		{
			Logger.Log("ViewLog: " + key);
			lock (_hitDic)
			{
				if (!_hitDic.ContainsKey(key))
					_hitDic.Add(key, 0L);

				_hitDic[key]++;
			}

#if (DEBUG)
			Save(0);
#else
			Save();
#endif
		}

		static void Save(int saveMinute = 5)
		{
#if (DEBUG)
#else
			lock (_hitDic)
			{
				if (saveMinute > 0 && DateTime.Now.Subtract(_lastSaveTime).TotalMinutes < saveMinute)
					return;
				_lastSaveTime = DateTime.Now;

				var hitJsonArray = _hitDic
					.OrderByDescending(e => e.Value)
					.Select(e => new JObject(
						new JProperty("Key", e.Key),
						new JProperty("Hit", e.Value)
					));

				var hitJsonObject = new JObject(new JProperty("hit", hitJsonArray));

				File.WriteAllText(_path, hitJsonObject.ToString(), Encoding.UTF8);
			}
#endif
		}
	}
}
