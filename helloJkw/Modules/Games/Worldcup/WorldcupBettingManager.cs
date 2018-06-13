using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw.Game.Worldcup
{
    static class WorldcupBettingManager
    {
        static string _rootPath = @"jkw/games/Worldcup/BettingData";
        static List<BettingData> _bettingDataList = new List<BettingData>();

        public static List<GroupData> GroupDataList = new List<GroupData>();

        static WorldcupBettingManager()
        {
        }

        public static void Load()
        {
            _bettingDataList = Directory.GetFiles(_rootPath, "*.json")
                .Select(x => File.ReadAllText(x, Encoding.UTF8))
                .Select(x => JsonConvert.DeserializeObject<BettingData>(x))
                .ToList();

            Update16TargetData();
        }

        static void Save(this BettingData bettingData)
        {
            var path = Path.Combine(_rootPath, $"{bettingData.BettingName.Replace(" ", "")}.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(bettingData, Formatting.Indented), Encoding.UTF8);
        }

        public static BettingData GetBettingData(string bettingName)
        {
            return _bettingDataList.FirstOrDefault(x => x.BettingName == bettingName);
        }

        public static BettingData UpdateData(this BettingData bettingData)
        {
            if (_bettingDataList.Any(x => x.BettingName == bettingData.BettingName))
            {
                _bettingDataList = _bettingDataList
                    .Select(x => x.BettingName == bettingData.BettingName ? bettingData : x)
                    .ToList();
            }
            else
            {
                _bettingDataList.Add(bettingData);
            }
            return bettingData;
        }

        public static BettingData RecalcMatchData(this BettingData bettingData, bool checkId)
        {
            foreach (var userData in bettingData.UserBettingList.SelectMany(x => x.Value.BettingList))
            {
                if (checkId)
                {
                    var target = bettingData.TargetList.First(x => x.Id == userData.Id);
                    userData.IsMatched = userData.Value == target.Value;
                }
                else
                {
                    userData.IsMatched = bettingData.TargetList.Any(x => x.Value == userData.Value);
                }
            }
            bettingData.Save();
            return bettingData;
        }

        public static bool UpdateUserBettingData(this BettingData bettingData, string username, List<UserBetting> userBettings)
        {
            if (bettingData.FreezeTime < DateTime.Now)
                return false;

            if (bettingData.UserBettingList.ContainsKey(username))
            {
                bettingData.UserBettingList[username].BettingList = userBettings;
            }
            else
            {
                bettingData.UserBettingList[username] = new UserBettingData(username, userBettings);
            }

            bettingData.RecalcMatchData(false);
            return true;
        }

        public static double CalcMyBettingRatio(this BettingData bettingData, string username)
        {
            if (!bettingData.UserBettingList.ContainsKey(username))
                return 0;

            var myBettingData = bettingData.UserBettingList[username];

            var myPoint = myBettingData
                .BettingList
                .Where(x => x.IsMatched)
                .Select(x => bettingData.TargetList.First(e => e.Id == x.Id).Weight)
                .Sum();

            var allPoint = bettingData.UserBettingList
                .Where(x => x.Value.BettingGroup == myBettingData.BettingGroup)
                .SelectMany(x => x.Value.BettingList)
                .Where(x => x.IsMatched)
                .Select(x => bettingData.TargetList.First(e => e.Id == x.Id).Weight)
                .Sum();

            if (allPoint == 0)
                return 0;

            return myPoint / allPoint;
        }

        public static void Update16TargetData()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    GroupDataList = await GetGroupResultAsync();
                    var bettingName = "16강 맞추기";
                    var bettingData = _bettingDataList.First(x => x.BettingName == bettingName);

                    foreach (var teamData in GroupDataList.SelectMany(x => x.TeamDataList.Where(e => e.Rank <= 2).Select(e => new { x.GroupName, TeamData = e })))
                    {
                        var id = $"{teamData.GroupName.Replace("Group ", "")}{teamData.TeamData.Rank}";
                        bettingData.TargetList.First(x => x.Id == id).Value = teamData.TeamData.TeamCode;
                    }

                    bettingData.RecalcMatchData(false);
                    await Task.Delay(600 * 1000);
                }
            });
        }

        static async Task<List<GroupData>> GetGroupResultAsync()
        {
            var url = "https://www.fifa.com/worldcup/groups";
            var request = WebRequest.CreateHttp(url);

            var response = await request.GetResponseAsync();
            var html = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            return htmlDoc.DocumentNode.SelectNodes("//table[@class='fi-table fi-standings']")
                .Select(x =>
                {
                    var groupName = x.SelectSingleNode(".//p[@class='fi-table__caption__title']").InnerText;
                    var teamDataList = x.SelectNodes(".//tbody/tr")
                        .Where(e => e.Attributes.Any(a => a.Name == "data-team-id"))
                        .Select((e, i) =>
                        {
                            var name = e.SelectSingleNode(".//div[@class='fi-t__n']").ChildNodes[1].InnerText;
                            var code = e.SelectSingleNode(".//div[@class='fi-t__n']").ChildNodes[3].InnerText;
                            //var picture = e.SelectSingleNode(".//img").Attributes.First(r => r.Name == "src").Value;
                            var point = e.SelectSingleNode(".//td[@class='fi-table__pts']/span").InnerText;
                            return new GroupTeamData
                            {
                                TeamName = name.Trim(),
                                TeamCode = code.Trim(),
                                TeamPicture = $"https://img.fifa.com/images/flags/4/{code.ToLower()}.png",
                                Rank = i + 1,
                                Point = int.Parse(point),
                            };
                        })
                        .ToList();
                    return new GroupData
                    {
                        GroupName = groupName,
                        TeamDataList = teamDataList,
                    };
                })
                .ToList();
        }
    }

    class BettingData
    {
        public string BettingName { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime FreezeTime { get; set; }
        public List<Target> TargetList { get; set; }
        public Dictionary<string /* username */, UserBettingData> UserBettingList { get; set; }

        [JsonIgnore]
        public bool IsOpen => OpenTime >= DateTime.Now;
        [JsonIgnore]
        public bool IsFreeze => FreezeTime >= DateTime.Now;
        [JsonIgnore]
        public bool IsEditable => IsOpen && !IsFreeze;

        public BettingData()
        {
        }

        public BettingData(BettingData bettingData)
        {
            BettingName = bettingData.BettingName;
            OpenTime = bettingData.OpenTime;
            FreezeTime = bettingData.FreezeTime;
            TargetList = bettingData.TargetList;
            UserBettingList = bettingData.UserBettingList;
        }
    }

    class Target
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public double Weight { get; set; }
    }

    class UserBettingData
    {
        public string Username { get; set; }
        public int BettingAmount { get; set; } = 0;
        public string BettingGroup { get; set; } = "";
        public List<UserBetting> BettingList { get; set; }

        public UserBettingData() { }

        public UserBettingData(string username, List<UserBetting> bettingList)
        {
            Username = username;
            BettingList = BettingList;
        }
    }

    class UserBetting
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public bool IsMatched { get; set; }
    }

    public class GroupData
    {
        public string GroupName { get; set; }
        public List<GroupTeamData> TeamDataList { get; set; }

        [JsonIgnore]
        public string GroupCode => GroupName.ToUpper().Replace("GROUP", "").Trim();
    }

    public class GroupTeamData
    {
        public string TeamName { get; set; }
        public string TeamCode { get; set; }
        public string TeamPicture { get; set; }
        public int Rank { get; set; }
        public int Point { get; set; }
    }
}
