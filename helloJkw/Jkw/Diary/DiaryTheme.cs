using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw
{
    public class DiaryTheme
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        [ThemeInfo("background")]
        public string Background { get; set; }
        [ThemeInfo("background")]
        public string DiaryBackground { get; set; }
        [ThemeInfo("")]
        public string FontLink { get; set; }
        [ThemeInfo("font-family")]
        public string DiaryFont { get; set; }
        [ThemeInfo("font-size")]
        public string DiaryFontSize { get; set; }
        [ThemeInfo("color")]
        public string DiaryFontColor { get; set; }
        [ThemeInfo("line-height")]
        public string DiaryLineHeight { get; set; }

        public string CssText(string name)
        {
            var type = typeof(DiaryTheme);
            var cssName = type.GetProperty(name)
                .GetAttribute<ThemeInfoAttribute>()
                .CssName;
            var value = (string)type.GetProperty(name).GetValue(this);
            return string.IsNullOrWhiteSpace(value) ? "" :
                string.IsNullOrWhiteSpace(cssName) ? value :
                $"{cssName}: {value};";
        }

        public static List<string> Titles()
        {
            var type = typeof(DiaryTheme);
            return type.GetProperties()
                .Where(x => x.HasAttribute<ThemeInfoAttribute>())
                .Select(x => x.Name)
                .ToList();
        }
    }
}
