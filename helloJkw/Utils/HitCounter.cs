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
		public int Date { get; set; }
		public string Key { get; set; }
		public long Hit { get; set; }
	}

	static class HitCounter
	{
		static DateTime _lastSaveTime;
		static string _path = "jkw/db/hit.txt";
		static Dictionary<Tuple<int, string>, long> _hitDic;

		static HitCounter()
		{
			_lastSaveTime = DateTime.Now;
			var hitJson = File.ReadAllText(_path, Encoding.UTF8);

			_hitDic = JsonConvert.DeserializeObject<List<HitClass>>(hitJson)
				.GroupBy(e => new { e.Date, e.Key })
				.ToDictionary(e => Tuple.Create(e.Key.Date, e.Key.Key), e => e.Max(t => t.Hit));
		}

		public static void Hit(string key)
		{
			int date = DateTime.Today.ToInt();
			Logger.Log("ViewLog: " + key);
			lock (_hitDic)
			{
				var tuple = Tuple.Create(date, key);
				if (!_hitDic.ContainsKey(tuple))
					_hitDic.Add(tuple, 0L);

				_hitDic[tuple]++;
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

				var jsonHit = JsonConvert.SerializeObject(
					_hitDic
						.Select(e => new { Date = e.Key.Item1, Key = e.Key.Item2, Hit = e.Value })
						.OrderByDescending(e => e.Date)
						.ThenByDescending(e => e.Hit)
						);

				File.WriteAllText(_path, jsonHit, Encoding.UTF8);
			}
#endif
		}
	}
}
