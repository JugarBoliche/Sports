using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

//namespace YahooFantasyAPI
//{
//	public class Week : YahooObjectBase
//	{
//		private int _week;
//		private string _leagueKey;
//		private bool? _isPlayoff = null;
//		private bool? _isConsolation = null;
//		private DateTime? _startDate = null;
//		private DateTime? _endDate = null;

//		public Week(YahooAPI yahoo, XElement xml) : base(yahoo, xml)
//		{
//			int? week = GetElementAsInt("week");
//			if(week == null)
//				throw new Exception("Could not determine what week matchup is for.");

//			_week = week.Value;
//		}

//		public static List<Week> GetWeeks(YahooAPI yahoo, string leagueKey)
//		{
//			List<Week> weeks = new List<Week>();
//			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"league/{0}/scoreboard", leagueKey));
//			foreach (XElement descendantXml in xDoc.Descendants(_yns + "matchup"))
//			{
//				Week week = new Week(yahoo, descendantXml);
//				if (!weeks.Any(w => w.WeekNum == week.WeekNum))
//				{
//					weeks.Add(week);
//				}
//			}
//			return weeks;
//		}

//		public static List<Matchup> GetMatchups(YahooAPI yahoo, string teamKey)
//		{
//			List<Matchup> matchups = new List<Matchup>();
//			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/matchups", teamKey));
//			foreach (XElement descendantXml in xDoc.Descendants(_yns + "matchup"))
//			{
//				matchups.Add(new Matchup(yahoo, descendantXml));
//			}
//			return matchups;
//		}


//		public int WeekNum
//		{
//			get
//			{
//				return _week;
//			}
//		}

//		public string LeagueKey
//		{
//			get
//			{
//				if (_leagueKey == null)
//				{
//					string[] teamKeyPieces = Team1key.Split('.'); // {game_key}.l.{league_id}.t.{team_id}
//					_leagueKey = string.Join(".", teamKeyPieces[0], teamKeyPieces[1], teamKeyPieces[2]);
//				}
//				return _leagueKey;
//			}
//		}

//		public DateTime? StartDate
//		{
//			get
//			{
//				if (_startDate == null)
//				{
//					_startDate = GetElementAsDateTime("week_start");
//				}
//				return _startDate;
//			}
//		}

//		public DateTime? EndDate
//		{
//			get
//			{
//				if (_endDate == null)
//				{
//					_endDate = GetElementAsDateTime("week_end");
//				}
//				return _endDate;
//			}
//		}

//		public bool? IsPlayoffs
//		{
//			get
//			{
//				if(_isPlayoff == null)
//				{
//					_isPlayoff = GetElementAsBool("is_playoffs");
//				}
//				return _isPlayoff;
//			}
//		}

//		public bool? IsConsolation
//		{
//			get
//			{
//				if(_isConsolation == null)
//				{
//					_isConsolation = GetElementAsBool("is_consolation");
//				}
//				return _isConsolation;
//			}
//		}
//	}
//}
