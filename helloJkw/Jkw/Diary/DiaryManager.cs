using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw
{
	public class Diary
	{
		public DateTime Date;
		public DateTime RegDate;
		public DateTime LastModifyDate;
		public string Text;

		public Diary(string path)
		{
			var filename = Path.GetFileName(path);
			Date = filename.Left(8).ToDate();
			Text = File.ReadAllText(path, Encoding.UTF8);
		}
	}

	public static class DiaryManager
	{
		static string _rootPath = @"jkw/project/diary";
		static Dictionary<string /* diaryName */, IEnumerable<Diary>> _diaryDic = new Dictionary<string, IEnumerable<Diary>>();


		public static IEnumerable<Diary> LoadDiaryAll(User user, bool reload = false)
		{
			// upgradable read lock
			if (!_diaryDic.ContainsKey(user.DiaryName) || reload)
			{
				// write lock
				_diaryDic.Remove(user.DiaryName);
				var currentPath = Path.Combine(_rootPath, user.DiaryName);
				if (!Directory.Exists(currentPath))
					return new List<Diary>();
				var diaryList = Directory.GetFiles(currentPath)
					.Select(x => new Diary(x));
				_diaryDic.Add(user.DiaryName, diaryList);
			}
			return _diaryDic[user.DiaryName];
		}

		public static IEnumerable<Diary> LoadDiaryByDate(User user, DateTime beginDate, DateTime endDate, bool reload = false)
		{
			return LoadDiaryAll(user, reload)
				.Where(x => x.Date.Date >= beginDate.Date && x.Date.Date <= endDate.Date);
		}

		public static IEnumerable<Diary> GetDiary(User user, DateTime date)
		{
			return LoadDiaryByDate(user, date, date);
		}
	}
}
