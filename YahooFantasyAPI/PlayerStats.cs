using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public class PlayerStats : YahooObjectBase
	{
		private string _playerKey;
		private StatLine _playerStats = null;
		protected string _coverageType;
		protected int? _week;
		protected DateTime? _date;

		public PlayerStats(YahooAPI yahoo, XElement xml, string playerKey) : base(yahoo, xml)
		{
			// Assumes root node is <player> with a child of <player_stats> and a player_key be passed in for the player data to be extracted
			_playerKey = playerKey;

			bool playerDataFound = false;
			//foreach (XElement playerXml in GetDescendants("player"))
			//{
				if (GetElementAsString(xml, "player_key").Equals(playerKey))
				{
					playerDataFound = true;
					XElement playerStats = GetElement(xml, "player_stats");
					_playerStats = new StatLine(yahoo, playerStats);
					_coverageType = GetElementAsString(playerStats, "coverage_type");
					if("week".Equals(_coverageType, StringComparison.CurrentCultureIgnoreCase))
					{
						_week = GetElementAsInt(playerStats, "week");
					}
					else if ("date".Equals(_coverageType, StringComparison.CurrentCultureIgnoreCase))
					{
						_date = GetElementAsDateTime(playerStats, "date");
					}
				}
			//}
			if (!playerDataFound)
				throw new Exception("Player data not found in matchup for player key provided (" + playerKey + ").");
		}


		public string PlayerKey
		{
			get
			{
				return _playerKey;
			}
		}

		public StatLine Stats
		{
			get
			{
				return _playerStats;
			}
		}
	}
}
