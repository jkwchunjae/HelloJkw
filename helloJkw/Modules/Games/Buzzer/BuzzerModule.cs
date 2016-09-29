using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Extensions;
using Newtonsoft.Json.Linq;

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
		static string _infoPath = @"jkw/games/Buzzer/info.txt";

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

			dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(_infoPath, Encoding.UTF8));

			var managerSet = ((JArray)obj.manager).Select(x => (string)x).ToList()
				.ToHashSet();

			return managerSet.Contains(session.User.Email);
		}

		/// <summary> 로그인을 해야만 시스템을 이용할 수 있는지 알기 위한 함수 </summary>
		public static bool MustLogin()
		{
			dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(_infoPath, Encoding.UTF8));
			return (bool)obj.login;
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
				Model.MustLogin = BuzzerHelper.MustLogin();
				return View["Games/Buzzer/buzzerMain.cshtml", Model];
			};

			Post["/games/buzzer/push"] = _ =>
			{
				var mustLogin = BuzzerHelper.MustLogin();
				if (mustLogin && !session.IsLogin)
					return "";
				var userName = mustLogin ? session.User.Name : (string)Request.Form["userName"];

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
