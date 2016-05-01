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
		public bool IsOperator(Session session)
		{
			if (!session.IsLogin || session.IsExpired)
				return false;
			return true;
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

				Model.IsOperator = session.IsLogin;

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
						AttendantList = e.Attendants.Select(x => FnbMember.GetMember(x))
							.OrderBy(x => x.MemberType).ThenBy(x => x.JoinDate)
							.Select((x, i) => new { Index = i, Member = x })
							.GroupBy(x => x.Index / 5)
							.Select(x => x.Select(t => t.Member.GetShortInfo()).StringJoin(", "))
							.StringJoin("<br/>"),
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

					FnbMeeting.AddMeeting(new FnbMeeting.Meeting { No = no, Date = date, Attendants = attendants, Others = others });
				}
				catch (Exception ex)
				{
					return ex.Message;
				}

				return "success";
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
						InMoney = x.InMoney != 0 ? x.InMoney.ToComma() : "-",
						OutMoney = x.OutMoney != 0 ? x.OutMoney.ToComma() : "-",
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

			#endregion
		}
	}
}
