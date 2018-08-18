using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Extensions;

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
        public static List<UserResult> UserResultList = new List<UserResult>();

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
            //UpdateKnockoutData();
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
                    var target = bettingData.TargetList.FirstOrDefault(x => x.Id == userData.Id);
                    userData.IsMatched = userData.Value == target?.Value;
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
                .Select(x => new { x.Key.BettingGroup, MinScore = x.Min(e => e.BettingList.Where(r => r.IsMatched).Where(r => weightDic.ContainsKey(r.Id)).Sum(r => weightDic[r.Id])) })
                .Select(x => new { x.BettingGroup, Offset = x.MinScore > bettingData.ScoreMinimum ? x.MinScore - bettingData.ScoreMinimum : 0 })
                .ToDictionary(x => x.BettingGroup, x => bettingData.ScoreMinimum < 0 ? 0 : x.Offset);

            var ratioBaseDic = bettingData.UserBettingList.Select(x => x.Value)
                .GroupBy(x => new { BettingGroup = x.BettingGroup ?? "" })
                .Select(x => new { x.Key.BettingGroup, RatioBase = x.Sum(e => e.BettingList.Where(r => r.IsMatched).Where(r => weightDic.ContainsKey(r.Id)).Sum(r => weightDic[r.Id]) - offsetScoreDic[x.Key.BettingGroup ?? ""]) })
                .ToDictionary(x => x.BettingGroup, x => x.RatioBase);

            var userBettingList = bettingData.UserBettingList
                .Select(x => new { Data = x.Value, Weight = (x.Value.BettingList.Where(e => e.IsMatched).Where(r => weightDic.ContainsKey(r.Id)).Sum(e => weightDic[e.Id]) - offsetScoreDic[x.Value.BettingGroup ?? ""]) / ratioBaseDic[x.Value.BettingGroup ?? ""] })
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
                .Select(x => new { x.Key.BettingGroup, MinScore = x.Min(e => e.BettingList.Where(r => r.IsMatched).Where(r => weightDic.ContainsKey(r.Id)).Sum(r => weightDic[r.Id])) })
                .Select(x => new { x.BettingGroup, Offset = x.MinScore > bettingData.ScoreMinimum ? x.MinScore - bettingData.ScoreMinimum : 0 })
                .ToDictionary(x => x.BettingGroup, x => bettingData.ScoreMinimum < 0 ? 0 : (int)x.Offset);

            var currDashboard = bettingData.UserBettingList.Select(x => x.Value)
                .Select(x => new DashboardItem
                {
                    Username = x.Username,
                    BettingGroup = x.BettingGroup ?? "",
                    MatchedCount = (int)x.BettingList.Where(e => e.IsMatched).Where(r => weightDic.ContainsKey(r.Id)).Sum(e => weightDic[e.Id]),
                    OffsetCount = (int)x.BettingList.Where(e => e.IsMatched).Where(r => weightDic.ContainsKey(r.Id)).Sum(e => weightDic[e.Id]) - offsetScoreDic[x.BettingGroup ?? ""],
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

        #region Crawl from FIFA
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

                        var knockoutTemp = await GetKnockoutPhaseAsync();
                        UserResultList = CalcUserResultList(knockoutTemp);

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

                        var finalBettingName = "final";
                        var finalBettingData = _bettingDataList.FirstOrDefault(x => x.BettingName == finalBettingName);
                        if (finalBettingData != null)
                        {
                            finalBettingData.TargetList = new List<Target>();
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "Champion",
                                Value = KnockoutData.Final[0].Winner?.TeamCode ?? "",
                                Weight = 32,
                            });
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "Second",
                                Value = KnockoutData.Final[0].Loser?.TeamCode ?? "",
                                Weight = 8,
                            });
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "Third",
                                Value = KnockoutData.Third[0].Winner?.TeamCode ?? "",
                                Weight = 4,
                            });
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "Fourth",
                                Value = KnockoutData.Third[0].Loser?.TeamCode ?? "",
                                Weight = 2,
                            });
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "R8W1",
                                Value = KnockoutData.Round8[0].Winner?.TeamCode ?? "",
                                Weight = 1,
                            });
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "R8W2",
                                Value = KnockoutData.Round8[1].Winner?.TeamCode ?? "",
                                Weight = 1,
                            });
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "R8W4",
                                Value = KnockoutData.Round8[3].Winner?.TeamCode ?? "",
                                Weight = 1,
                            });
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "R8W3",
                                Value = KnockoutData.Round8[2].Winner?.TeamCode ?? "",
                                Weight = 1,
                            });
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "R4W1",
                                Value = KnockoutData.Round4[0].Winner?.TeamCode ?? "",
                                Weight = 5,
                            });
                            finalBettingData.TargetList.Add(new Target
                            {
                                Id = "R4W2",
                                Value = KnockoutData.Round4[1].Winner?.TeamCode ?? "",
                                Weight = 5,
                            });
                        }

                        finalBettingData.RecalcMatchData(true);
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
        #endregion

        #region 경우의수
        public static List<UserResult> CalcUserResultList(KnockoutPhase knockoutData)
        {
            var userNameDic = UserDatabase.GetAllUser()
                .Where(x => x.Email != null)
                .ToDictionary(x => x.Email, x => x.Name);

            var userData = GetUserData(userNameDic);

            var resultList = MakePermutation(knockoutData)
                .SelectMany(target => MakeUserResult(target, userData))
                .OrderByDescending(x => x.Allotment)
                .ToList();

            return resultList;
        }

        public static List<UserData> GetUserData(Dictionary<string, string> userNameDic)
        {
            var bettingName = "final";
            var bettingData = WorldcupBettingManager.GetBettingData(bettingName);

            if (bettingData == null)
                return new List<UserData>();

            var idList = new[] { "Champion", "Second", "Third", "Fourth", }
                .Select((x, i) => new { Code = x, Index = i + 1 })
                .ToDictionary(x => x.Code, x => x.Index);

            var result = bettingData.UserBettingList.Select(x => x.Value)
                .Select(x => new UserData
                {
                    Email = x.Username,
                    Name = userNameDic.ContainsKey(x.Username) && !string.IsNullOrEmpty(userNameDic[x.Username])? userNameDic[x.Username] : x.Username,
                    List = x.BettingList.Where(e => idList.ContainsKey(e.Id))
                        .Select(e => new { Rank = idList[e.Id], Team = e.Value })
                        .OrderBy(e => e.Rank)
                        .Select(e => e.Team)
                        .ToList(),
                })
                .ToList();

            return result;
        }

        public static List<UserResult> MakeUserResult(List<string> target, List<UserData> userData)
        {
            var result = userData
                .Select(x => new UserResult
                {
                    Target = target,
                    Email = x.Email,
                    Name = x.Name,
                    List = x.List,
                    Score = CalcScore(x, target),
                    Allotment = 0,
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Email)
                .ToList();

            var offset = Math.Max(0, result.Min(x => x.Score) - 1);
            foreach (var data in result)
                data.OffsetScore = data.Score - offset;
            foreach (var data in result)
            {
                data.Allotment = result.Count() * 10000 * data.OffsetScore / result.Sum(x => x.OffsetScore);
                data.Allotment /= 10;
                data.Allotment *= 10;
            }
            return result;
        }

        public static int CalcScore(UserData userData, List<string> target)
        {
            var finalT = target.Take(2).ToList();
            var score = 0;
            if (userData.List[0] == target[0]) score += 32;
            if (userData.List[1] == target[1]) score += 8;
            if (userData.List[2] == target[2]) score += 4;
            if (userData.List[3] == target[3]) score += 2;
            if (finalT.Contains(userData.List[0])) score += 5;
            if (finalT.Contains(userData.List[1])) score += 5;
            if (target.Contains(userData.List[0])) score += 1;
            if (target.Contains(userData.List[1])) score += 1;
            if (target.Contains(userData.List[2])) score += 1;
            if (target.Contains(userData.List[3])) score += 1;
            return score;
        }
        #endregion

        #region MakePermutation
        public static List<List<string>> MakePermutation(KnockoutPhase data)
        {
            var matches = new[]
            {
                data.Round8,
                data.Round4,
                data.Third,
                data.Final,
            }
            .SelectMany(x => x)
            .OrderBy(x => x.MatchNumber)
            .ToList();

            var teams = matches.Where(x => x.MatchNumber >= 61)
                .SelectMany(x => new[] { x.TeamHome, x.TeamAway })
                .Where(x => x.TeamNumber > 0)
                .ToDictionary(x => x.TeamCode);

            var startMatchNumber = teams.First().Value.TeamNumber;
            var startIndex = matches.Select((x, i) => new { Index = i, x.MatchNumber })
                .First(x => x.MatchNumber == startMatchNumber)
                .Index;

            var result = new List<List<string>>();
            Rec(startIndex, matches, teams, result);

            result = result.Select(x => new { Text = x.StringJoin(""), List = x })
                .GroupBy(x => x.Text)
                .Select(x => x.First().List)
                .ToList();

            return result;
        }

        public static void Rec(int index, List<KnockoutMatch> matches, Dictionary<string, KnockoutTeam> teams, List<List<string>> permutation)
        {
            if (index == matches.Count())
            {
                var final = matches[index - 1];
                var third = matches[index - 2];
                permutation.Add(new[] { final.Winner, final.Loser, third.Winner, third.Loser }.Select(x => x.TeamCode).ToList());
                return;
            }

            var match = matches[index];

            match.TeamHome.Score = 1;
            match.TeamAway.Score = 0;
            if (match.MatchNumber <= 62)
            {
                if (teams.ContainsKey($"W{match.MatchNumber}"))
                {
                    var winnerTeam = teams[$"W{match.MatchNumber}"];
                    var nextCode = winnerTeam.TeamCode;
                    winnerTeam.TeamCode = match.Winner.TeamCode;
                }
            }
            if (match.MatchNumber == 61 || match.MatchNumber == 62)
            {
                if (teams.ContainsKey($"L{match.MatchNumber}"))
                {
                    var loserTeam = teams[$"L{match.MatchNumber}"];
                    loserTeam.TeamCode = match.Loser.TeamCode;
                }
            }
            Rec(index + 1, matches, teams, permutation);
            match.TeamHome.Score = 0;
            match.TeamAway.Score = 1;
            if (match.MatchNumber <= 62)
            {
                if (teams.ContainsKey($"W{match.MatchNumber}"))
                {
                    var winnerTeam = teams[$"W{match.MatchNumber}"];
                    var nextCode = winnerTeam.TeamCode;
                    winnerTeam.TeamCode = match.Winner.TeamCode;
                }
            }
            if (match.MatchNumber == 61 || match.MatchNumber == 62)
            {
                if (teams.ContainsKey($"L{match.MatchNumber}"))
                {
                    var loserTeam = teams[$"L{match.MatchNumber}"];
                    loserTeam.TeamCode = match.Loser.TeamCode;
                }
            }
            Rec(index + 1, matches, teams, permutation);
        }
        #endregion
    }

    #region BettingData
    public class BettingData
    {
        public string BettingName { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime FreezeTime { get; set; }
        public List<Target> TargetList { get; set; }
        public Dictionary<string /* username */, UserBettingData> UserBettingList { get; set; }
        public int ScoreMinimum { get; set; } // ignore: -1
        public List<string> RandomSelectedUser { get; set; } = new List<string>();

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
    #endregion

    #region GroupData
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
    #endregion

    #region DashboardData
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
    #endregion

    #region KnockoutPhase
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
        public int MatchNumber { get; set; }
        public KnockoutTeam TeamHome { get; set; }
        public KnockoutTeam TeamAway { get; set; }
        public DateTime GameStartTime { get; set; }
        public bool IsStarted { get; set; }

        public KnockoutTeam Winner
            => TeamHome.Score > TeamAway.Score ? TeamHome :
            TeamHome.Score < TeamAway.Score ? TeamAway :
            TeamHome.SubScore > TeamAway.SubScore ? TeamHome :
            TeamHome.SubScore < TeamAway.SubScore ? TeamAway : null;
        [JsonIgnore]
        public KnockoutTeam Loser
            => Winner == null ? null : Winner == TeamHome ? TeamAway : TeamHome;

        public bool IsFreeze => IsStarted || GameStartTime < DateTime.Now || Winner != null;

        public KnockoutMatch() { }
        public KnockoutMatch(string matchId, HtmlNode matchSection)
        {
            MatchId = matchId;
            MatchNumber = matchSection.SelectNodes(".//div[contains(@class, 'fi__info__matchnumber')]/span")[1].InnerText.ToInt();

            var list = matchSection.SelectNodes("./div/div[contains(@class, 'fi-t')]").ToList();
            var scoreText = matchSection.SelectSingleNode(".//span[contains(@class, 'fi-s__scoreText')]").InnerText.Trim();
            var homeScore = scoreText.Contains("-") ? scoreText.Split('-')[0] : "";
            var awayScore = scoreText.Contains("-") ? scoreText.Split('-')[1] : "";
            IsStarted = homeScore != "";
            var subScore = matchSection.SelectSingleNode(".//div[contains(@class, 'fi-mu__penaltyscore-wrap')]").InnerText.Trim().Replace("(", "").Replace(")", "");
            var homeSubScore = string.IsNullOrWhiteSpace(subScore) ? "" : subScore.Split('-')[0];
            var awaySubScore = string.IsNullOrWhiteSpace(subScore) ? "" : subScore.Split('-')[1];
            TeamHome = new KnockoutTeam(list[0], homeScore, homeSubScore);
            TeamAway = new KnockoutTeam(list[1], awayScore, awaySubScore);
            var matchTimeUtc = matchSection.SelectSingleNode(".//div[contains(@class, 'fi-mu__info__datetime')]").GetAttributeValue("data-utcdate", "");
            GameStartTime = DateTime.Parse(matchTimeUtc).AddHours(12);
        }
    }

    public class KnockoutTeam
    {
        public int TeamNumber { get; set; }
        public string TeamName { get; set; }
        public string TeamCode { get; set; }
        public int Score { get; set; }
        public int SubScore { get; set; }

        public KnockoutTeam() { }
        public KnockoutTeam(HtmlNode teamSection, string scoreText, string subScoreText)
        {
            TeamName = teamSection.SelectSingleNode(".//span[contains(@class, 'fi-t__nText')]").InnerText.Trim();
            TeamCode = teamSection.SelectSingleNode(".//span[contains(@class, 'fi-t__nTri')]").InnerText.Trim();
            if (int.TryParse(TeamCode.Substring(1, 2), out var teamNumber))
            {
                TeamNumber = teamNumber;
            }
            if (!string.IsNullOrWhiteSpace(scoreText))
            {
                Score = scoreText.ToInt();
            }
            if (!string.IsNullOrWhiteSpace(subScoreText))
            {
                SubScore = subScoreText.ToInt();
            }
        }
    }
    #endregion

    #region UserResult
    public class UserResult
    {
        public List<string> Target { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public List<string> List { get; set; } // Team codes
        public int Score { get; set; }
        public int OffsetScore { get; set; }
        public int Allotment { get; set; }
    }
    public class UserData
    {
        public string Email { get; set; }
        public string Name { get; set; } = "";
        public List<string> List { get; set; } // Team codes
    }
    #endregion
}
