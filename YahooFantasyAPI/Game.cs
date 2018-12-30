using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public class Game : YahooObjectBase
	{
		private string _gameKey = null;
		private string _gameID = null;
		private string _name = null;
		private string _code = null;
		private string _type = null;
		private string _url = null;
		private string _season = null;
		private bool? _isRegistrationOver = null;
		private bool? _isGameOver = null;
		private bool? _isOffseason = null;

		public Game(YahooAPI yahoo, XElement xml) : base(yahoo, xml)
		{
		}

		public static Game GetGame(YahooAPI yahoo, string game)
		{
			Game newGame = null;
			XDocument xDoc = yahoo.ExecuteMethod(@"game/" + game);
			List<XElement> descendantXml = xDoc.Descendants(_yns + "game").ToList();
			if (descendantXml.Count > 0)
			{
				newGame = new Game(yahoo, descendantXml[0]);
			}
			return newGame;
		}

		public static List<Game> GetGames(YahooAPI yahoo)
		{
			return GetGames(yahoo, true);
		}

		public static List<Game> GetGames(YahooAPI yahoo, bool useLoggedInUser)
		{
			List<Game> games = new List<Game>();
			string uri = @"games;game_types=full";
			if(useLoggedInUser)
			{
				uri = @"users;use_login=1/games";
			}
			XDocument xDoc = yahoo.ExecuteMethod(uri);
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "game"))
			{
				games.Add(new Game(yahoo, descendantXml));
			}
			return games;
		}

		public List<League> GetLeagues()
		{
			return League.GetLeagues(Yahoo, GameKey);
		}

		public string GameKey
		{
			get
			{
				if(_gameKey == null)
				{
					_gameKey = GetElementAsString("game_key");
				}
				return _gameKey;
			}
		}

		public string GameID
		{
			get
			{
				if (_gameID == null)
				{
					_gameID = GetElementAsString("game_id");
				}
				return _gameID;
			}
		}

		public string Name
		{
			get
			{
				if(_name == null)
				{
					_name = GetElementAsString("name");
				}
				return _name;
			}
		}

		public string Code
		{
			get
			{
				if(_code == null)
				{
					_code = GetElementAsString("code");
				}
				return _code;
			}
		}

		public string Type
		{
			get
			{
				if(_type == null)
				{
					_type = GetElementAsString("type");
				}
				return _type;
			}
		}

		public string Url
		{
			get
			{
				if(_url == null)
				{
					_url = GetElementAsString("url");
				}
				return _url;
			}
		}

		public string Season
		{
			get
			{
				if(_season == null)
				{
					_season = GetElementAsString("season");
				}
				return _season;
			}
		}

		public bool? IsRegistrationOver
		{
			get
			{
				if (_isRegistrationOver == null)
				{
					_isRegistrationOver = GetElementAsBool("is_registration_over");
				}
				return _isRegistrationOver;
			}
		}

		public bool? IsGameOver
		{
			get
			{
				if (_isGameOver == null)
				{
					_isGameOver = GetElementAsBool("is_game_over");
				}
				return _isGameOver;
			}
		}

		public bool? IsOffseason
		{
			get
			{
				if (_isOffseason == null)
				{
					_isOffseason = GetElementAsBool("is_offseason");
				}
				return _isOffseason;
			}
		}
	}
}
