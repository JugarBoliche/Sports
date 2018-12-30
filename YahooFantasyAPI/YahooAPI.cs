using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace YahooFantasyAPI
{
	public class YahooAPI
	{
		public string BaseUri
		{
			get
			{
				return @"https://fantasysports.yahooapis.com/fantasy/v2/";
			}
		}
		public string AccessToken { get; set; }
		public XDocument ExecuteMethod(string ApiUrl)
		{
			XDocument xDoc = null;
			HttpWebRequest apiRequest = createHttpWebRequest(BaseUri + ApiUrl);

			// gets the response
			WebResponse apiResponse = apiRequest.GetResponse();
			using (StreamReader userinfoResponseReader = new StreamReader(apiResponse.GetResponseStream()))
			{
				// reads response body
				xDoc = XDocument.Parse(userinfoResponseReader.ReadToEnd());
			}
			return xDoc;
		}

		//async public Task<string> ExecuteMethodAsync(string ApiUrl)
		//{
		//	string retVal = "";
		//	HttpWebRequest apiRequest = createHttpWebRequest(ApiUrl);

		//	// gets the response
		//	WebResponse apiResponse = await apiRequest.GetResponseAsync();
		//	using (StreamReader userinfoResponseReader = new StreamReader(apiResponse.GetResponseStream()))
		//	{
		//		// reads response body
		//		retVal = await userinfoResponseReader.ReadToEndAsync();
		//	}
		//	return retVal;
		//}


		private HttpWebRequest createHttpWebRequest(string Uri)
		{
			HttpWebRequest apiRequest = (HttpWebRequest)WebRequest.Create(Uri);
			apiRequest.Method = "GET";
			apiRequest.Headers.Add(string.Format("Authorization: Bearer {0}", AccessToken));
			apiRequest.ContentType = "application/x-www-form-urlencoded";
			return apiRequest;
		}
	}
}
