using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace helloJkw.Extensions
{
	public static class Extensions
	{
		public static string RemovePrefixNumber(this string filename)
		{
			return Regex.Replace(filename, @"\d*\.\s", "");
		}

		public static ExpandoObject ToExpando(this object anonymousObject)
		{
			IDictionary<string, object> expando = new ExpandoObject();
			var type = anonymousObject.GetType();
			foreach (var field in type.GetFields())
				expando.Add(field.Name, field.GetValue(anonymousObject));
			foreach (var properity in type.GetProperties())
				expando.Add(properity.Name, properity.GetValue(anonymousObject));

			return (ExpandoObject)expando;
		}

		public static Dictionary<string, string> InfoToDictionary(this string filepath)
		{
			var infoDic = new Dictionary<string, string>();
			string currentKey = null;
			foreach (var line in File.ReadAllLines(filepath, Encoding.Default))
			{
				if (line.Contains('='))
				{
					var splitted = line.Split('=');
					currentKey = splitted[0].Trim();
					string value = "";
					if (splitted.Count() > 1)
						value = splitted[1];
					infoDic.Add(currentKey, value);
				}
				else
				{
					infoDic[currentKey] = (infoDic[currentKey] += Environment.NewLine + line).Trim();
				}
			}
			return infoDic;
		}
	}
}
