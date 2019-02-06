using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace YahooFantasyAPI
{
	[DebuggerDisplay("Points = {Points}, Rebounds = {Rebounds}, Assists = {Assists}, Steals = {Steals}, Blocks = {Blocks}")]
	public class StatLine : YahooObjectBase
	{
		public const string Pts_Stat_ID = "12";
		public const string Rebs_Stat_ID = "15";
		public const string Asts_Stat_ID = "16";
		public const string Stls_Stat_ID = "17";
		public const string Blks_Stat_ID = "18";

		Dictionary<string, int?> _allStats = new Dictionary<string, int?>();

		public StatLine(YahooAPI yahoo, XElement xml) : base(yahoo, xml)
		{
			foreach(XElement descendantXml in GetDescendants(xml, "stat"))
			{
				string stat_id = GetElementAsString(descendantXml, "stat_id");
				//string stat_string_value = GetElementAsString(descendantXml, "value");
				int? stat_value = GetElementAsInt(descendantXml, "value");

				if(!string.IsNullOrEmpty(stat_id))
				{
					switch (stat_id)
					{
						case Pts_Stat_ID:
						case Rebs_Stat_ID:
						case Asts_Stat_ID:
						case Stls_Stat_ID:
						case Blks_Stat_ID:
							_allStats.Add(stat_id, stat_value);
							break;
					}
				}
					
			}
		}

		public StatLine(int pts, int rebs, int asts, int stls, int blks) : base(null, null)
		{
			Points = pts;
			Rebounds = rebs;
			Assists = asts;
			Steals = stls;
			Blocks = blks;
		}

		public int? GetStatValue(string yahooStatId)
		{
			switch(yahooStatId)
			{
				case Pts_Stat_ID:
					return Points;
				case Rebs_Stat_ID:
					return Rebounds;
				case Asts_Stat_ID:
					return Assists;
				case Stls_Stat_ID:
					return Steals;
				case Blks_Stat_ID:
					return Blocks;
				default:
					return null;
			}
		}

		public int? Points
		{
			get
			{
				return _allStats[Pts_Stat_ID];
			}
			set
			{
				_allStats[Pts_Stat_ID] = value;
			}
		}

		public int? Rebounds
		{
			get
			{
				return _allStats[Rebs_Stat_ID];
			}
			set
			{
				_allStats[Rebs_Stat_ID] = value;
			}
		}

		public int? Assists
		{
			get
			{
				return _allStats[Asts_Stat_ID];
			}
			set
			{
				_allStats[Asts_Stat_ID] = value;
			}
		}

		public int? Steals
		{
			get
			{
				return _allStats[Stls_Stat_ID];
			}
			set
			{
				_allStats[Stls_Stat_ID] = value;
			}
		}

		public int? Blocks
		{
			get
			{
				return _allStats[Blks_Stat_ID];
			}
			set
			{
				_allStats[Blks_Stat_ID] = value;
			}
		}

		public bool StatsNotNull
		{
			get
			{
				return Points.HasValue && Rebounds.HasValue && Assists.HasValue && Steals.HasValue && Blocks.HasValue;
			}
		}

		public Dictionary<string, int?> AllStats
		{
			get
			{
				return _allStats;
			}
		}

		//public bool StatsNotZero
		//{
		//	get
		//	{
		//		bool retVal = false;
		//		if(StatsNotNull)
		//		{
		//			retVal = Points.Value + Rebounds.Value + Assists.Value + Steals.Value + Blocks.Value > 0;
		//		}
		//		return retVal;
		//	}
		//}
	}
}
