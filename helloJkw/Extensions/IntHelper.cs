using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw.Extensions
{
	public static class IntHelper
	{
		public static string ToComma(this int value)
		{
			return "{0:#,#}".With(value);
		}
	}
}
