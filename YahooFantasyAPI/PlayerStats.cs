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
		public enum CoverageType
		{
			Other,
			Week,
			Date
		}

		private StatLine _playerStats = null;
		private CoverageType _coverageType;
		private string _teamKey;
		private Player _player;

		public PlayerStats(YahooAPI yahoo, XElement xml, string team_Key) : base(yahoo, xml)
		{
			_teamKey = team_Key;
			// Assumes root node is <player> with a child of <player_stats>
			XElement playerStats = GetElement(xml, "player_stats");
			_playerStats = new StatLine(yahoo, playerStats);
			string coverageType = GetElementAsString(playerStats, "coverage_type");

			if ("week".Equals(coverageType, StringComparison.CurrentCultureIgnoreCase))
			{
				_coverageType = CoverageType.Week;
			}
			else if ("date".Equals(coverageType, StringComparison.CurrentCultureIgnoreCase))
			{
				_coverageType = CoverageType.Date;
			}
			else
			{
				_coverageType = CoverageType.Other;
			}
		}


		public string PlayerKey
		{
			get
			{
				return GetElementAsString("player_key"); ;
			}
		}

		public Player Player
		{
			get
			{
				if(_player == null)
				{
					_player = new Player(Yahoo, Xml);
				}
				return _player;
			}
		}

		public string TeamKey
		{
			get
			{
				return _teamKey;
			}
		}

		public StatLine Stats
		{
			get
			{
				return _playerStats;
			}
		}

		protected CoverageType Coverage
		{
			get
			{
				return _coverageType;
			}
		}

		protected int? Week
		{
			get
			{
				if (Coverage == CoverageType.Week)
					return GetElementAsInt("week");
				else return null;
			}
		}

		protected DateTime? Date
		{
			get
			{
				if (Coverage == CoverageType.Date)
					return GetElementAsDateTime("date");
				else return null;				
			}
		}
	}
}
