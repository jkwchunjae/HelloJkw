using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
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
			foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(anonymousObject))
			{
				var obj = propertyDescriptor.GetValue(anonymousObject);
				expando.Add(propertyDescriptor.Name, obj);
			}
			return (ExpandoObject)expando;
		}
	}
}
