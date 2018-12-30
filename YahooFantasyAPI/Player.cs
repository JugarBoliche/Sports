using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public class Player : YahooObjectBase
	{
		private string _playerKey = null;
		private string _playerID = null;
		private string _firstName = null;
		private string _lastName = null;
		private string _url = null;
		private string _manager = null;

		public Player(YahooAPI yahoo, XElement xml) : base(yahoo, xml)
		{
		}
		public static Player GetPlayer(YahooAPI yahoo, string playerKey, DateTime date)
		{
			Player newPlayer = null;
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"player/{0}/stats;date={1}", playerKey, date.ToString("yyyy-MM-dd")));
			List<XElement> descendantXml = xDoc.Descendants(_yns + "player").ToList();
			if (descendantXml.Count > 0)
			{
				newPlayer = new Player(yahoo, descendantXml[0]);
			}
			return newPlayer;
		}
		public static List<Player> GetPlayers(YahooAPI yahoo, string teamKey, int week)
		{
			List<Player> players = new List<Player>();
			//			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/players/stats;type=week;week={1}", teamKey, week));
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"team/{0}/roster;week={1}/players/stats;type=week;week={1}", teamKey, week));
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "player"))
			{
				Player player = new Player(yahoo, descendantXml);
				players.Add(player);
				Player repeatPlayer1 = Player.GetPlayer(yahoo, player._playerKey, new DateTime(2017, 10, 17));
				Player repeatPlayer2 = Player.GetPlayer(yahoo, player._playerKey, new DateTime(2017, 10, 18));
				Player repeatPlayer3 = Player.GetPlayer(yahoo, player._playerKey, new DateTime(2017, 10, 19));
				Player repeatPlayer4 = Player.GetPlayer(yahoo, player._playerKey, new DateTime(2017, 10, 20));
				Player repeatPlayer5 = Player.GetPlayer(yahoo, player._playerKey, new DateTime(2017, 10, 21));
				Player repeatPlayer6 = Player.GetPlayer(yahoo, player._playerKey, new DateTime(2017, 10, 22));
			}
			return players;
		}
	}
}
