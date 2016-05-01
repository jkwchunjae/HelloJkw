using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace helloJkw.Jkw.Others.FnB
{
	public static class FnbMeeting
	{
		public class Meeting
		{
			/// <summary> 회차 </summary>
			public int No;
			public DateTime Date;
			public List<string> Attendants;
			public string Others;
		}

		static string _path = @"jkw/project/fnb/json/meeting.json";
		static List<Meeting> _meetingList;

		static FnbMeeting()
		{
			Load();
		}

		#region Load & Save

		static void Load()
		{
			try
			{
				_meetingList = JsonConvert.DeserializeObject<List<Meeting>>(File.ReadAllText(_path, Encoding.UTF8));
			}
			catch
			{
				_meetingList = new List<Meeting>();
			}
		}

		static bool Save()
		{
			try
			{
				string json = JsonConvert.SerializeObject(_meetingList, Formatting.Indented);
				File.WriteAllText(_path, json, Encoding.UTF8);
				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		public static IEnumerable<Meeting> GetMeeting()
		{
			return _meetingList.OrderByDescending(x => x.No);
		}

		public static void AddMeeting(Meeting newMeeting)
		{
			if (_meetingList.Any(x => x.Date.Date == newMeeting.Date.Date))
				throw new Exception("중복된 날짜가 있습니다.");

			if (_meetingList.Any(x => x.No == newMeeting.No))
				throw new Exception("중복된 회차가 있습니다.");


			_meetingList.Add(newMeeting);
			if (!Save())
			{
				_meetingList.Remove(newMeeting);
				throw new Exception("파일 저장에 실패했습니다.");
			}
		}

		public static void DeleteMeeting(DateTime date)
		{
			if (!_meetingList.Any(x => x.Date.Date == date.Date))
				throw new Exception("없는 날짜입니다.");

			var meeting = _meetingList.First(x => x.Date.Date == date.Date);
			_meetingList.Remove(meeting);
			if (!Save())
			{
				_meetingList.Add(meeting);
				throw new Exception("파일 저장에 실패했습니다.");
			}
		}
	}
}
