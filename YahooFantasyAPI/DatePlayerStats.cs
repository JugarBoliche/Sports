using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public enum PlayedStatus
	{
		Played,
		DNP,
		NoGame
	}
	public class DatePlayerStats : PlayerStats
	{
		public DatePlayerStats(YahooAPI yahoo, XElement xml, string teamKey) : base(yahoo, xml, teamKey)
		{
			if (Coverage != CoverageType.Date)
			{
				throw new Exception("Stats do not represent a date player stats, or the dateinformation is missing.");
			}
		}
		public static List<DatePlayerStats> GetDatePlayerStats(YahooAPI yahoo, string teamKey, DateTime date)
		{
			return GetDatePlayerStats(yahoo, teamKey, date.ToString("yyyy-MM-dd"));
		}
		public static List<DatePlayerStats> GetDatePlayerStats(YahooAPI yahoo, string teamKey, string date)
		{
			List<DatePlayerStats> playerStats = new List<DatePlayerStats>();
			//XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/players/stats;type=date;date={1}", teamKey, date));
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/roster;date={1}/players/stats;type=date;date={1}", teamKey, date));
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "player"))
			{
				playerStats.Add(new DatePlayerStats(yahoo, descendantXml, teamKey));
			}
			return playerStats;
		}

		public new DateTime Date
		{
			get
			{
				return base.Date.Value;
			}
		}

		public PlayedStatus Status
		{
			get
			{
				if(!Stats.StatsNotNull)
				{
					return PlayedStatus.NoGame;
				}
				else if((Stats.Points + Stats.Rebounds + Stats.Assists + Stats.Steals + Stats.Blocks) == 0)
				{
					return PlayedStatus.DNP;
				}
				else
				{
					return PlayedStatus.Played;
				}
			}
		}
			
	}
}
