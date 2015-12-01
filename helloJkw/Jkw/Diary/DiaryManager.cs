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
		public bool IsSecure;
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


		public static IEnumerable<Diary> LoadDiaryAll(string diaryName, bool reload = false)
		{
			// upgradable read lock
			if (!_diaryDic.ContainsKey(diaryName) || reload)
			{
				// write lock
				_diaryDic.Remove(diaryName);
				var currentPath = Path.Combine(_rootPath, diaryName);
				if (!Directory.Exists(currentPath))
					return new List<Diary>();
				var diaryList = Directory.GetFiles(currentPath)
					.Select(x => new Diary(x));
				_diaryDic.Add(diaryName, diaryList);
			}
			return _diaryDic[diaryName];
		}

		public static IEnumerable<Diary> LoadDiaryByDate(string diaryName, DateTime beginDate, DateTime endDate, bool withSecure, bool reload = false)
		{
			return LoadDiaryAll(diaryName, reload)
				.Where(x => x.Date.Date >= beginDate.Date && x.Date.Date <= endDate.Date)
				.Where(x => withSecure ? true : !x.IsSecure);
		}

		public static IEnumerable<Diary> GetDiary(string diaryName, DateTime date, bool withSecure)
		{
			return LoadDiaryByDate(diaryName, date, date, withSecure);
		}
	}
}
