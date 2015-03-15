using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using helloJkw.Extensions;
using System.IO;

namespace helloJkw
{
	public static class LuciaStatic
	{
		public static string RootPath = @"lucia";
		public static string RootPathWeb = @"lucia-web";
		public static string RootPathMobile = @"lucia-mobile";
		public static LuciaDirInfo LuciaDir;

		private static DateTime _lastUpdateTime;

		public static string MainDirName
		{
			get
			{
				return LuciaDir.GetDirNames().Where(e => e.Contains("main")).First();
			}
		}

		public static LuciaDirInfo UpdateLuciaDir(int minute = 10)
		{
			if (DateTime.Now.Subtract(_lastUpdateTime).TotalMinutes < minute)
				return LuciaDir;
			LuciaDir = RootPath.CreateDirInfo();
			var rootFullPath = Path.GetFullPath(RootPath).Replace(@"\", "/");
			if (rootFullPath[rootFullPath.Length - 1] != '/') rootFullPath += '/';
			ImageResizer.SyncImages(rootFullPath, "/lucia/", "/lucia-web/", ratio:0.4);
			ImageResizer.SyncImages(rootFullPath, "/lucia/", "/lucia-mobile/", ratio:0.25);
			_lastUpdateTime = DateTime.Now;
			return LuciaDir;
		}

		public static IEnumerable<string> GetMainMenu()
		{
			return LuciaDir.GetDirNames()
				.Where(e => e != MainDirName)
				.Select(e => e.RemovePrefixNumber());
		}
	}

	public static class StaticRandom
	{
		public static Random random = new Random((int)DateTime.Now.Ticks);
		
		public static int Next(int maxValue)
		{
			return random.Next(maxValue);
		}
		
		public static int Next(int minValue, int maxValue)
		{
			return random.Next(minValue, maxValue);
		}
	}
}
