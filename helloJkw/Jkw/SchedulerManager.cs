using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class Schedule
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public int Date { get; set; }
		public int Time { get; set; }
		public int Duration { get; set; }
		public string Title { get; set; }
	}

	public static class SchedulerManager
	{
		public static bool AddSchedule(Schedule schedule)
		{
			try
			{
				var query = @"insert into scheduler (userid, date, time, duration, title)
									values (@userId, @date, @time, @duration, @title);";
				var cmd = query.CreateCommand();
				cmd.Parameters.AddWithValue("@userId", schedule.UserId);
				cmd.Parameters.AddWithValue("@date", schedule.Date);
				cmd.Parameters.AddWithValue("@time", schedule.Time);
				cmd.Parameters.AddWithValue("@duration", schedule.Duration);
				cmd.Parameters.AddWithValue("@title", schedule.Title);

				if (cmd.ExecuteNonQuery() == 0)
					return false;

				schedule.Id = (int)cmd.LastInsertedId;
			}
			catch
			{
				return false;
			}
			return true;
		}
	}
}
