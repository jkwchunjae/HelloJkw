using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Extensions;

namespace helloJkw.Jkw.Others.FnB
{
	public static class FnbMember
	{
		/// <summary> 정회원, 준회원 </summary>
		public enum MemberType
		{
			None, Regular, Associate, Leave, 
		}

		public class Member
		{
			public string Name;
			public string Team;
			public string Role;
			public DateTime JoinDate;
			public MemberType MemberType = MemberType.None;
			public string PictureName
			{
				get
				{
					var images = Directory.GetFiles(_picturePath, "{0}.*".With(Name));
					if (!images.Any())
					{
						return "person.png";
					}
					return Path.GetFileName(images.First());
				}
			}

			public bool HasRole { get { return !string.IsNullOrWhiteSpace(Role); } }
			public string JoinDateDot { get { return JoinDate.ToString("yyyy.MM.dd"); } }

			public string GetShortInfo()
			{
				var format = "{Name}({Type})";
				return format.WithVar(new { Name = Name, Type = MemberType == MemberType.Regular ? "정" : MemberType == MemberType.Associate ? "준" : "" });
			}
		}

		static string _path = @"jkw/project/fnb/json/member.json";
		static string _picturePath = @"jkw/static/others/fnb/member";
		static List<Member> _memberList;

		static FnbMember()
		{
			Load();
		}

		#region Load & Save

		static void Load()
		{
			try
			{
				_memberList = JsonConvert.DeserializeObject<List<Member>>(File.ReadAllText(_path, Encoding.UTF8));
				/*
				var pictureDic = Directory.GetFiles(_picturePath)
					.ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => Path.GetFileName(x));
				foreach (var member in _memberList)
				{
					if (pictureDic.ContainsKey(member.Name))
					{
						member.PictureName = pictureDic[member.Name];
					}
					else
					{
						member.PictureName = "person.png";
					}
				}
				*/
			}
			catch
			{
				_memberList = new List<Member>();
			}
		}

		static bool Save()
		{
			try
			{
				string json = JsonConvert.SerializeObject(_memberList, Formatting.Indented);
				File.WriteAllText(_path, json, Encoding.UTF8);
				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		public static IEnumerable<Member> GetMember()
		{
			return _memberList.OrderBy(x => x.JoinDate);
		}

		public static IEnumerable<Member> GetMember(MemberType memberType)
		{
			return _memberList.Where(x => x.MemberType == memberType).OrderBy(x => x.JoinDate);
		}

		public static Member GetMember(string memberName)
		{
			if (_memberList.Any(x => x.Name == memberName))
				return _memberList.First(x => x.Name == memberName);
			return new Member() { Name = memberName, JoinDate = DateTime.MaxValue, MemberType = MemberType.None };
		}

		public static void AddMember(Member newMember)
		{
			if (_memberList.Any(x => x.Name == newMember.Name))
				throw new Exception("중복된 이름이 있습니다.");

			_memberList.Add(newMember);
			if (!Save())
			{
				_memberList.Remove(newMember);
				throw new Exception("추가 작업에 실패했습니다.");
			}
		}

		public static void LeaveMember(string memberName)
		{
			ChangeMemberType(memberName, MemberType.Leave);
		}

		public static void DeleteMember(string memberName)
		{
			if (!_memberList.Any(x => x.Name == memberName))
				throw new Exception("없는 이름입니다.");

			var member = _memberList.First(x => x.Name == memberName);
			_memberList.Remove(member);

			if (!Save())
			{
				_memberList.Add(member);
				throw new Exception("삭제 작업에 실패했습니다.");
			}
		}

		public static void EditMember(string memberName, Member newMember)
		{
			if (!_memberList.Any(x => x.Name == memberName))
				throw new Exception("없는 이름입니다.");

			var member = _memberList.First(x => x.Name == memberName);

			var oldTeam = member.Team;
			var oldRole = member.Role;
			var oldJoinDate = member.JoinDate;

			member.Team = newMember.Team;
			member.Role = newMember.Role;
			member.JoinDate = newMember.JoinDate;

			if (!Save())
			{
				member.Team = oldTeam;
				member.Role = oldRole;
				member.JoinDate = oldJoinDate;
				throw new Exception("정보 변경에 실패했습니다.");
			}
		}

		public static void ChangeMemberType(string memberName, MemberType newMemberType)
		{
			var member = GetMember(memberName);
			var oldMemberType = member.MemberType;
			member.MemberType = newMemberType;

			if (!Save())
			{
				member.MemberType = oldMemberType;
				throw new Exception("정보 변경에 실패했습니다.");
			}
		}
	}
}
