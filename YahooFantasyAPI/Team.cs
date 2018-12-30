using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public class Team : YahooObjectBase
	{
		private string _teamKey = null;
		private string _teamID = null;
		private string _name = null;
		private string _url = null;
		private string _managerID = null;
		private string _managerName = null;

		public Team(YahooAPI yahoo, XElement xml) : base(yahoo, xml)
		{
		}

		public static Team GetTeam(YahooAPI yahoo, string leagueKey, string teamID)
		{
			return Team.GetTeam(yahoo, leagueKey + ".t." + teamID);
		}
		public static Team GetTeam(YahooAPI yahoo, string teamKey)
		{
			Team newTeam = null;
			XDocument xDoc = yahoo.ExecuteMethod(@"team/" + teamKey);
			List<XElement> descendantXml = xDoc.Descendants(_yns + "team").ToList();
			if (descendantXml.Count > 0)
			{
				newTeam = new Team(yahoo, descendantXml[0]);
			}
			return newTeam;
		}
		public static List<Team> GetTeams(YahooAPI yahoo, string leagueKey)
		{
			List<Team> teams = new List<Team>();
			XDocument xDoc = yahoo.ExecuteMethod(string.Format(@"league/{0}/teams", leagueKey));
			foreach (XElement descendantXml in xDoc.Descendants(_yns + "team"))
			{
				teams.Add(new Team(yahoo, descendantXml));
			}
			return teams;
		}

		public string TeamKey
		{
			get
			{
				if (_teamKey == null)
				{
					_teamKey = GetElementAsString("team_key");
				}
				return _teamKey;
			}
		}

		public string TeamID
		{
			get
			{
				if (_teamID == null)
				{
					_teamID = GetElementAsString("team_id");
				}
				return _teamID;
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

		public string ManagerName
		{
			get
			{
				if (_managerName == null)
				{
					XElement managers = GetElement("managers");
					if((managers != null) && (managers.HasElements))
					{
						XElement manager = GetElement(managers, "manager");
						if((manager != null) && (manager.HasElements))
						{
							_managerName = GetElementAsString(manager, "nickname");
						}
					}
					
				}
				return _managerName;
			}
		}

		public string ManagerID
		{
			get
			{
				if (_managerID == null)
				{
					XElement managers = GetElement("managers");
					if ((managers != null) && (managers.HasElements))
					{
						XElement manager = GetElement(managers, "manager");
						if ((manager != null) && (manager.HasElements))
						{
							_managerID = GetElementAsString(manager, "manager_id");
						}
					}

				}
				return _managerID;
			}
		}
	}
}
