using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using helloJkw.Extensions;

namespace helloJkw
{
	public static class LuciaStatic
	{
		public static string RootPath = @"lucia";
		public static LuciaDirInfo LuciaDir;
		public static DateTime LastUpdateTime { get; set; }

		public static string MainDirName
		{
			get
			{
				return LuciaDir.GetDirNames().Where(e => e.Contains("main")).First();
			}
		}

		public static LuciaDirInfo UpdateLuciaDir(int minute = 0)
		{
			if (DateTime.Now.Subtract(LastUpdateTime).TotalMinutes < minute)
				return LuciaDir;
			LuciaDir = RootPath.CreateDirInfo();
			LastUpdateTime = DateTime.Now;
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
