using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
    public class ThemeInfoAttribute : Attribute
    {
        public string CssName { get; set; }

        public ThemeInfoAttribute(string cssName)
        {
            CssName = cssName;
        }
    }
}
