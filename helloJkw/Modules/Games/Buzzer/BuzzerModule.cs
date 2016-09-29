using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Extensions;

namespace helloJkw
{
	class Record
	{
		public string UserName;
		DateTime _pushTime;
		public int Diff;

		public Record(string userName)
		{
			UserName = userName;
			_pushTime = DateTime.Now;
			Diff = 0;
		}

		public Record(string userName, Record firstRecord)
		{
			UserName = userName;
			_pushTime = DateTime.Now;
			Diff = (int)_pushTime.Subtract(firstRecord._pushTime).TotalMilliseconds;
		}
	}

	public static class BuzzerHelper
	{
		static string _managerPath = @"jkw/games/Buzzer/manager.txt";

		static List<Record> _recordList = new List<Record>();
		static Record _firstRecord;
		static object _lockObj = new object();
		public static string Push(string userName)
		{
			lock (_lockObj)
			{
				if (_recordList.Any() == false)
				{
					_firstRecord = new Record(userName);
					_recordList.Add(_firstRecord);
				}
				else
				{
					_recordList.Add(new Record(userName, _firstRecord));
				}
				return JsonConvert.SerializeObject(_recordList);
			}
		}

		public static void Reset()
		{
			lock(_lockObj)
			{
				_recordList.Clear();
				_firstRecord = null;
			}
		}

		public static bool IsManager(Session session)
		{
#if DEBUG
			return true;
#endif
			if (session == null)
				return false;

			if (!session.IsLogin)
				return false;

			var managerSet = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(_managerPath, Encoding.UTF8))
				.ToHashSet();

			return managerSet.Contains(session.User.Email);
		}
	}

	public class BuzzerModule : JkwModule
	{
		public BuzzerModule()
		{
			Get["/games/Buzzer"] = _ =>
			{
#if DEBUG
				var userName = "TESTER";
#else
				var userName = session.IsLogin ? session.User.Name : "TESTER";
#endif

				Model.UserName = userName;
				Model.IsManager = BuzzerHelper.IsManager(session);
				return View["Games/Buzzer/buzzerMain.cshtml", Model];
			};

			Post["/games/buzzer/push"] = _ =>
			{
				var userName = "";
#if DEBUG
				userName = "TESTER";
#else
				if (!session.IsLogin)
					return "";
				userName = session.User.Name;
#endif

				return BuzzerHelper.Push(userName);
			};

			Post["/games/buzzer/reset"] = _ =>
			{
#if DEBUG
#else
				if (!session.IsLogin)
					return "";
#endif
				if (BuzzerHelper.IsManager(session))
					BuzzerHelper.Reset();
				return "";
			};
		}
	}
}
