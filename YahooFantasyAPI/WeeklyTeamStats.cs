using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public class WeeklyTeamStats : YahooObjectBase
	{
		private int _week;
		private string _teamkey;
		private StatLine _teamStats = null;
		private int _gamesPlayed;
		private int _gamesMissed;
		private decimal _ptsWin = 0;
		private decimal _rebsWin = 0;
		private decimal _asstsWin = 0;
		private decimal _stlsWin = 0;
		private decimal _blksWin = 0;

		public WeeklyTeamStats(YahooAPI yahoo, XElement xml, string teamKey) : base(yahoo, xml)
		{
			// Assumes root node is <matchup> and a team_key be passed in for the teams data to be extracted from the matchup
			_teamkey = teamKey;
			int? week = GetElementAsInt("week");
			if (week == null)
				throw new Exception("Could not determine what week matchup is for.");
			_week = week.Value;

			bool teamDataFound = false;
			foreach(XElement teamXml in GetDescendants("team"))
			{
				if(GetElementAsString(teamXml, "team_key").Equals(teamKey))
				{
					teamDataFound = true;
					XElement teamStats = GetElement(teamXml, "team_stats");
					_teamStats = new StatLine(yahoo, teamStats);
				}
			}
			if (!teamDataFound)
				throw new Exception("Team data not found in matchup for team key provided (" + teamKey + ").");

			foreach(XElement statWinnerXml in GetDescendants("stat_winner"))
			{
				string statID = GetElementAsString(statWinnerXml, "stat_id");
				string winningTeam = GetElementAsString(statWinnerXml, "winner_team_key");
				bool? isTied = GetElementAsBool(statWinnerXml, "is_tied");

				decimal winTotal = 0;
				if(isTied.HasValue && isTied.Value)
				{
					winTotal = 0.5M;
				}
				else if(winningTeam.Equals(teamKey))
				{
					winTotal = 1;
				}
				switch (statID)
				{
					case StatLine.Pts_Stat_ID:
						_ptsWin = winTotal;
						break;
					case StatLine.Rebs_Stat_ID:
						_rebsWin = winTotal;
						break;
					case StatLine.Asts_Stat_ID:
						_asstsWin = winTotal;
						break;
					case StatLine.Stls_Stat_ID:
						_stlsWin = winTotal;
						break;
					case StatLine.Blks_Stat_ID:
						_blksWin = winTotal;
						break;
				}
			}
		}

		public static List<WeeklyTeamStats> GetWeeklyTeamStats(YahooAPI yahoo, string leagueKey, int week)
		{
			List<WeeklyTeamStats> teamStats = new List<WeeklyTeamStats>();
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"league/{0}/scoreboard;week={1}", leagueKey, week));
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "matchup"))
			{
				foreach (XElement teamXml in descendantXml.Descendants(_yns + "team"))
				{
					XElement teamKeyXml = teamXml.Element(_yns + "team_key");
					if (teamKeyXml == null)
						throw new Exception("Couldn't find team_key in matchup");
					string teamKey = teamKeyXml.Value;
					teamStats.Add(new WeeklyTeamStats(yahoo, descendantXml, teamKey));
				}
			}
			return teamStats;
		}

		public int Week
		{
			get
			{
				return _week;
			}
		}

		public string Teamkey
		{
			get
			{
				return _teamkey;
			}
		}

		public StatLine TeamStats
		{
			get
			{
				return _teamStats;
			}
		}

		public int GamesPlayed
		{
			get
			{
				return _gamesPlayed;
			}
		}

		public int GamesMissed
		{
			get
			{
				return _gamesMissed;
			}
		}

		public decimal PtsWin
		{
			get
			{
				return _ptsWin;
			}
		}

		public decimal RebsWin
		{
			get
			{
				return _rebsWin;
			}
		}

		public decimal AsstsWin
		{
			get
			{
				return _asstsWin;
			}
		}

		public decimal StlsWin
		{
			get
			{
				return _stlsWin;
			}
		}

		public decimal BlksWin
		{
			get
			{
				return _blksWin;
			}
		}
	}
}
