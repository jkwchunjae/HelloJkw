using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using helloJkw.Jkw.Others.FnB;
using Newtonsoft.Json;
using System.Dynamic;

namespace helloJkw
{
	public class JkwFnbModule : JkwModule
	{
		static string _operatorPath = @"jkw/project/fnb/json/operator.json";
		public bool IsOperator(Session session)
		{
			if (!session.IsLogin || session.IsExpired)
				return false;

			var operatorList = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(_operatorPath, Encoding.UTF8))
				.Select(x => x.ToLower()).ToList();

			if (session.User.Email == null)
				throw new Exception("해당 계정에 이메일이 세팅되어 있지 않습니다.");
			return operatorList.Contains(session.User.Email.ToLower());
		}

		public JkwFnbModule()
		{
			#region Get /fnb

			Get["/fnb/"] = _ =>
			{
				#region Background image

				Model.BackGroundFileName = Directory.GetFiles(@"jkw/static/others/fnb/background", "*").GetRandom()
					.Replace(@"\", "/").Replace(@"/static/", @"/");

				#endregion

				#region MemberList

				Model.RegularMemberList = FnbMember.GetMember(FnbMember.MemberType.Regular);
				Model.AssociateMemberList = FnbMember.GetMember(FnbMember.MemberType.Associate);

				#endregion

				#region Meetings

				Model.MeetingList = FnbMeeting.GetMeeting();

				#endregion

				#region Accounting

				Model.Accounting = FnbAccounting.GetAccountingData();

				#endregion

				try
				{
					Model.IsOperator = IsOperator(session);
				}
				catch
				{
					Model.IsOperator = false;
				}

				return View["fnb/fnbHome.cshtml", Model];
			};

			#endregion

			#region Member

			#region request member

			Post["/fnb/member/request"] = _ =>
			{
				string memberTypeStr = Request.Form["memberType"];
				var memberType = (FnbMember.MemberType)Enum.Parse(typeof(FnbMember.MemberType), memberTypeStr);
				var memberList = FnbMember.GetMember(memberType);
				var json = JsonConvert.SerializeObject(memberList);
				return json;
			};

			#endregion

			#region Manage

			Post["/fnb/member/changeType"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				try
				{
					string memberName = Request.Form["memberName"];
					string memberTypeStr = Request.Form["newMemberType"];
					var newMemberType = (FnbMember.MemberType)Enum.Parse(typeof(FnbMember.MemberType), memberTypeStr);

					FnbMember.ChangeMemberType(memberName, newMemberType);
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};

			Post["/fnb/member/add"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				try
				{
					string name = Request.Form["name"];
					string role = Request.Form["role"];
					string team = Request.Form["team"];
					var joinDate = ((string)Request.Form["joinDate"]).ToDate();
					string memberTypeStr = Request.Form["memberType"];
					var memberType = (FnbMember.MemberType)Enum.Parse(typeof(FnbMember.MemberType), memberTypeStr);

					if (string.IsNullOrWhiteSpace(name))
						throw new Exception("이름이 공백이면 안됩니다.");
					if (string.IsNullOrWhiteSpace(team))
						throw new Exception("팀이 공백이면 안됩니다.");
					if (joinDate == DateTime.MinValue)
						throw new Exception("날짜에 오류가 있습니다.");

					var member = new FnbMember.Member
					{
						Name = name.Trim(),
						Role = role.Trim(),
						Team = team.Trim(),
						JoinDate = joinDate,
						MemberType = memberType
					};

					FnbMember.AddMember(member);
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};

			Post["/fnb/member/edit"] = _ =>
			{
				if (!IsOperator(session))
					return "권리자 권한이 없습니다.";

				try
				{
					string name = Request.Form["name"];
					string role = Request.Form["role"];
					string team = Request.Form["team"];
					var joinDate = ((string)Request.Form["joinDate"]).ToDate();

					var member = new FnbMember.Member
					{
						Name = name.Trim(),
						Team = team.Trim(),
						Role = role.Trim(),
						JoinDate = joinDate
					};

					FnbMember.EditMember(name, member);
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};

			Post["/fnb/member/leave"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				try
				{
					string memberName = Request.Form["memberName"];

					FnbMember.LeaveMember(memberName);
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};

			#endregion

			#endregion

			#region Meeting

			#region Request meeting

			Post["/fnb/meeting/request"] = _ =>
			{
				var meetingList = FnbMeeting.GetMeeting()
					.Select(e => new
					{
						No = e.No,
						DateDot = e.Date.ToString("yyyy.MM.dd"),
						e.Attendants,
						AttendantList = e.Attendants.Select(x => FnbMember.GetMember(x))
							.OrderBy(x => x.MemberType).ThenBy(x => x.JoinDate)
							.Select(x => x.Name)
							.StringJoin(","),
						Others = e.Others,
					});
				var json = JsonConvert.SerializeObject(meetingList);
				return json;
			};

			#endregion

			#region AddMeeting

			Post["fnb/check/member"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				var memberList = ((string)Request.Form["memberlist"])
					.Split(new[] { ',', ' ' })
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.Select(x => FnbMember.GetMember(x))
					.Where(x => x.MemberType == FnbMember.MemberType.None)
					.ToList();

				if (memberList.Any())
				{
					return "멤버가 아닌 이름이 있습니다. " + memberList.Select(x => x.Name).StringJoin(", ");
				}
				return "success";
			};

			Post["/fnb/meeting/add"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				try
				{
					int no = Request.Form["no"];
					DateTime date = ((string)Request.Form["date"]).ToDate();
					List<string> attendants = ((string)Request.Form["attendants"])
						.Split(new[] { ',', ' ' })
						.Where(x => !string.IsNullOrWhiteSpace(x))
						.ToList();
					string others = Request.Form["others"];

					if (date == DateTime.MinValue)
						throw new Exception("날짜에 오류가 있습니다.");

					FnbMeeting.AddMeeting(new FnbMeeting.Meeting { No = no, Date = date, Attendants = attendants, Others = others });
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};
			#endregion

			#region EditMeeting

			Post["/fnb/meeting/edit"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				try
				{
					int no = Request.Form["no"];
					var date = ((string)Request.Form["date"]).ToDate();
					var attendants = ((string)Request.Form["attendants"])
						.Split(new[] { ',', ' ' })
						.Where(x => !string.IsNullOrWhiteSpace(x))
						.ToList();
					string others = Request.Form["others"];

					if (date == DateTime.MinValue)
						throw new Exception("날짜에 오류가 있습니다.");

					var meeting = new FnbMeeting.Meeting
					{
						No = no,
						Date = date,
						Attendants = attendants,
						Others = others,
					};

					FnbMeeting.EditMeeting(no, meeting);
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};

			#endregion

			#region DeleteMeeting

			Post["/fnb/meeting/delete"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				try
				{
					int no = Request.Form["no"];
					FnbMeeting.DeleteMeeting(no);
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};

			#endregion

			#endregion

			#region Accounting

			#region Request accounting data

			Post["fnb/accounting/request"] = _ =>
			{
				var accountingDataList = FnbAccounting.GetAccountingData()
					.Select(x => new
					{
						x.Id,
						DateDot = x.Date.ToString("yyyy.MM.dd"),
						x.Content,
						x.InMoney,
						x.OutMoney,
						InMoneyComma = x.InMoney != 0 ? x.InMoney.ToComma() : "-",
						OutMoneyComma = x.OutMoney != 0 ? x.OutMoney.ToComma() : "-",
					});
				var totalOutMoney = FnbAccounting.GetAccountingData().Sum(x => x.OutMoney).ToComma();
				var totalInMoney = FnbAccounting.GetAccountingData().Sum(x => x.InMoney).ToComma();

				dynamic obj = new ExpandoObject();
				obj.accountingDataList = accountingDataList;
				obj.totalOutMoney = totalOutMoney;
				obj.totalInMoney = totalInMoney;

				var json = JsonConvert.SerializeObject(obj);
				return json;
			};

			#endregion

			#region Manage

			Post["/fnb/accounting/add"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				try
				{
					var date = ((string)Request.Form["date"]).ToDate();
					string content = Request.Form["content"];
					long outMoney = ((string)Request.Form["outMoney"]).ToLong();
					long inMoney = ((string)Request.Form["inMoney"]).ToLong();

					if (date == DateTime.MinValue)
						throw new Exception("날짜를 입력하세요");

					if (string.IsNullOrWhiteSpace(content))
						throw new Exception("내용을 입력하세요");

					if (outMoney == 0 && inMoney == 0)
						throw new Exception("지출, 수입 모두 0이면 안됩니다.");

					if (outMoney != 0 && inMoney != 0)
						throw new Exception("지출, 수입 모두 값이 있으면 안됩니다.");

					if (outMoney < 0 || inMoney < 0)
						throw new Exception("지출, 수입 양수만 가능합니다.");

					var accountingData = new FnbAccounting.AccountingData
					{
						Id = FnbAccounting.GetAccountingData().Any() ? FnbAccounting.GetAccountingData().Max(x => x.Id) + 1 : 1,
						Date = date,
						Content = content,
						OutMoney = outMoney,
						InMoney = inMoney,
					};

					FnbAccounting.AddData(accountingData);
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};

			Post["/fnb/accounting/edit"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				try
				{
					int id = Request.Form["id"];
					var date = ((string)Request.Form["date"]).ToDate();
					string content = Request.Form["content"];
					long outMoney = ((string)Request.Form["outMoney"]).ToLong();
					long inMoney = ((string)Request.Form["inMoney"]).ToLong();

					if (date == DateTime.MinValue)
						throw new Exception("날짜를 입력하세요");

					if (string.IsNullOrWhiteSpace(content))
						throw new Exception("내용을 입력하세요");

					if (outMoney == 0 && inMoney == 0)
						throw new Exception("지출, 수입 모두 0이면 안됩니다.");

					if (outMoney != 0 && inMoney != 0)
						throw new Exception("지출, 수입 모두 값이 있으면 안됩니다.");

					if (outMoney < 0 || inMoney < 0)
						throw new Exception("지출, 수입 양수만 가능합니다.");

					var accountingData = new FnbAccounting.AccountingData
					{
						Id = id,
						Date = date,
						Content = content,
						OutMoney = outMoney,
						InMoney = inMoney,
					};

					FnbAccounting.EditData(id, accountingData);
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};

			Post["/fnb/accounting/delete"] = _ =>
			{
				if (!IsOperator(session))
					return "관리자 권한이 없습니다.";

				try
				{
					int id = Request.Form["id"];

					FnbAccounting.DeleteData(id);
					return "success";
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
			};

			#endregion

			#endregion
		}
	}
}
