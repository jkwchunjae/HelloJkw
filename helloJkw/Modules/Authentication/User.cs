using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw
{
	public enum UserGrade
	{
		Admin,
		Family,
		Friend,
		Someone,
	}

	public class User
	{
		string _imageUrl;

		public readonly int No; // 회원가입 일련번호
		public readonly string Id; // email id 가 아닌 google 고유 번호
		public string Name;
		public string Email;
		public UserGrade Grade;
		public bool IsUseGoogleImage;

		public readonly DateTime RegDate;
		public DateTime LastLogin;

		#region Properties

		#region ImageUrl
		public string ImageUrl
		{
			get
			{
				return _imageUrl;
			}

			set
			{
				if (value == null)
				{
					_imageUrl = _images.GetRandom();
				}
				else
				{
					_imageUrl = value;
				}
			}
		}
		#endregion

		#endregion

		#region static Fields, Methods
		static List<string> _images;
		static User()
		{
			try
			{
				var imagesPath = @"Static/image/user-image-sample";
				_images = Directory.GetFiles(imagesPath).ToList();
				_images = _images
					.Select(e => e.Replace(@"\", "/").RegexReplace(@".*?/*" + imagesPath, "/" + imagesPath))
					.ToList();
			}
			catch
			{
				_images = new List<string>();
			}
		}
		#endregion

		public User(int no, string id, DateTime regDate)
		{
			No = no;
			Id = id;
			RegDate = regDate;
			IsUseGoogleImage = true;
		}
	}
}
