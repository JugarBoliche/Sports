using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public class Week : YahooObjectBase
	{
		private int _week;
		private string _leagueKey;
		private string _team1key;
		private string _team2key;
		private bool? _isPlayoff = null;
		private bool? _isConsolation = null;
		private DateTime? _startDate = null;
		private DateTime? _endDate = null;
		private StatLine _team1Stats = null;
		private StatLine _team2Stats = null;

		public Matchup(YahooAPI yahoo, XElement xml) : base(yahoo, xml)
		{
			int? week = GetElementAsInt("week");
			if(week == null)
				throw new Exception("Could not determine what week matchup is for.");

			_week = week.Value;

			List<XElement> teamElements = GetDescendants(xml, "team").ToList();
			if (teamElements.Count != 2)
				throw new Exception("Stats don't represent a head to head matchup.");

			_team1key = GetElementAsString(teamElements[0], "team_key");
			_team1Stats = new StatLine(yahoo, teamElements[0]);

			_team2key = GetElementAsString(teamElements[1], "team_key");
			_team2Stats = new StatLine(yahoo, teamElements[1]);
		}

		public static List<Matchup> GetMatchups(YahooAPI yahoo, string leagueKey, int week)
		{
			List<Matchup> matchups = new List<Matchup>();
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"league/{0}/scoreboard;week={1}", leagueKey, week));
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "matchup"))
			{
				matchups.Add(new Matchup(yahoo, descendantXml));
			}
			return matchups;
		}

		public static List<Matchup> GetMatchups(YahooAPI yahoo, string teamKey)
		{
			List<Matchup> matchups = new List<Matchup>();
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/matchups", teamKey));
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "matchup"))
			{
				matchups.Add(new Matchup(yahoo, descendantXml));
			}
			return matchups;
		}


		public int Week
		{
			get
			{
				return _week;
			}
		}

		public string LeagueKey
		{
			get
			{
				if (_leagueKey == null)
				{
					string[] teamKeyPieces = Team1key.Split('.'); // {game_key}.l.{league_id}.t.{team_id}
					_leagueKey = string.Join(".", teamKeyPieces[0], teamKeyPieces[1], teamKeyPieces[2]);
				}
				return _leagueKey;
			}
		}

		public DateTime? StartDate
		{
			get
			{
				if (_startDate == null)
				{
					_startDate = GetElementAsDateTime("week_start");
				}
				return _startDate;
			}
		}

		public DateTime? EndDate
		{
			get
			{
				if (_endDate == null)
				{
					_endDate = GetElementAsDateTime("week_end");
				}
				return _endDate;
			}
		}

		public string Team1key
		{
			get
			{
				return _team1key;
			}
		}

		public string Team2key
		{
			get
			{
				return _team2key;
			}
		}

		public StatLine Team1Stats
		{
			get
			{
				return _team1Stats;
			}
		}

		public StatLine Team2Stats
		{
			get
			{
				return _team2Stats;
			}
		}

		public bool? IsPlayoffs
		{
			get
			{
				if(_isPlayoff == null)
				{
					_isPlayoff = GetElementAsBool("is_playoffs");
				}
				return _isPlayoff;
			}
		}

		public bool? IsConsolation
		{
			get
			{
				if(_isConsolation == null)
				{
					_isConsolation = GetElementAsBool("is_consolation");
				}
				return _isConsolation;
			}
		}
	}
}
