using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public static class LuciaStatic
	{
		public static string RootPath;
		public static DirInfo LuciaDir;

		public static string MainDirName
		{
			get
			{
				return LuciaDir.GetDirNames().Where(e => e.Contains("main")).First();
			}
		}

		//public IEnumerable<string>
	}
}
