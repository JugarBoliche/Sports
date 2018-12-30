using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace YahooFantasyAPI
{
	public class YahooObjectBase
	{
		//private string _xml = "";
		protected static XNamespace _yns = "http://fantasysports.yahooapis.com/fantasy/v2/base.rng";
		private XElement _xml;
		private YahooAPI _yahoo;
		public YahooObjectBase(YahooAPI yahoo, XElement xml)
		{
			Xml = xml;
			Yahoo = yahoo;
		}

		protected XElement Xml
		{
			get
			{
				return _xml;
			}
			private set
			{
				_xml = value;
			}
		}

		protected XNamespace YahooNS
		{
			get
			{
				return _yns;
			}
		}

		protected YahooAPI Yahoo
		{
			get
			{
				return _yahoo;
			}
			private set
			{
				_yahoo = value;
			}
		}

		//protected XElement GetElement(string xPath)
		//{
		//	XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
		//	nsManager.AddNamespace("y", Xml.GetDefaultNamespace().NamespaceName);
		//	string newXpath = AddNameSpace(xPath, "y");
		//	return Xml.XPathSelectElement(newXpath, nsManager);
		//}

		//private string AddNameSpace(string xPath, string ns)
		//{
		//	string[] pieces = xPath.Split(new char[] { '/' }, StringSplitOptions.None);
		//	for (int i = 0; i < pieces.Length; i++)
		//	{
		//		string piece = pieces[i];
		//		if (!string.IsNullOrEmpty(piece))
		//		{
		//			pieces[i] = ns + ":" + piece;
		//		}
		//	}
		//	return string.Join("/", pieces);
		//}

		protected XElement GetElement(string nodeName)
		{
			return GetElement(Xml, nodeName);
		}
		protected XElement GetElement(XElement xml, string nodeName)
		{
			return xml.Element(YahooNS + nodeName);
		}

		protected string GetElementAsString(string nodeName)
		{
			return GetElementAsString(Xml, nodeName);
		}
		protected string GetElementAsString(XElement xml, string nodeName)
		{
			string retVal = string.Empty;
			XElement element = xml.Element(YahooNS + nodeName);
			if (element != null)
			{
				retVal = element.Value;
			}
			return retVal;
		}

		protected bool? GetElementAsBool(string nodeName)
		{
			return GetElementAsBool(Xml, nodeName);
		}

		protected bool? GetElementAsBool(XElement xml, string nodeName)
		{
			bool? retVal = null;
			XElement element = xml.Element(YahooNS + nodeName);
			if ((element != null) && (element.Value != null))
			{
				if (element.Value.Equals("0"))
				{
					retVal = false;
				}
				else if (element.Value.Equals("1"))
				{
					retVal = true;
				}
				else
				{
					bool tempBool;
					if (bool.TryParse(element.Value, out tempBool))
					{
						retVal = tempBool;
					}
				}
			}
			return retVal;
		}

		protected int? GetElementAsInt(string nodeName)
		{
			return GetElementAsInt(Xml, nodeName);
		}

		protected int? GetElementAsInt(XElement xml, string nodeName)
		{
			int? retVal = null;
			XElement element = xml.Element(YahooNS + nodeName);
			if ((element != null) && (element.Value != null))
			{
				int tempInt;
				if (int.TryParse(element.Value, out tempInt))
				{
					retVal = tempInt;
				}
			}
			return retVal;
		}

		protected DateTime? GetElementAsDateTime(string nodeName)
		{
			return GetElementAsDateTime(Xml, nodeName);
		}

		protected DateTime? GetElementAsDateTime(XElement xml, string nodeName)
		{
			return GetElementAsDateTime(xml, nodeName, "yyyy-MM-dd");
		}

		protected DateTime? GetElementAsDateTime(XElement xml, string nodeName, string format)
		{
			DateTime? retVal = null;
			XElement element = xml.Element(YahooNS + nodeName);
			if ((element != null) && (element.Value != null))
			{
				
				DateTime tempDateTime;
				if (DateTime.TryParseExact(element.Value, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out tempDateTime))
				{
					retVal = tempDateTime;
				}
			}
			return retVal;
		}

		protected IEnumerable<XElement> GetDescendants(string nodeName)
		{
			return GetDescendants(Xml, nodeName);
		}

		protected IEnumerable<XElement> GetDescendants(XElement xml, string nodeName)
		{
			return xml.Descendants(YahooNS + nodeName);
		}

		//public static T CreateYahooObject<T>(string json) where T : YahooObjectBase
		//{ 
		//	Dictionary<string, string> something = JsonConvert.DeserializeAnonymousType<.DeserializeObject<Dictionary<string, string>>(json);
		//	T retVal = JsonConvert.DeserializeObject<T>(json);
		//	retVal.Json = json;
		//	return retVal;
		//}
	}
}
