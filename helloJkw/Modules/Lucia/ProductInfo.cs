using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw.Modules.Lucia
{
	public class ProductInfo
	{
		public string Name;
		public int Price;
		public int SalePrice;

		public ExpandoObject ToExpando()
		{
			IDictionary<string, object> expando = new ExpandoObject();
			var type = this.GetType();
			foreach (var field in type.GetFields())
				expando.Add(field.Name, field.GetValue(this));

			return (ExpandoObject)expando;
		}
	}
}
