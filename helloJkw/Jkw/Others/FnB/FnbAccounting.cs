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
	public static class FnbAccounting
	{
		public class AccountingData
		{
			/// <summary> 그냥 고유 일련번호 </summary>
			public int Id;
			public DateTime Date;
			public string Content;
			public long InMoney;
			public long OutMoney;
		}

		static string _path = @"jkw/project/fnb/json/accounting.json";
		static List<AccountingData> _dataList;

		static FnbAccounting()
		{
			Load();
		}

		#region Load & Save

		static void Load()
		{
			try
			{
				_dataList = JsonConvert.DeserializeObject<List<AccountingData>>(File.ReadAllText(_path, Encoding.UTF8));
			}
			catch
			{
				_dataList = new List<AccountingData>();
			}
		}

		static bool Save()
		{
			try
			{
				string json = JsonConvert.SerializeObject(_dataList, Formatting.Indented);
				File.WriteAllText(_path, json, Encoding.UTF8);
				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		public static IEnumerable<AccountingData> GetAccountingData()
		{
			return _dataList.OrderByDescending(x => x.Date);
		}

		public static IEnumerable<AccountingData> GetAccountingData(DateTime beginDate, DateTime endDate)
		{
			return _dataList.Where(x => x.Date >= beginDate && x.Date <= endDate).OrderByDescending(x => x.Date);
		}

		public static void AddData(AccountingData newData)
		{
			_dataList.Add(newData);
			if (!Save())
			{
				_dataList.Remove(newData);
				throw new Exception("추가 작업에 실패했습니다.");
			}
		}

		public static void EditData(int id, AccountingData newData)
		{
			if (!_dataList.Any(x => x.Id == id))
				throw new Exception("잘못된 ID 입니다. (ID = {0})".With(id));

			var data = _dataList.First(x => x.Id == id);

			var oldDate = data.Date;
			var oldContent = data.Content;
			var oldOutMoney = data.OutMoney;
			var oldInMoney = data.InMoney;

			data.Date = newData.Date;
			data.Content = newData.Content;
			data.OutMoney = newData.OutMoney;
			data.InMoney = newData.InMoney;

			if (!Save())
			{
				data.Date = oldDate;
				data.Content = oldContent;
				data.OutMoney = oldOutMoney;
				data.InMoney = oldInMoney;
				throw new Exception("정보 변경에 실패했습니다.");
			}
		}

		public static bool DeleteData(int id)
		{
			if (!_dataList.Any(x => x.Id == id))
				throw new Exception("Id를 찾지 못했습니다.");

			var data = _dataList.First(x => x.Id == id);
			_dataList.Remove(data);

			if (!Save())
			{
				_dataList.Add(data);
				throw new Exception("삭제 작업에 실패했습니다.");
			}
			return true;
		}
	}
}
