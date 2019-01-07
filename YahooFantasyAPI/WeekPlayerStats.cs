using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public class WeekPlayerStats : PlayerStats
	{
		public WeekPlayerStats(YahooAPI yahoo, XElement xml, string teamKey) : base(yahoo, xml, teamKey)
		{
			if(Coverage != CoverageType.Week)
			{
				throw new Exception("Stats do not represent a week player stats, or the week information is missing.");
			}
		}

		public static List<WeekPlayerStats> GetWeeklyPlayerStats(YahooAPI yahoo, string teamKey, int week)
		{
			List<WeekPlayerStats> playerStats = new List<WeekPlayerStats>();
			//XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/players/stats;type=week;week={1}", teamKey, week));
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/roster;week={1}/players/stats;type=week;week={1}", teamKey, week));
			
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "player"))
			{
				playerStats.Add(new WeekPlayerStats(yahoo, descendantXml, teamKey));
			}
			return playerStats;
		}

		public new int Week
		{
			get
			{
				return base.Week.Value;
			}
		}
	}
}
