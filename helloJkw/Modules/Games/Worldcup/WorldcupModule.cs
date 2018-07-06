using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Newtonsoft.Json;
using Extensions;

namespace helloJkw.Game.Worldcup
{
    public class WorldcupModule : JkwModule
    {
        static string _simpleLoginPath = "jkw/games/Worldcup/SimpleLoginData.json";
        static Dictionary<string, SimpleLoginData> _simpleLoginDic = new Dictionary<string, SimpleLoginData>();

        class SimpleLoginData
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        static List<SimpleLoginData> LoadLoginData()
            => JsonConvert.DeserializeObject<List<SimpleLoginData>>(File.ReadAllText(_simpleLoginPath, Encoding.UTF8));

        public WorldcupModule()
        {
            Get["/worldcup"] = _ => Response.AsRedirect("/worldcup/2018");

            Get["/worldcup/2018"] = _ =>
            {
                if (DateTime.Now < new DateTime(2018, 7, 4, 6, 0, 0))
                    return Response.AsRedirect("/worldcup/2018/round16");
                return Response.AsRedirect("/worldcup/2018/final");
            };

            Get["/worldcup/2018/final"] = _ =>
            {
                var username = "";
                Model.SimpleLogin = false;
                if (session.IsLogin)
                {
                    username = session.User.Email;
                }
                if (_simpleLoginDic.ContainsKey(sessionId))
                {
                    var loginData = _simpleLoginDic[sessionId];
                    Model.SimpleLogin = true;
                    username = loginData.Username;
                }
                var bettingName = "final";
                var bettingData = WorldcupBettingManager.GetBettingData(bettingName);
                var random = new Random((int)DateTime.Now.Ticks);
                var sampleList = bettingData.UserBettingList.Select(x => new { Rnd = random.Next(1, 10000), Value = x })
                    .OrderBy(x => x.Rnd)
                    .Take(3)
                    .Select(x => x.Value.Value.BettingList)
                    .Select(x => x.Select(e => $"https://img.fifa.com/images/flags/4/{e.Value.ToLower()}.png").ToList())
                    .ToList();
                //WorldcupBettingManager.KnockoutData.Round4[0].TeamHome = new KnockoutTeam { TeamCode = "FRA", TeamName = "Uruguay" };
                //WorldcupBettingManager.KnockoutData.Round4[0].TeamAway = new KnockoutTeam { TeamCode = "BEL", TeamName = "Brazil" };
                //WorldcupBettingManager.KnockoutData.Round4[1].TeamHome = new KnockoutTeam { TeamCode = "RUS", TeamName = "Russia" };
                //WorldcupBettingManager.KnockoutData.Round4[1].TeamAway = new KnockoutTeam { TeamCode = "SWE", TeamName = "Sweden" };
                var knockoutData = WorldcupBettingManager.KnockoutData;
                var dashboard = WorldcupBettingManager.DashboardList
                    .Where(x => x.BettingName == bettingName)
                    .OrderBy(x => x.CalcTime)
                    .LastOrDefault()?.List
                    ?.Select(x => new DashboardItem
                    {
                        Username = x.Username,
                        BettingGroup = x.BettingGroup,
                        MatchedCount = x.MatchedCount,
                        OffsetCount = x.OffsetCount,
                        BettingAmount = x.BettingAmount,
                        AllotmentAmount = x.AllotmentAmount,
                    })
                    ?.OrderByDescending(x => x.MatchedCount)
                    ?.ThenByDescending(x =>
                    {
                        var score = 0;
                        var bettingList = bettingData.UserBettingList[x.Username].BettingList;

                        var finalId = new[] { "Champion", "Second" };
                        var userFinalTeams = bettingList.Where(e => finalId.Contains(e.Id)).Select(e => e.Value).ToList();
                        var finalTeams = new[] { knockoutData.Final[0].TeamHome.TeamName, knockoutData.Final[0].TeamAway.TeamName };
                        score += 10 * userFinalTeams.Join(finalTeams, a => 1, b => 2, (a, b) => new { a, b })
                            .Count(e => e.a == e.b);

                        var semiId = finalId.Concat(new[] { "Third", "Fourth" }).ToList();
                        var userSemiTeams = bettingList.Where(e => semiId.Contains(e.Id)).Select(e => e.Value).ToList();
                        var semiTeams = Enumerable.Range(0, 2).SelectMany(e => new[] { knockoutData.Round4[e].TeamHome.TeamName, knockoutData.Round4[e].TeamAway.TeamName }).ToList();
                        score += 1 * userFinalTeams.Join(finalTeams, a => 1, b => 2, (a, b) => new { a, b })
                            .Count(e => e.a == e.b);

                        return score;
                    })
                    ?.ThenBy(x => x.Username)
                    ?.ToList() ?? new List<DashboardItem>();

                Model.Username = username;
                Model.SampleList = sampleList;
                Model.KnockoutData = WorldcupBettingManager.KnockoutData;
                Model.Dashboard = dashboard;
                Model.BettingData = bettingData;
                Model.FreezeTime = bettingData.FreezeTime;
                Model.IsRandomSelected = bettingData.RandomSelectedUser.Contains(username);
                return View["Games/Worldcup/worldcupFinal.cshtml", Model];
            };

            Get["/worldcup/2018/round16"] = _ =>
            {
                Model.SimpleLogin = false;
                Model.Username = "";
                if (session.IsLogin)
                {
                }
                if (_simpleLoginDic.ContainsKey(sessionId))
                {
                    var loginData = _simpleLoginDic[sessionId];
                    Model.SimpleLogin = true;
                    Model.Username = loginData.Username;
                }
                var bettingName = "round16";
                var bettingData = WorldcupBettingManager.GetBettingData(bettingName);
                var random = new Random((int)DateTime.Now.Ticks);
                var sampleList = bettingData.UserBettingList.Select(x => new { Rnd = random.Next(1, 10000), Value = x })
                    .OrderBy(x => x.Rnd)
                    .Take(3)
                    .Select(x => x.Value.Value.BettingList)
                    .Select(x => x.Select(e => $"https://img.fifa.com/images/flags/4/{e.Value.ToLower()}.png").ToList())
                    .ToList();
                var dashboard = WorldcupBettingManager.DashboardList
                    .Where(x => x.BettingName == bettingName)
                    .OrderBy(x => x.CalcTime)
                    .LastOrDefault()?.List
                    ?.Select(x => new DashboardItem
                    {
                        Username = x.Username,
                        BettingGroup = x.BettingGroup,
                        MatchedCount = x.MatchedCount,
                        OffsetCount = x.OffsetCount,
                        BettingAmount = x.BettingAmount,
                        AllotmentAmount = x.AllotmentAmount,
                    })
                    ?.OrderByDescending(x => x.MatchedCount)
                    ?.ThenBy(x => x.Username)
                    ?.ToList() ?? new List<DashboardItem>();

                Model.SampleList = sampleList;
                Model.KnockoutData = WorldcupBettingManager.KnockoutData;
                Model.Dashboard = dashboard;
                Model.BettingData = bettingData;
                var notyetList = WorldcupBettingManager.KnockoutData.Round16
                    .Where(x => !x.IsFreeze);
                Model.FreezeTime = notyetList.Any() ? notyetList.Min(x => x.GameStartTime) :
                    WorldcupBettingManager.KnockoutData.Round16.OrderBy(x => x.GameStartTime).Last().GameStartTime;
                return View["Games/Worldcup/worldcupRound16.cshtml", Model];
            };

            Get["/worldcup/2018/group"] = _ =>
            {
                Model.SimpleLogin = false;
                Model.Username = "";
                if (session.IsLogin)
                {
                }
                if (_simpleLoginDic.ContainsKey(sessionId))
                {
                    var loginData = _simpleLoginDic[sessionId];
                    Model.SimpleLogin = true;
                    Model.Username = loginData.Username;
                }
                var bettingName = "16강 맞추기";
                var bettingData = WorldcupBettingManager.GetBettingData(bettingName);
                bettingData = bettingData.RecalcMatchData(false);
                var random = new Random((int)DateTime.Now.Ticks);
                var sampleList = bettingData.UserBettingList.Select(x => new { Rnd = random.Next(1, 10000), Value = x })
                    .OrderBy(x => x.Rnd)
                    .Take(3)
                    .Select(x => x.Value.Value.BettingList)
                    .Select(x => x.Select(e => $"https://img.fifa.com/images/flags/4/{e.Value.ToLower()}.png").ToList())
                    .ToList();
                var dashboard = WorldcupBettingManager.DashboardList
                    .Where(x => x.BettingName == bettingName)
                    .OrderBy(x => x.CalcTime)
                    .LastOrDefault()?.List
                    ?.Select(x => new DashboardItem
                    {
                        Username = x.Username,
                        BettingGroup = x.BettingGroup,
                        MatchedCount = x.MatchedCount,
                        OffsetCount = x.OffsetCount,
                        BettingAmount = x.BettingAmount,
                        AllotmentAmount = x.AllotmentAmount,
                    })
                    ?.ToList() ?? new List<DashboardItem>();

                Model.SampleList = sampleList;
                Model.GroupList = WorldcupBettingManager.GroupDataList;
                Model.Dashboard = dashboard;
                Model.FreezeTime = bettingData.FreezeTime;
                return View["Games/Worldcup/worldcupGroup.cshtml", Model];
            };

            Get["/worldcup/dataview/{bettingName}"] = _ =>
            {
                if (IsDebug || session.IsLogin && session.User.Email == "jkwchunjae@gmail.com")
                {
                    string bettingName = _.bettingName;
                    var bettingData = WorldcupBettingManager.GetBettingData(bettingName);
                    Model.BettingData = bettingData;
                    return View["Games/Worldcup/worldcupDataView.cshtml", Model];
                }
                return Response.AsRedirect("/worldcup");
            };

            Get["/worldcup/manageuser"] = _ =>
            {
                if (IsDebug || session.IsLogin && session.User.Email == "jkwchunjae@gmail.com")
                {
                    var loginData = LoadLoginData();
                    var jsonText = JsonConvert.SerializeObject(loginData, Formatting.Indented);
                    return $"<pre>{jsonText}</pre>";
                }
                return Response.AsRedirect("/worldcup");
            };

            Get["/worldcup/manageuser/{username}/{password}"] = _ =>
            {
                if (IsDebug || session.IsLogin && session.User.Email == "jkwchunjae@gmail.com")
                {
                    string username = _.username;
                    string password = _.password;

                    var loginData = LoadLoginData();

                    if (loginData.Any(x => x.Username == username))
                        return "이미 존재하는 아이디입니다.";

                    loginData.Add(new SimpleLoginData { Username = username, Password = password });
                    File.WriteAllText(_simpleLoginPath, JsonConvert.SerializeObject(loginData, Formatting.Indented), Encoding.UTF8);

                    return $"{username} {password}";
                }
                return Response.AsRedirect("/worldcup");
            };

            Post["/worldcup/simplelogin"] = _ =>
            {
                string username = Request.Form["username"];
                string password = Request.Form["password"];

                var loginData = LoadLoginData();
                if (loginData.Any(x => x.Username == username && x.Password == password))
                {
                    _simpleLoginDic[sessionId] = new SimpleLoginData
                    {
                        Username = username,
                        Password = password,
                    };
                }

                return Response.AsRedirect("/worldcup");
            };

            Get["/worldcup/logout"] = _ =>
            {
                _simpleLoginDic.Remove(sessionId);
                return Response.AsRedirect("/worldcup");
            };

            Get["/worldcup/select16"] = _ =>
            {
                if (!(session.IsLogin || _simpleLoginDic.ContainsKey(sessionId)))
                    return "[]";

                var bettingName = "16강 맞추기";
                var username = session.IsLogin ? session.User.Email : _simpleLoginDic[sessionId].Username;

                var bettingData = WorldcupBettingManager.GetBettingData(bettingName);
                if (bettingData == null)
                    return "[]";
                if (!bettingData.UserBettingList.ContainsKey(username))
                    return "[]";
                var userBettings = bettingData.UserBettingList[username]
                    .BettingList
                    .Select(x => new { groupCode = x.Id.Substring(0, 1), teamCode = x.Value })
                    .ToList();
                return JsonConvert.SerializeObject(userBettings);
            };

            Post["/worldcup/getdata"] = _ =>
            {
                if (IsDebug || session.IsLogin && session.User.Email == "jkwchunjae@gmail.com")
                {
                    string bettingName = Request.Form["bettingName"];
                    var bettingData = WorldcupBettingManager.GetBettingData(bettingName);
                    return JsonConvert.SerializeObject(bettingData);
                }
                return "{}";
            };

            Post["/worldcup/select16"] = _ =>
            {
                if (!(session.IsLogin || _simpleLoginDic.ContainsKey(sessionId)))
                    return "로그인을 하십시오. 구글로그인은 빠르고 편리합니다.";

                var bettingName = "16강 맞추기";
                var username = session.IsLogin ? session.User.Email : _simpleLoginDic[sessionId].Username;

                string selectedTeamText = Request.Form["selectedTeam"];

                WorldcupBettingManager.Log(username, "select16", selectedTeamText);

                var userBettings = JsonConvert.DeserializeObject<List<dynamic>>(selectedTeamText)
                    .Select(x => new { GroupCode = (string)x["groupCode"], TeamCode = (string)x["teamCode"] })
                    .OrderBy(x => x.GroupCode)
                    .GroupBy(x => new { x.GroupCode })
                    .Select(x => new { x.Key.GroupCode, Teams = x.Select((e, i) => new { Index = i + 1, e.TeamCode }) })
                    .SelectMany(x => x.Teams.Select(e => new { x.GroupCode, e.Index, e.TeamCode }))
                    .Select(x => new UserBetting { Id = $"{x.GroupCode}{x.Index}", Value = x.TeamCode })
                    .ToList();

                var bettingData = WorldcupBettingManager.GetBettingData(bettingName);

                if (bettingData.FreezeTime < DateTime.Now)
                    return "이제 변경할 수 없습니다.";

                var result = bettingData.UpdateUserBettingData(username, userBettings, false);
                return result ? "저장되었습니다." : "이제 변경할 수 없습니다.";
            };

            Get["/worldcup/round16"] = _ =>
            {
                if (!(session.IsLogin || _simpleLoginDic.ContainsKey(sessionId)))
                    return "[]";

                var bettingName = "round16";
                var username = session.IsLogin ? session.User.Email : _simpleLoginDic[sessionId].Username;

                var bettingData = WorldcupBettingManager.GetBettingData(bettingName);
                if (bettingData == null)
                    return "[]";
                if (!bettingData.UserBettingList.ContainsKey(username))
                    return "[]";
                var userBettings = bettingData.UserBettingList[username]
                    .BettingList
                    .Select(x => new { matchId = x.Id, teamCode = x.Value })
                    .ToList();
                return JsonConvert.SerializeObject(userBettings);
            };

            Post["/worldcup/round16"] = _ =>
            {
                if (!(session.IsLogin || _simpleLoginDic.ContainsKey(sessionId)))
                    return "로그인을 하십시오. 구글로그인은 빠르고 편리합니다.";

                var bettingName = "round16";
                var username = session.IsLogin ? session.User.Email : _simpleLoginDic[sessionId].Username;

                string selectedTeamText = Request.Form["selectedTeam"];

                WorldcupBettingManager.Log(username, "round16", selectedTeamText);

                var knockoutData = WorldcupBettingManager.KnockoutData;
                var bettingData = WorldcupBettingManager.GetBettingData(bettingName);

                var freezeMatchList = knockoutData.Round16.Where(x => x.IsFreeze).Select(x => x.MatchId).ToList();

                var userBettings = JsonConvert.DeserializeObject<List<dynamic>>(selectedTeamText)
                    .Select(x => new UserBetting { Id = (string)x["matchId"], Value = (string)x["teamCode"] })
                    .Where(x => !freezeMatchList.Contains(x.Id))
                    .OrderBy(x => x.Id)
                    .ToList();

                var notyetList = knockoutData.Round16.Where(x => !x.IsFreeze);
                Model.FreezeTime = notyetList.Any() ? notyetList.Min(x => x.GameStartTime) :
                    knockoutData.Round16.OrderBy(x => x.GameStartTime).Last().GameStartTime;

                var freezeTime = knockoutData.Round16.Where(x => !x.IsFreeze)?.Min(x => x.GameStartTime);
                if (freezeTime == null || freezeTime < DateTime.Now)
                    return "이제 변경할 수 없습니다.";

                if (bettingData.UserBettingList.ContainsKey(username))
                {
                    // 변경 할 수 없는 데이터와 합친다.
                    var currentBettings = bettingData.UserBettingList[username].BettingList;
                    userBettings = currentBettings.Where(x => freezeMatchList.Contains(x.Id))
                        .Concat(userBettings)
                        .OrderBy(x => x.Id)
                        .ToList();
                }

                var result = bettingData.UpdateUserBettingData(username, userBettings, true);
                return result ? "저장되었습니다." : "이제 변경할 수 없습니다.";
            };

            Get["/worldcup/final"] = _ =>
            {
                if (!(session.IsLogin || _simpleLoginDic.ContainsKey(sessionId)))
                    return "[]";

                var bettingName = "final";
                var username = session.IsLogin ? session.User.Email : _simpleLoginDic[sessionId].Username;

                var bettingData = WorldcupBettingManager.GetBettingData(bettingName);
                if (bettingData == null)
                    return "[]";
                if (!bettingData.UserBettingList.ContainsKey(username))
                    return "[]";
                var userBettings = bettingData.UserBettingList[username]
                    .BettingList
                    .Select(x => new { matchId = x.Id, teamCode = x.Value })
                    .ToList();
                return JsonConvert.SerializeObject(userBettings);
            };

            Post["/worldcup/final"] = _ =>
            {
                if (!(session.IsLogin || _simpleLoginDic.ContainsKey(sessionId)))
                    return "로그인을 하십시오. 구글로그인은 빠르고 편리합니다.";

                var bettingName = "final";
                var username = session.IsLogin ? session.User.Email : _simpleLoginDic[sessionId].Username;

                string selectedTeamText = Request.Form["selectedTeam"];
                bool isRandom = Request.Form["isRandom"];

                WorldcupBettingManager.Log(username, "round16", selectedTeamText);

                var knockoutData = WorldcupBettingManager.KnockoutData;
                var bettingData = WorldcupBettingManager.GetBettingData(bettingName);

                if (bettingData.FreezeTime < DateTime.Now)
                    return "이제 변경할 수 없습니다.";

                if (bettingData.RandomSelectedUser.Contains(username))
                    return "한 번 랜덤 선택하면 변경할 수 없습니다.";

                var userBettingsTemp = JsonConvert.DeserializeObject<List<dynamic>>(selectedTeamText)
                    .Select(x => new { Id = (string)x["matchId"], Value = (string)x["teamCode"], OtherTeamCode = (string)x["otherTeamCode"] })
                    .ToList();

                if (userBettingsTemp.Any(x => x.Id == "FINAL"))
                {
                    var matchInfo = userBettingsTemp.First(x => x.Id == "FINAL");
                    userBettingsTemp.Add(new { Id = "Champion", Value = matchInfo.Value, OtherTeamCode = "" });
                    userBettingsTemp.Add(new { Id = "Second", Value = matchInfo.OtherTeamCode, OtherTeamCode = "" });
                }

                if (userBettingsTemp.Any(x => x.Id == "THIRD"))
                {
                    var matchInfo = userBettingsTemp.First(x => x.Id == "THIRD");
                    userBettingsTemp.Add(new { Id = "Third", Value = matchInfo.Value, OtherTeamCode = "" });
                    userBettingsTemp.Add(new { Id = "Fourth", Value = matchInfo.OtherTeamCode, OtherTeamCode = "" });
                }

                var userBettings = userBettingsTemp
                    .Select(x => new UserBetting { Id = x.Id, Value = x.Value })
                    .ToList();

                if (isRandom)
                {
                    bettingData.RandomSelectedUser.Add(username);
                }
                var result = bettingData.UpdateUserBettingData(username, userBettings, true);
                return result ? "저장되었습니다." : "저장 실패";
            };

            Post["/worldcup/applydata"] = _ =>
            {
                if (IsDebug || session.IsLogin && session.User.Email == "jkwchunjae@gmail.com")
                {
                    string bettingDataText = Request.Form["bettingData"];

                    WorldcupBettingManager.Log("admin", "applyBettingData", bettingDataText);

                    var bettingData = JsonConvert.DeserializeObject<BettingData>(bettingDataText);
                    WorldcupBettingManager.UpdateData(bettingData);
                    bettingData.UpdateAllotmentAmount().Save();
                    return true;
                }
                return false;
            };
        }
    }
}
