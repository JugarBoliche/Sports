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
		public DatePlayerStats(YahooAPI yahoo, XElement xml, string playerKey) : base(yahoo, xml, playerKey)
		{
			if (!"date".Equals(_coverageType, StringComparison.CurrentCultureIgnoreCase) || !_date.HasValue)
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
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/players/stats;type=date;date={1}", teamKey, date));
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "player"))
			{
				XElement playerKeyXml = descendantXml.Element(_yns + "player_key");
				if (playerKeyXml == null)
					throw new Exception("Couldn't find player_key in stats");
				string playerKey = playerKeyXml.Value;
				playerStats.Add(new DatePlayerStats(yahoo, descendantXml, playerKey));
			}
			return playerStats;
		}

		public DateTime Date
		{
			get
			{
				return _date.Value;
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
