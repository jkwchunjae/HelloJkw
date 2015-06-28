using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw.Utils
{
	public static class Utils
	{
		public static string JQueryAjaxEncoding(this byte[] bytes)
		{
			var newBytes = new List<byte>();
			for (var i = 0; i < bytes.Count(); i++)
			{
				if (bytes[i] == 37 && i + 2 < bytes.Count() && bytes[i + 1] != 37 && bytes[i + 2] != 37)
				{
					string hex = string.Format("{0}{1}", (char)bytes[i + 1], (char)bytes[i + 2]);
					newBytes.Add((byte)int.Parse(hex, System.Globalization.NumberStyles.HexNumber));
					i += 2;
				}
				else
				{
					newBytes.Add(bytes[i]);
				}
			}
			return Encoding.UTF8.GetString(newBytes.ToArray());
		}
	}
}
