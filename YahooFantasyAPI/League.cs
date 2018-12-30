using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public class League : YahooObjectBase
	{
		private string _leagueKey = null;
		private string _leagueID = null;
		private string _name = null;
		private string _url = null;
		private bool? _isFinished = null;
		private int? _currentWeek = null;
		private int? _startWeek = null;
		private int? _endWeek = null;

		public League(YahooAPI yahoo, XElement xml) : base(yahoo, xml)
		{
		}

		public static League GetLeague(YahooAPI yahoo, string gameKey, string leagueID)
		{
			return League.GetLeague(yahoo, gameKey + ".l." + leagueID);
		}
		public static League GetLeague(YahooAPI yahoo, string leagueKey)
		{
			League newLeague = null;
			XDocument xDoc = yahoo.ExecuteMethod(@"league/" + leagueKey);
			List<XElement> descendantXml = xDoc.Descendants(_yns + "league").ToList();
			if(descendantXml.Count > 0)
			{
				newLeague = new League(yahoo, descendantXml[0]);
			}
			return newLeague;
		}
		public static List<League> GetLeagues(YahooAPI yahoo, string gameKey)
		{
			List<League> leagues = new List<League>();
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"users;use_login=1/games;game_keys={0}/leagues", gameKey));
			foreach(XElement descendantXml in xDoc.Descendants(_yns + "league"))
			{
				leagues.Add(new League(yahoo, descendantXml));
			}
			return leagues;
		}

		public string LeagueKey
		{
			get
			{
				if (_leagueKey == null)
				{
					_leagueKey = GetElementAsString("league_key");
				}
				return _leagueKey;
			}
		}

		public string LeagueID
		{
			get
			{
				if (_leagueID == null)
				{
					_leagueID = GetElementAsString("league_id");
				}
				return _leagueID;
			}
		}

		public string Name
		{
			get
			{
				if (_name == null)
				{
					_name = GetElementAsString("name");
				}
				return _name;
			}
		}

		public string Url
		{
			get
			{
				if (_url == null)
				{
					_url = GetElementAsString("url");
				}
				return _url;
			}
		}

		public bool? IsFinished
		{
			get
			{
				if (_isFinished == null)
				{
					_isFinished = GetElementAsBool("is_finished");
				}
				return _isFinished;
			}
		}

		public int? CurrentWeek
		{
			get
			{
				if (_currentWeek == null)
				{
					_currentWeek = GetElementAsInt("current_week");
				}
				return _currentWeek;
			}
		}

		public int? StartWeek
		{
			get
			{
				if (_startWeek == null)
				{
					_startWeek = GetElementAsInt("start_week");
				}
				return _startWeek;
			}
		}

		public int? EndWeek
		{
			get
			{
				if (_endWeek == null)
				{
					_endWeek = GetElementAsInt("end_week");
				}
				return _endWeek;
			}
		}
	}
}
