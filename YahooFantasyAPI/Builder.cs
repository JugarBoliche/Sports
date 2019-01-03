using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SportsDataAccess;
using YahooFantasyAPI.Extensions;

namespace YahooFantasyAPI
{
	public class Builder
	{
		private YahooAPI _yahoo = null;
		SportsDataDataContext _sportsData;

		public Builder(YahooAPI yahoo)
		{
			_yahoo = yahoo;
			_sportsData = new SportsDataDataContext();
		}

		public void PopulateEverything(string game)
		{
			if(!_sportsData.GameInfos.Any(gi => gi.game_key == game))
			{
				AddGameData(game);
			}
			List<LeagueInfo> leagues = AddLeagueData(game);
			foreach(LeagueInfo li in leagues)
			{
				IEnumerable<WeekInfo> weeks = li.WeekInfos.Where(wi => (wi.startDate < DateTime.Now.Date && !wi.lastLoadDate.HasValue) || (wi.lastLoadDate.HasValue && wi.lastLoadDate.Value < wi.endDate.AddDays(2)));
				foreach (WeekInfo week in weeks)
				{
					AddWeeklyTeamData(li.league_key, week.week);
					week.lastLoadDate = DateTime.Now;
					_sportsData.SubmitChanges();
				}
			}
			
		}

		public void AddGameData()
		{
			List<Game> games = Game.GetGames(_yahoo);
			foreach (Game game in games)
			{
				addGameInfo(game);
			}
		}

		public void AddGameData(string gameKey)
		{
			Game game = Game.GetGame(_yahoo, gameKey);
			addGameInfo(game);
		}

		private void addGameInfo(Game game)
		{
			GameInfo gi = game.CreateGameInfo();
			if (!_sportsData.GameInfos.Any(g => g.game_key == gi.game_key))
			{
				_sportsData.GameInfos.InsertOnSubmit(gi);
				_sportsData.SubmitChanges();
			}
		}

		public List<LeagueInfo> AddLeagueData(string gameKey)
		{
			List<LeagueInfo> leagueInfos = new List<LeagueInfo>();

			List<League> leagues = League.GetLeagues(_yahoo, gameKey);
			foreach (League league in leagues)
			{
				LeagueInfo li = _sportsData.LeagueInfos.SingleOrDefault(l => l.league_key == league.LeagueKey);
				if (li == null)
				{
					li = league.CreateLeagueInfo();
					_sportsData.LeagueInfos.InsertOnSubmit(li);
					_sportsData.SubmitChanges();
				}
				leagueInfos.Add(li);

				if(li.TeamInfos.Count == 0)
				{
					List<Team> teams = Team.GetTeams(_yahoo, li.league_key);
					foreach (Team team in teams)
					{
						TeamInfo ti = team.CreateTeamInfo();
						if (!_sportsData.TeamInfos.Any(t => t.team_key == ti.team_key))
						{
							_sportsData.TeamInfos.InsertOnSubmit(ti);
						}
					}
					_sportsData.SubmitChanges();
				}
				
				foreach (TeamInfo ti in li.TeamInfos)
				{
					if (li.WeekInfos.Count == 0 || (ti.MatchupInfos.Count + ti.MatchupInfos1.Count != li.WeekInfos.Count))
					{
						// Need to make sure to loop through all teams prior to this and make sure they are in the DB, otherwise you get foriegn key errors because teams don't exist
						List<Matchup> matchups = Matchup.GetMatchups(_yahoo, ti.team_key);
						foreach (Matchup matchup in matchups)
						{
							WeekInfo wi = _sportsData.WeekInfos.SingleOrDefault(w => w.league_key == matchup.LeagueKey && w.week == matchup.Week);
							if (wi == null)
							{
								wi = matchup.CreateWeek();
								_sportsData.WeekInfos.InsertOnSubmit(wi);
								_sportsData.SubmitChanges();
							}

							MatchupInfo mi = matchup.CreateMatchup(wi);
							if (!_sportsData.MatchupInfos.Any(m => m.week_id == mi.week_id && m.league_key == mi.league_key && ((m.team1_key == mi.team1_key && m.team2_key == mi.team2_key) || (m.team1_key == mi.team2_key && m.team2_key == mi.team1_key))))
							{
								_sportsData.MatchupInfos.InsertOnSubmit(mi);
							}
						}
						_sportsData.SubmitChanges();
					}
				}
			}
			return leagueInfos;
		}

		public void AddWeeklyTeamData(string leagueKey, int week)
		{
			WeekInfo weekInfo = _sportsData.GetWeek(leagueKey, week);
			foreach(WeeklyTeamStats teamstats in WeeklyTeamStats.GetWeeklyTeamStats(_yahoo, leagueKey, week))
			{
				NBAWeeklyTeamStat wts = teamstats.CreateWeeklyTeamStats(weekInfo);
				NBAWeeklyTeamStat existingWts = _sportsData.NBAWeeklyTeamStats.SingleOrDefault(s => ((s.week_id == wts.week_id) && (s.team_key == wts.team_key)));
				if (existingWts != null)
				{
					existingWts.UpdateWeeklyTeamStats(wts);
				}
				else
				{
					_sportsData.NBAWeeklyTeamStats.InsertOnSubmit(wts);
				}
				_sportsData.SubmitChanges();
			}
		}
	}
}
