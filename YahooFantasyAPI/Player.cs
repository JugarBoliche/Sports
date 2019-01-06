using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	[DebuggerDisplay("{FirstName} {LastName}")]
	public class Player : YahooObjectBase
	{
		public Player(YahooAPI yahoo, XElement xml) : base(yahoo, xml)
		{
		}

		public static List<Player> GetPlayers(YahooAPI yahoo, string teamKey, int week)
		{
			List<Player> players = new List<Player>();
			//			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/players/stats;type=week;week={1}", teamKey, week));
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/roster;week={1}", teamKey, week));
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "player"))
			{
				players.Add(new Player(yahoo, descendantXml));
			}
			return players;
		}

		public string PlayerKey
		{
			get
			{
				return GetElementAsString("player_key");
			}
		}

		public string PlayerID
		{
			get
			{
				return GetElementAsString("player_id");
			}
		}

		public string FirstName
		{
			get
			{
				return GetElementAsString(GetElement("name"), "first");
			}
		}

		public string LastName
		{
			get
			{
				return GetElementAsString(GetElement("name"), "last");
			}
		}

		public string SelectedPosition
		{
			get
			{
				return GetElementAsString(GetElement("selected_position"), "position");
			}
		}

		public bool IsStarting
		{
			get
			{
				return SelectedPosition.Equals("G", StringComparison.CurrentCultureIgnoreCase) || SelectedPosition.Equals("F", StringComparison.CurrentCultureIgnoreCase) || SelectedPosition.Equals("C", StringComparison.CurrentCultureIgnoreCase);
			}
		}
	}
}
