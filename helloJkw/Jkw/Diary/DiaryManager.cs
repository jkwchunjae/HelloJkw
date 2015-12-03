using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Newtonsoft.Json;

namespace helloJkw
{
	public class Diary
	{
		public DateTime Date;
		public DateTime RegDate;
		public DateTime LastModifyDate;
		public bool IsSecure;
		public string Text;
	}

	public static class DiaryManager
	{
		static string _rootPath = @"jkw/project/diary";
		static Dictionary<string /* diaryName */, IEnumerable<Diary>> _diaryDic = new Dictionary<string, IEnumerable<Diary>>();


		private static IEnumerable<Diary> LoadDiaryAll(string diaryName, bool reload = false)
		{
#if DEBUG
			reload = true;
#endif
			// upgradable read lock
			if (!_diaryDic.ContainsKey(diaryName) || reload)
			{
				// write lock
				_diaryDic.Remove(diaryName);
				var currentPath = Path.Combine(_rootPath, diaryName);
				if (!Directory.Exists(currentPath))
					return new List<Diary>();
				var diaryList = Directory.GetFiles(currentPath)
					.Select(x => JsonConvert.DeserializeObject<Diary>(File.ReadAllText(x, Encoding.UTF8)))
					.OrderBy(x => x.Date)
					.ToList();
				_diaryDic.Add(diaryName, diaryList);
			}
			return _diaryDic[diaryName];
		}

		private static IEnumerable<Diary> LoadDiaryByDate(string diaryName, DateTime beginDate, DateTime endDate, bool withSecure, bool reload = false)
		{
			return LoadDiaryAll(diaryName, reload)
				.Where(x => x.Date.Date >= beginDate.Date && x.Date.Date <= endDate.Date)
				.Where(x => withSecure ? true : !x.IsSecure);
		}

		public static IEnumerable<Diary> GetDiary(string diaryName, DateTime date, bool withSecure)
		{
			return LoadDiaryByDate(diaryName, date, date, withSecure);
		}

		public static IEnumerable<Diary> GetLastDiary(string diaryName, bool withSecure)
		{
			var lastDiary = LoadDiaryAll(diaryName)
				.Where(x => withSecure ? true : !x.IsSecure)
				.Last();
			return GetDiary(diaryName, lastDiary.Date, withSecure);
		}

		public static DateTime GetPrevDate(string diaryName, DateTime date, bool withSecure)
		{
			var list = LoadDiaryAll(diaryName)
				.Where(x => withSecure ? true : !x.IsSecure)
				.Where(x => x.Date.Date < date.Date);

			return list.Any() ? list.Max(x => x.Date) : DateTime.MinValue;
		}

		public static DateTime GetNextDate(string diaryName, DateTime date, bool withSecure)
		{
			var list = LoadDiaryAll(diaryName)
				.Where(x => withSecure ? true : !x.IsSecure)
				.Where(x => x.Date.Date > date.Date);

			return list.Any() ? list.Min(x => x.Date) : DateTime.MinValue;
		}
	}
}
