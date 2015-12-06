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
		public int Index;
	}

	public static class DiaryManager
	{
		static string _rootPath = @"jkw/project/diary";
		const string _diaryExt = "diary";
		static Dictionary<string /* diaryName */, IEnumerable<Diary>> _diaryDic = new Dictionary<string, IEnumerable<Diary>>();

		#region Load & Get
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

				var diaryList = Directory.GetFiles(currentPath, "*." + _diaryExt)
					.Select(x => new { Diary = JsonConvert.DeserializeObject<Diary>(File.ReadAllText(x, Encoding.UTF8)), FileName = Path.GetFileNameWithoutExtension(x) })
					.Select(x => new { x.Diary, FileIndex = x.FileName.Substring(9, x.FileName.Length - 9 - (1 + _diaryExt.Length)).ToInt()})
					.OrderBy(x => x.Diary.Date)
					.ToList();

				if (diaryList.Where(x => x.Diary.Index != x.FileIndex).Any())
				{
					// 다이어리에 기록된 파일명과 실재 파일명이 다르다.
					// 어떻게 해야하지?
				}

				_diaryDic.Add(diaryName, diaryList.Select(x => x.Diary));
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
		#endregion

		#region Write & Modify
		public static Diary WriteDiary(string diaryName, DateTime date, string text, bool isSecure)
		{
			// write lock
			// 쓰기할땐 항상 다시 로드 하자
			var diaryIndex = LoadDiaryByDate(diaryName, date, date, withSecure: true, reload: true)
				.Select(x => x.FileName.Substring(9, x.FileName.Length - 9 - (_diaryExt.Length + 1)).ToInt())
				.DefaultIfEmpty(0)
				.Max() + 1;
			var fileName = "{date}_{index}.{ext}".WithVar(new { date = date.ToInt(), index = diaryIndex, ext = _diaryExt });

			var diary = new Diary
			{
				Date = date,
				RegDate = DateTime.Now,
				LastModifyDate = DateTime.Now,
				IsSecure = isSecure,
				Text = text,
				FileName = fileName,
			};

			var json = JsonConvert.SerializeObject(diary);

			File.WriteAllText(Path.Combine(_rootPath, diaryName, fileName), json, Encoding.UTF8);

			return diary;
		}
		#endregion
	}
}
