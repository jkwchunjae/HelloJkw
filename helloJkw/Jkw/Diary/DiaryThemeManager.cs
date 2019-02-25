using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
    public static class DiaryThemeManager
    {
        private static string _themePath = @"jkw/db/diaryThemes.json";
        private static List<DiaryTheme> _themeList = new List<DiaryTheme>();

        static DiaryThemeManager()
        {
            Reload();
        }

        static List<DiaryTheme> Load()
            => JsonConvert.DeserializeObject<List<DiaryTheme>>(File.ReadAllText(_themePath, Encoding.UTF8));

        static void Save()
            => File.WriteAllText(_themePath, JsonConvert.SerializeObject(_themeList, Formatting.Indented), Encoding.UTF8);

        static List<DiaryTheme> Update(DiaryTheme theme)
            => _themeList
            .Where(x => x.Name != theme.Name)
            .Concat(new[] { theme })
            .ToList();

        public static void Reload()
        {
            _themeList = Load();
        }

        public static void AddOrUpdate(DiaryTheme theme)
        {
            _themeList = Update(theme);
            Save();
        }

        public static DiaryTheme GetTheme(string name)
            => _themeList
            .FirstOrDefault(x => x.Name == name) ?? new DiaryTheme();

        public static List<DiaryTheme> GetAllThemes()
            => _themeList;
    }
}
