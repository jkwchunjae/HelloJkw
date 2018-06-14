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
            Get["/worldcup"] = _ =>
            {
                return Response.AsRedirect("/worldcup/2018");
            };

            Get["/worldcup/2018"] = _ =>
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
                    ?.Select(x =>
                    {
                        x.Username = x.Username.Left(3) + "***";
                        return x;
                    })?.ToList() ?? new List<DashboardItem>();

                Model.SampleList = sampleList;
                Model.GroupList = WorldcupBettingManager.GroupDataList;
                Model.Dashboard = dashboard;
                Model.FreezeTime = bettingData.FreezeTime;
                return View["Games/Worldcup/worldcupHome.cshtml", Model];
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

                var result = bettingData.UpdateUserBettingData(username, userBettings);
                return result ? "저장되었습니다." : "이제 변경할 수 없습니다.";
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
