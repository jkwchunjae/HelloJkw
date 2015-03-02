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
		public static DirInfo LuciaDir;

		public static string MainDirName
		{
			get
			{
				return LuciaDir.GetDirNames().Where(e => e.Contains("main")).First();
			}
		}

		public static IEnumerable<string> GetMainMenu()
		{
			return LuciaDir.GetDirNames()
				.Where(e => e != MainDirName)
				.Select(e => e.RemovePrefixNumber());
		}
	}
}
