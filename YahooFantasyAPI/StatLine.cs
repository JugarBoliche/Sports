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

		public StatLine(YahooAPI yahoo, XElement xml) : base(yahoo, xml)
		{
			foreach(XElement descendantXml in GetDescendants(xml, "stat"))
			{
				string stat_id = GetElementAsString(descendantXml, "stat_id");
				//string stat_string_value = GetElementAsString(descendantXml, "value");
				int? stat_value = GetElementAsInt(descendantXml, "value");

				if((!string.IsNullOrEmpty(stat_id)) && (stat_value.HasValue))
				{
					switch (stat_id)
					{
						case Pts_Stat_ID:
							Points = stat_value.Value;
							break;
						case Rebs_Stat_ID:
							Rebounds = stat_value.Value;
							break;
						case Asts_Stat_ID:
							Assists = stat_value.Value;
							break;
						case Stls_Stat_ID:
							Steals = stat_value.Value;
							break;
						case Blks_Stat_ID:
							Blocks = stat_value.Value;
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

		public int? Points { get; set; }
		public int? Rebounds { get; set; }
		public int? Assists { get; set; }
		public int? Steals { get; set; }
		public int? Blocks { get; set; }

		public bool StatsNotNull
		{
			get
			{
				return Points.HasValue && Rebounds.HasValue && Assists.HasValue && Steals.HasValue && Blocks.HasValue;
			}
		}
	}
}
