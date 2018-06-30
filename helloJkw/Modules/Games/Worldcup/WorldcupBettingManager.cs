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
        static string _rootPath = "jkw/games/Worldcup/BettingData";
        static string _LogPath = "jkw/games/Worldcup/logs.txt";
        static string _DashboardPath = "jkw/games/Worldcup/dashboard.json";
        static List<BettingData> _bettingDataList = new List<BettingData>();

        public static List<GroupData> GroupDataList = new List<GroupData>();
        public static KnockoutPhase KnockoutData = new KnockoutPhase();
        public static List<DashboardData> DashboardList = new List<DashboardData>();

        static WorldcupBettingManager()
        {
        }

        public static void Load()
        {
            _bettingDataList = Directory.GetFiles(_rootPath, "*.json")
                .Select(x => File.ReadAllText(x, Encoding.UTF8))
                .Select(x => JsonConvert.DeserializeObject<BettingData>(x))
                .ToList();

            DashboardList = JsonConvert.DeserializeObject<List<DashboardData>>(File.ReadAllText(_DashboardPath));

            //Update16TargetData(); // 끝났음.
            UpdateKnockoutData();
        }

        public static void Save(this BettingData bettingData)
        {
#if !DEBUG
            var path = Path.Combine(_rootPath, $"{bettingData.BettingName.Replace(" ", "")}.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(bettingData, Formatting.Indented), Encoding.UTF8);
#endif
        }

        public static void Log(string username, string title, string text)
        {
#if !DEBUG
            try
            {
                var time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logText = $"[{time}][{username}][{title}] {text}";
                File.AppendAllLines(_LogPath, new[] { logText }, Encoding.UTF8);
            }
            catch { }
#endif
        }

        public static BettingData GetBettingData(string bettingName)
        {
            return _bettingDataList.FirstOrDefault(x => x.BettingName == bettingName);
        }

        public static BettingData UpdateData(BettingData bettingData)
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
            bettingData
                .UpdateAllotmentAmount()
                .UpdateDashboard()
                .Save();
            return bettingData;
        }

        public static bool UpdateUserBettingData(this BettingData bettingData, string username, List<UserBetting> userBettings, bool checkId)
        {
            if (bettingData.UserBettingList.ContainsKey(username))
            {
                bettingData.UserBettingList[username].BettingList = userBettings;
            }
            else
            {
                bettingData.UserBettingList[username] = new UserBettingData(username, userBettings);
            }

            bettingData.RecalcMatchData(checkId);
            return true;
        }

        public static BettingData UpdateAllotmentAmount(this BettingData bettingData)
        {
            var weightDic = bettingData.TargetList.ToDictionary(x => x.Id, x => x.Weight);

            var offsetScoreDic = bettingData.UserBettingList.Select(x => x.Value)
                .GroupBy(x => new { BettingGroup = x.BettingGroup ?? "" })
                .Select(x => new { x.Key.BettingGroup, MinScore = x.Min(e => e.BettingList.Where(r => r.IsMatched).Sum(r => weightDic[r.Id])) })
                .Select(x => new { x.BettingGroup, Offset = x.MinScore > bettingData.ScoreMinimum ? x.MinScore - bettingData.ScoreMinimum : 0 })
                .ToDictionary(x => x.BettingGroup, x => bettingData.ScoreMinimum < 0 ? 0 : x.Offset);

            var ratioBaseDic = bettingData.UserBettingList.Select(x => x.Value)
                .GroupBy(x => new { BettingGroup = x.BettingGroup ?? "" })
                .Select(x => new { x.Key.BettingGroup, RatioBase = x.Sum(e => e.BettingList.Where(r => r.IsMatched).Sum(r => weightDic[r.Id]) - offsetScoreDic[x.Key.BettingGroup ?? ""]) })
                .ToDictionary(x => x.BettingGroup, x => x.RatioBase);

            var userBettingList = bettingData.UserBettingList
                .Select(x => new { Data = x.Value, Weight = (x.Value.BettingList.Where(e => e.IsMatched).Sum(e => weightDic[e.Id]) - offsetScoreDic[x.Value.BettingGroup ?? ""]) / ratioBaseDic[x.Value.BettingGroup ?? ""] })
                .Select(x => new { x.Data, Allotment = x.Weight * bettingData.UserBettingList.Where(e => (e.Value.BettingGroup ?? "") == (x.Data.BettingGroup ?? "")).Sum(e => e.Value.BettingAmount) })
                .Select(x =>
                {
                    x.Data.AllotmentAmount = (int)(x.Allotment / 10) * 10;
                    return x.Data;
                })
                .ToDictionary(x => x.Username, x => x);

            bettingData.UserBettingList = userBettingList;
            return bettingData;
        }

        public static BettingData UpdateDashboard(this BettingData bettingData)
        {
            var weightDic = bettingData.TargetList.ToDictionary(x => x.Id, x => x.Weight);
            var lastDashboard = DashboardList
                .Where(x => x.BettingName == bettingData.BettingName)
                .OrderBy(x => x.CalcTime)
                .LastOrDefault();

            var offsetScoreDic = bettingData.UserBettingList.Select(x => x.Value)
                .GroupBy(x => new { BettingGroup = x.BettingGroup ?? "" })
                .Select(x => new { x.Key.BettingGroup, MinScore = x.Min(e => e.BettingList.Where(r => r.IsMatched).Sum(r => weightDic[r.Id])) })
                .Select(x => new { x.BettingGroup, Offset = x.MinScore > bettingData.ScoreMinimum ? x.MinScore - bettingData.ScoreMinimum : 0 })
                .ToDictionary(x => x.BettingGroup, x => bettingData.ScoreMinimum < 0 ? 0 : (int)x.Offset);

            var currDashboard = bettingData.UserBettingList.Select(x => x.Value)
                .Select(x => new DashboardItem
                {
                    Username = x.Username,
                    BettingGroup = x.BettingGroup,
                    MatchedCount = (int)x.BettingList.Where(e => e.IsMatched).Sum(e => weightDic[e.Id]),
                    OffsetCount = (int)x.BettingList.Where(e => e.IsMatched).Sum(e => weightDic[e.Id]) - offsetScoreDic[x.BettingGroup],
                    BettingAmount = x.BettingAmount,
                    AllotmentAmount = x.AllotmentAmount,
                })
                .OrderByDescending(x => x.MatchedCount)
                .ToList();

            var lastJsonText = JsonConvert.SerializeObject(lastDashboard?.List);
            var currJsonText = JsonConvert.SerializeObject(currDashboard);

            if (lastJsonText != currJsonText)
            {
                DashboardList.Add(new DashboardData
                {
                    CalcTime = DateTime.Now,
                    BettingName = bettingData.BettingName,
                    List = currDashboard,
                });
                Log("SYSTEM", "Dashboard", currJsonText);
#if !DEBUG
                File.WriteAllText(_DashboardPath, JsonConvert.SerializeObject(DashboardList, Formatting.Indented), Encoding.UTF8);
#endif
            }

            return bettingData;
        }

        public static void Update16TargetData()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        GroupDataList = await GetGroupResultAsync();
                        Log("SYSTEM", "Update16TargetData", JsonConvert.SerializeObject(GroupDataList));
                        var bettingName = "16강 맞추기";
                        var bettingData = _bettingDataList.First(x => x.BettingName == bettingName);

                        foreach (var teamData in GroupDataList.SelectMany(x => x.TeamDataList.Where(e => e.Rank <= 2).Select(e => new { x.GroupName, TeamData = e })))
                        {
                            var id = $"{teamData.GroupName.Replace("Group ", "")}{teamData.TeamData.Rank}";
                            bettingData.TargetList.First(x => x.Id == id).Value = teamData.TeamData.TeamCode;
                        }

                        bettingData.RecalcMatchData(false);
                    }
                    catch (Exception ex)
                    {
                        Log("SYSTEM", "Error:Update16TargetData", ex.Message);
                    }
                    finally
                    {
                        await Task.Delay(600 * 1000);
                    }
                }
            });
        }

        public static void UpdateKnockoutData()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        KnockoutData = await GetKnockoutPhaseAsync();
                        Log("SYSTEM", "UpdateKnockoutData", JsonConvert.SerializeObject(KnockoutData));

                        var bettingName = "round16";
                        var bettingData = _bettingDataList.FirstOrDefault(x => x.BettingName == bettingName);
                        if (bettingData != null)
                        {
                            // TODO: TargetList 만들고 데이터 엮어야함.
                            bettingData.TargetList = bettingData.TargetList
                                .Select(x => new Target
                                {
                                    Id = x.Id,
                                    Weight = x.Weight,
                                    Value = KnockoutData.Round16.FirstOrDefault(e => e.MatchId == x.Id)?.Winner?.TeamCode ?? "",
                                })
                                .ToList();

                            bettingData.RecalcMatchData(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log("SYSTEM", "Error:UpdateKnockoutData", ex.Message);
                    }
                    finally
                    {
                        await Task.Delay(600 * 1000);
                    }
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
                    var groupName = x.SelectSingleNode(".//p[@class='fi-table__caption__title fi-ltr--force']").InnerText;
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
                                //TeamPicture = $"https://img.fifa.com/images/flags/4/{code.ToLower()}.png",
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

        static async Task<KnockoutPhase> GetKnockoutPhaseAsync()
        {
            var url = "https://www.fifa.com/worldcup/matches";
            var request = WebRequest.CreateHttp(url);

            var response = await request.GetResponseAsync();
            var html = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var htmlDoc = new HtmlDocument();

            htmlDoc.LoadHtml(html);

            var knockoutSection = htmlDoc.DocumentNode.SelectNodes(".//div[contains(@class, 'fi-matchlist')]")[1];

            var knockoutPhase = new KnockoutPhase(knockoutSection);
            return knockoutPhase;
        }
    }

    public class BettingData
    {
        public string BettingName { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime FreezeTime { get; set; }
        public List<Target> TargetList { get; set; }
        public Dictionary<string /* username */, UserBettingData> UserBettingList { get; set; }
        public int ScoreMinimum { get; set; } // ignore: -1

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

    public class Target
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public double Weight { get; set; }
    }

    public class UserBettingData
    {
        public string Username { get; set; }
        public int BettingAmount { get; set; }
        public int AllotmentAmount { get; set; }
        public string BettingGroup { get; set; }
        public List<UserBetting> BettingList { get; set; }

        public UserBettingData() { }

        public UserBettingData(string username, List<UserBetting> bettingList)
        {
            Username = username;
            BettingList = bettingList;
        }
    }

    public class UserBetting
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
        //public string TeamPicture { get; set; }
        public int Rank { get; set; }
        public int Point { get; set; }
    }

    public class DashboardData
    {
        public DateTime CalcTime { get; set; }
        public string BettingName { get; set; }
        public List<DashboardItem> List { get; set; }
    }

    public class DashboardItem
    {
        public string Username { get; set; }
        public string BettingGroup { get; set; }
        public int MatchedCount { get; set; }
        public int OffsetCount { get; set; }
        public int BettingAmount { get; set; }
        public int AllotmentAmount { get; set; }
    }

    public class KnockoutPhase
    {
        public List<KnockoutMatch> Round16 { get; set; }
        public List<KnockoutMatch> Round8 { get; set; }
        public List<KnockoutMatch> Round4 { get; set; }
        public List<KnockoutMatch> Third { get; set; }
        public List<KnockoutMatch> Final { get; set; }

        public KnockoutPhase() { }
        public KnockoutPhase(HtmlNode knockoutSection)
        {
            var list = knockoutSection.SelectNodes("./div[contains(@class, 'fi-mu-list')]").ToList();

            Round16 = list[0].SelectNodes(".//div[contains(@class, 'fi-mu ')]").Select((x, i) => new KnockoutMatch($"R16W{i + 1}", x)).ToList();
            Round8 = list[1].SelectNodes(".//div[contains(@class, 'fi-mu ')]").Select((x, i) => new KnockoutMatch($"R8W{i + 1}", x)).ToList();
            Round4 = list[2].SelectNodes(".//div[contains(@class, 'fi-mu ')]").Select((x, i) => new KnockoutMatch($"R4W{i + 1}", x)).ToList();
            Third = list[3].SelectNodes(".//div[contains(@class, 'fi-mu ')]").Select((x, i) => new KnockoutMatch($"THIRD", x)).ToList();
            Final = list[4].SelectNodes(".//div[contains(@class, 'fi-mu ')]").Select((x, i) => new KnockoutMatch($"FINAL", x)).ToList();
        }
    }

    public class KnockoutMatch
    {
        public string MatchId { get; set; }
        public KnockoutTeam TeamHome { get; set; }
        public KnockoutTeam TeamAway { get; set; }
        public DateTime GameStartTime { get; set; }
        public bool IsStarted { get; set; }

        public KnockoutTeam Winner
            => TeamHome.Score > TeamAway.Score ? TeamHome :
            TeamHome.Score < TeamAway.Score ? TeamAway :
            TeamHome.SubScore > TeamAway.SubScore ? TeamHome :
            TeamHome.SubScore < TeamAway.SubScore ? TeamAway : null;
        public bool IsFreeze => IsStarted || GameStartTime < DateTime.Now || Winner != null;

        public KnockoutMatch() { }
        public KnockoutMatch(string matchId, HtmlNode matchSection)
        {
            MatchId = matchId;

            var list = matchSection.SelectNodes("./div/div[contains(@class, 'fi-t')]").ToList();
            var scoreText = matchSection.SelectSingleNode(".//span[contains(@class, 'fi-s__scoreText')]").InnerText.Trim();
            var homeScore = scoreText.Contains("-") ? scoreText.Split('-')[0] : "";
            var awayScore = scoreText.Contains("-") ? scoreText.Split('-')[1] : "";
            IsStarted = homeScore != "";
            TeamHome = new KnockoutTeam(list[0], homeScore);
            TeamAway = new KnockoutTeam(list[1], awayScore);
            var matchTimeUtc = matchSection.SelectSingleNode(".//div[contains(@class, 'fi-mu__info__datetime')]").GetAttributeValue("data-utcdate", "");
            GameStartTime = DateTime.Parse(matchTimeUtc).AddHours(12);
        }
    }

    public class KnockoutTeam
    {
        public string TeamName { get; set; }
        public string TeamCode { get; set; }
        public int Score { get; set; }
        public int SubScore { get; set; }

        public KnockoutTeam() { }
        public KnockoutTeam(HtmlNode teamSection, string scoreText)
        {
            TeamName = teamSection.SelectSingleNode(".//span[contains(@class, 'fi-t__nText')]").InnerText.Trim();
            TeamCode = teamSection.SelectSingleNode(".//span[contains(@class, 'fi-t__nTri')]").InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(scoreText))
            {
                if (scoreText.Contains("("))
                {
                    // 승부차기 ?! 일단 추측 코딩 해봄
                    var arr = scoreText.Split('(');
                    Score = int.Parse(arr[0].Trim());
                    SubScore = int.Parse(arr[1].Replace(")", "").Trim());
                }
                else
                {
                    Score = int.Parse(scoreText);
                }
            }
        }
    }
}
