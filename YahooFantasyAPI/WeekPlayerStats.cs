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
		public WeekPlayerStats(YahooAPI yahoo, XElement xml, string playerKey) : base(yahoo, xml, playerKey)
		{
			if(!"week".Equals(_coverageType, StringComparison.CurrentCultureIgnoreCase) || !_week.HasValue)
			{
				throw new Exception("Stats do not represent a week player stats, or the week information is missing.");
			}
		}

		public static List<WeekPlayerStats> GetWeeklyPlayerStats(YahooAPI yahoo, string teamKey, int week)
		{
			List<WeekPlayerStats> playerStats = new List<WeekPlayerStats>();
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/players/stats;type=week;week={1}", teamKey, week));
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "player"))
			{
				XElement playerKeyXml = descendantXml.Element(_yns + "player_key");
				if (playerKeyXml == null)
					throw new Exception("Couldn't find player_key in stats");
				string playerKey = playerKeyXml.Value;
				playerStats.Add(new WeekPlayerStats(yahoo, descendantXml, playerKey));
			}
			return playerStats;
		}

		public int Week
		{
			get
			{
				return _week.Value;
			}
		}
	}
}
