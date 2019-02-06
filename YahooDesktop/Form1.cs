using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Linq;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Security.Cryptography;
using System.Threading;

using YahooFantasyAPI;
using YahooFantasyAPI.Extensions;
using SportsDataAccess;

namespace YahooDesktop
{
	public partial class Form1 : Form
	{
		// client configuration
		const string clientID = "dj0yJmk9ODk1MU02TTRETTdmJmQ9WVdrOVZqbFpjMGxaTTJVbWNHbzlNQS0tJnM9Y29uc3VtZXJzZWNyZXQmeD05Zg--";
		const string clientSecret = "63fd438f8585ed83edff55cd453ed77ba7ea7b8f";
		//const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
		const string authorizationEndpoint = "https://api.login.yahoo.com/oauth2/request_auth";
		//const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
		const string tokenEndpoint = "https://api.login.yahoo.com/oauth2/get_token";
		//const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
		//const string userInfoEndpoint = "https://fantasysports.yahooapis.com/fantasy/v2/league/nba.l.431";
		const string userInfoEndpoint = "https://fantasysports.yahooapis.com/fantasy/v2/users;use_login=1/games;game_keys=nba/teams?format=json";

		private YahooAPI _yahoo;

		public Form1()
		{
			InitializeComponent();
		}
		
		// ref http://stackoverflow.com/a/3978040
		public static int GetRandomUnusedPort()
		{
			var listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			//var port = ((IPEndPoint)listener.LocalEndpoint).Port;
			var port = 80;
			listener.Stop();
			return port;
		}
		/// <summary>
		/// Returns URI-safe data with a given input length.
		/// </summary>
		/// <param name="length">Input length (nb. output will be longer)</param>
		/// <returns></returns>
		public static string randomDataBase64url(uint length)
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			byte[] bytes = new byte[length];
			rng.GetBytes(bytes);
			return base64urlencodeNoPadding(bytes);
		}

