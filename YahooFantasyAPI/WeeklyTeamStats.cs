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
		private Dictionary<string, WinTieInfo> _statsWinTieInfo = new Dictionary<string, WinTieInfo>();
		//private bool? _ptsWin;
		//private bool? _rebsWin;
		//private bool? _asstsWin;
		//private bool? _stlsWin;
		//private bool? _blksWin;
		//private bool? _ptsTie;
		//private bool? _rebsTie;
		//private bool? _asstsTie;
		//private bool? _stlsTie;
		//private bool? _blksTie;

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

				bool isWin = false;
				bool isTie = false;
				if(isTied.HasValue && isTied.Value)
				{
					isTie = true;
				}
				else if(winningTeam.Equals(teamKey))
				{
					isWin = true;
				}

				_statsWinTieInfo.Add(statID, new WinTieInfo(isWin, isTie));
				
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

		public Dictionary<string, WinTieInfo> StatsWinTieInfo
		{
			get
			{
				return _statsWinTieInfo;
			}
		}
	}
}
