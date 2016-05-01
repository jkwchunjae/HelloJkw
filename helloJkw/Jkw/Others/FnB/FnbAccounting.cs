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

		public static bool AddData(AccountingData newData)
		{
			_dataList.Add(newData);
			return Save();
		}

		public static bool DeleteData(AccountingData deleteData)
		{
			if (!_dataList.Contains(deleteData))
				return false;

			_dataList.Remove(deleteData);
			return true;
		}
	}
}