		/// <summary>
		/// Returns the SHA256 hash of the input string.
		/// </summary>
		/// <param name="inputStirng"></param>
		/// <returns></returns>
		public static byte[] sha256(string inputStirng)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
			SHA256Managed sha256 = new SHA256Managed();
			return sha256.ComputeHash(bytes);
		}

		/// <summary>
		/// Base64url no-padding encodes the given input buffer.
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public static string base64urlencodeNoPadding(byte[] buffer)
		{
			string base64 = Convert.ToBase64String(buffer);

			// Converts base64 to base64url.
			base64 = base64.Replace("+", "-");
			base64 = base64.Replace("/", "_");
			// Strips padding.
			base64 = base64.Replace("=", "");

			return base64;
		}

		private async void btnLoginYahoo_Click(object sender, EventArgs e)
		{
			// Generates state and PKCE values.
			string state = randomDataBase64url(32);
			string code_verifier = randomDataBase64url(32);
			string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
			const string code_challenge_method = "S256";

			// Creates a redirect URI using an available port on the loopback address.
			//string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());
			string redirectURI = string.Format("http://{0}/", IPAddress.Loopback);
			output("redirect URI: " + redirectURI);

			// Creates an HttpListener to listen for requests on that redirect URI.
			var http = new HttpListener();
			http.Prefixes.Add(redirectURI);
			output("Listening..");
			http.Start();

			// Creates the OAuth 2.0 authorization request.
			//string authorizationRequest = string.Format("{0}?response_type=code&scope=openid%20profile&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
			//	authorizationEndpoint,
			//	System.Uri.EscapeDataString(redirectURI),
			//	clientID,
			//	state,
			//	code_challenge,
			//	code_challenge_method);

			string authorizationRequest = string.Format("{0}?response_type=code&redirect_uri={1}&client_id={2}&state={3}",
				authorizationEndpoint,
				System.Uri.EscapeDataString(redirectURI),
				clientID,
				state);

			// Opens request in the browser.
			System.Diagnostics.Process.Start(authorizationRequest);

			// Waits for the OAuth authorization response.
			var context = await http.GetContextAsync();

			// Brings this app back to the foreground.
			this.Activate();

			// Sends an HTTP response to the browser.
			var response = context.Response;
			string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
			var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			response.ContentLength64 = buffer.Length;
			var responseOutput = response.OutputStream;
			Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
			{
				responseOutput.Close();
				http.Stop();
				Console.WriteLine("HTTP server stopped.");
			});

			// Checks for errors.
			if (context.Request.QueryString.Get("error") != null)
			{
				output(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
				return;
			}
			if (context.Request.QueryString.Get("code") == null
				|| context.Request.QueryString.Get("state") == null)
			{
				output("Malformed authorization response. " + context.Request.QueryString);
				return;
			}

			// extracts the code
			var code = context.Request.QueryString.Get("code");
			var incoming_state = context.Request.QueryString.Get("state");

			// Compares the receieved state to the expected value, to ensure that
			// this app made the request which resulted in authorization.
			if (incoming_state != state)
			{
				//output(String.Format("Received request with invalid state ({0})", incoming_state));
				return;
			}
			output("Authorization code: " + code);

			// Starts the code exchange at the Token Endpoint.
			performCodeExchange(code, code_verifier, redirectURI);
		}

		async void performCodeExchange(string code, string code_verifier, string redirectURI)
		{
			output("Exchanging code for tokens...");

			// builds the  request
			//string tokenRequestURI = "https://www.googleapis.com/oauth2/v4/token";
			//string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code",
			//	code,
			//	System.Uri.EscapeDataString(redirectURI),
			//	clientID,
			//	code_verifier,
			//	clientSecret
			//	);

			//string tokenRequestBody = string.Format("client_id={0}&client_secret={1}&redirect_uri={2}&code={3}&grant_type=authorization_code",
			//	clientID,
			//	clientSecret,
			//	System.Uri.EscapeDataString(redirectURI),
			//	code
			//	);

			string tokenRequestBody = string.Format("redirect_uri={0}&code={1}&grant_type=authorization_code",
				System.Uri.EscapeDataString(redirectURI),
				code
				);

			// sends the request
			//HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenRequestURI);
			HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
			tokenRequest.Method = "POST";
			tokenRequest.ContentType = "application/x-www-form-urlencoded";
			//tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
			tokenRequest.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(clientID + ":" + clientSecret)));
			byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
			tokenRequest.ContentLength = _byteVersion.Length;
			Stream stream = tokenRequest.GetRequestStream();
			await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
			stream.Close();

			try
			{
				// gets the response
				WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
				using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
				{
					// reads response body
					string responseText = await reader.ReadToEndAsync();
					output(responseText);

					// converts to dictionary
					Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

					string access_token = tokenEndpointDecoded["access_token"];
					//userinfoCall(access_token);
					_yahoo = new YahooAPI();
					_yahoo.AccessToken = access_token;

					//List<League> leagues = League.GetLeagues(_yahoo, "nba");
					//SportsDataDataContext sportsData = new SportsDataDataContext();
					//foreach(League league in leagues)
					//{
					//	LeagueInfo li = league.CreateLeagueInfo();
					//	sportsData.LeagueInfos.InsertOnSubmit(li);
					//}
					//sportsData.SubmitChanges();

					//List<Game> games = Game.GetGames(yahoo);
					//SportsDataDataContext sportsData = new SportsDataDataContext();
					//foreach (Game game in games)
					//{
					//	GameInfo gi = game.CreateGameInfo();
					//	sportsData.GameInfos.InsertOnSubmit(gi);
					//}
					//sportsData.SubmitChanges();

					//League league = League.GetLeague(yahoo, "375", "105375");
					//List<League> leagues = League.GetLeagues(yahoo, "375");
					//Builder builder = new Builder(_yahoo);
					////builder.AddGameData();
					////builder.AddLeagueData("nba");
					//////builder.AddWeeklyData("385.l.105375", 1);
					////builder.AddWeeklyData("385.l.91256", 1);
					//builder.PopulateEverything("nba");
					return;
					Game nbaGame = Game.GetGame(_yahoo, "nba"); //new Game(yahoo.ExecuteMethod(@"game/nba"));
					List<League> currentNBALeagues = nbaGame.GetLeagues();
					League currentLeague = currentNBALeagues[0];
					List<YahooFantasyAPI.Matchup> matchups = new List<YahooFantasyAPI.Matchup>();
					if ((currentLeague.StartWeek.HasValue) && (currentLeague.CurrentWeek.HasValue))
					{
						for (int i = currentLeague.StartWeek.Value; i < currentLeague.CurrentWeek.Value; i++)
						{
							List<YahooFantasyAPI.Matchup> weeksMatchups = YahooFantasyAPI.Matchup.GetMatchups(_yahoo, currentLeague.LeagueKey, i);
							foreach(YahooFantasyAPI.Matchup mtchup in weeksMatchups)
							{
								List<Player> players = Player.GetPlayers(_yahoo, mtchup.Team1key, mtchup.Week);
								players.AddRange(Player.GetPlayers(_yahoo, mtchup.Team2key, mtchup.Week));
							}
							matchups.AddRange(weeksMatchups);
						}
					}
					//List<YahooFantasyAPI.Matchup> matchups = YahooFantasyAPI.Matchup.GetMatchups(yahoo, currentNBALeagues[0].LeagueKey, 11);
					//Game nbaGame = Game.CreateGame(yahoo.ExecuteMethod(@"game/nba?format=json"));
				}
			}
			catch (WebException ex)
			{
				if (ex.Status == WebExceptionStatus.ProtocolError)
				{
					var response = ex.Response as HttpWebResponse;
					if (response != null)
					{
						output("HTTP: " + response.StatusCode);
						using (StreamReader reader = new StreamReader(response.GetResponseStream()))
						{
							// reads response body
							string responseText = await reader.ReadToEndAsync();
							output(responseText);
						}
					}

				}
			}
		}


		async void userinfoCall(string access_token)
		{
			output("Making API Call to Userinfo...");

			// builds the  request
			string userinfoRequestURI = userInfoEndpoint;

			// sends the request
			HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userinfoRequestURI);
			userinfoRequest.Method = "GET";
			userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
			userinfoRequest.ContentType = "application/x-www-form-urlencoded";
			//userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

			// gets the response
			WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
			using (StreamReader userinfoResponseReader = new StreamReader(userinfoResponse.GetResponseStream()))
			{
				// reads response body
				string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();
				output(userinfoResponseText);
			}
		}

		/// <summary>
		/// Appends the given string to the on-screen log, and the debug console.
		/// </summary>
		/// <param name="output">string to be appended</param>
		public void output(string output)
		{
			textBoxOutput.Text = textBoxOutput.Text + output + Environment.NewLine;
			Console.WriteLine(output);
		}

		private void btnExecute_Click(object sender, EventArgs e)
		{
			try
			{
				XDocument xDoc = _yahoo.ExecuteMethod(txtURL.Text);
				textBoxOutput.Text = xDoc.ToString();
			}
			catch(Exception ex)
			{
				textBoxOutput.Text = ex.Message;
			}
		}

		private void btnClear_Click(object sender, EventArgs e)
		{
			textBoxOutput.Clear();
		}

		private void btnRunBuilder_Click(object sender, EventArgs e)
		{
			Builder builder = new Builder(_yahoo);
			builder.PopulateEverything("nba");
		}

		private void btnCalc_Click(object sender, EventArgs e)
		{
			Calculator calc = new Calculator();
			Dictionary<TeamInfo, StatLine> totalStats = calc.GetTotalYearStats(calc.GetCurrentNBA());
			calc.GetAvgYearStats(calc.GetCurrentNBA());
		}
	}
}
