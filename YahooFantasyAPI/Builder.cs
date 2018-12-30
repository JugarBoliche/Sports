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
				IEnumerable<MatchupInfo> matchups = li.MatchupInfos.Where(mi => mi.endDate.HasValue && mi.endDate < DateTime.Now);
				foreach(MatchupInfo matchup in matchups)
				{
					if(!matchup.TeamInfo.NBAWeeklyTeamStats.Any(s => s.week == matchup.week))
					{

					}
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
				LeagueInfo li = league.CreateLeagueInfo();
				leagueInfos.Add(li);
				if (!_sportsData.LeagueInfos.Any(l => l.league_key == li.league_key))
				{
					_sportsData.LeagueInfos.InsertOnSubmit(li);
					_sportsData.SubmitChanges();
				}

				List<Team> teams = Team.GetTeams(_yahoo, li.league_key);
				foreach (Team team in teams)
				{
					TeamInfo ti = team.CreateTeamInfo();
					if (!_sportsData.TeamInfos.Any(t => t.team_key == ti.team_key))
					{
						_sportsData.TeamInfos.InsertOnSubmit(ti);
					}
					_sportsData.SubmitChanges();
				}
				foreach (Team team in teams)
				{
					// Need to make sure to loop through all teams prior to this and make sure they are in the DB, otherwise you get foriegn key errors because teams don't exist
					List<Matchup> matchups = Matchup.GetMatchups(_yahoo, team.TeamKey);
					foreach(Matchup matchup in matchups)
					{
						MatchupInfo mi = matchup.CreateMatchup();
						if(!_sportsData.MatchupInfos.Any(m => m.week == mi.week && m.league_key == mi.league_key && ((m.team1_key == mi.team1_key && m.team2_key == mi.team2_key) || (m.team1_key == mi.team2_key && m.team2_key == mi.team1_key)) ))
						{
							_sportsData.MatchupInfos.InsertOnSubmit(mi);
						}
					}
					_sportsData.SubmitChanges();
				}
			}
			return leagueInfos;
		}

		public void AddWeeklyData(string leagueKey, int week)
		{
			foreach(WeeklyTeamStats teamstats in WeeklyTeamStats.GetWeeklyTeamStats(_yahoo, leagueKey, week))
			{
				NBAWeeklyTeamStat wts = teamstats.CreateWeeklyTeamStats();
				if(_sportsData.NBAWeeklyTeamStats.Any(s => ((s.week == wts.week) && (s.team_key == wts.team_key))))
				{
					NBAWeeklyTeamStat existingWts = _sportsData.NBAWeeklyTeamStats.Single(s => ((s.week == wts.week) && (s.team_key == wts.team_key)));
					existingWts.games_played = wts.games_played;
					existingWts.games_missed = wts.games_missed;
					existingWts.points = wts.points;
					existingWts.rebounds = wts.rebounds;
					existingWts.assists = wts.assists;
					existingWts.steals = wts.steals;
					existingWts.blocks = wts.blocks;
					existingWts.points_win = wts.points_win;
					existingWts.rebounds_win = wts.rebounds_win;
					existingWts.assists_win = wts.assists_win;
					existingWts.steals_win = wts.steals_win;
					existingWts.blocks_win = wts.blocks_win;
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
