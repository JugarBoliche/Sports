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
			foreach(LeagueInfo league in leagues)
			{
				//AddAdvancedWeeklyStats(league, league.WeekInfos.First());

				IEnumerable<WeekInfo> weeks = league.WeekInfos.Where(wi => (wi.startDate < DateTime.Now.Date && !wi.lastLoadDate.HasValue) || (wi.lastLoadDate.HasValue && wi.lastLoadDate.Value < wi.endDate.AddDays(2)));
				foreach (WeekInfo week in weeks)
				{
					//AddWeeklyTeamData(li.league_key, week);
					AddWeeklyData(league, week);
					if (week.endDate < DateTime.Now)
					{
						AddAdvancedWeeklyStats(league, week);
					}
					week.lastLoadDate = DateTime.Now;
					_sportsData.SubmitChanges();

					//AddWeeklyIndividualData(li, week);
				}
				UpdatePlayerSeasonStats(league);
			}			
		}

		public void PopulateAdvancedStats(string game)
		{
			List<LeagueInfo> leagues = AddLeagueData(game);
			foreach (LeagueInfo league in leagues)
			{
				IEnumerable<WeekInfo> weeks = league.WeekInfos.Where(wi => (wi.startDate < DateTime.Now.Date && !wi.lastLoadDate.HasValue) || (wi.lastLoadDate.HasValue && wi.lastLoadDate.Value < wi.endDate.AddDays(2)));
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

		public void AddWeeklyData(LeagueInfo league, WeekInfo week)
		{
			foreach (WeeklyTeamStats teamstats in WeeklyTeamStats.GetWeeklyTeamStats(_yahoo, league.league_key, week.week))
			{
				TeamInfo team = league.TeamInfos.Single(t => t.team_key == teamstats.Teamkey);

				List<NBAWeeklyPlayerStat> playerStats = AddWeeklyIndividualData(league, team, week);

				int? played = playerStats.Sum(s => s.games_played);
				int? missed = playerStats.Sum(s => s.games_missed);
				NBAWeeklyTeamStat wts = teamstats.CreateWeeklyTeamStats(week, played == null ? 0 : played.Value, missed == null ? 0 : missed.Value);
				NBAWeeklyTeamStat existingWts = _sportsData.NBAWeeklyTeamStats.SingleOrDefault(s => ((s.week_id == wts.week_id) && (s.team_key == wts.team_key)));
				if (existingWts != null)
				{
					existingWts.games_played = wts.games_played;
					existingWts.games_missed = wts.games_missed;
					List<StatTeamWeekTotal> stats = teamstats.CreateAllTeamWeekTotals(existingWts);
					AddOrUpdateStatTeamWeekTotal(stats);
				}
				else
				{
					_sportsData.NBAWeeklyTeamStats.InsertOnSubmit(wts);
					_sportsData.SubmitChanges();
					List<StatTeamWeekTotal> stats = teamstats.CreateAllTeamWeekTotals(wts);
					AddOrUpdateStatTeamWeekTotal(stats);
				}
				_sportsData.SubmitChanges();
			}
		}

		private void AddOrUpdateStatTeamWeekTotal(IEnumerable<StatTeamWeekTotal> stats)
		{
			foreach (StatTeamWeekTotal stat in stats)
			{
				StatTeamWeekTotal existingStat = _sportsData.StatTeamWeekTotals.SingleOrDefault(s => s.nba_weekly_team_id == stat.nba_weekly_team_id && s.stat_type_id == stat.stat_type_id);
				if (existingStat != null)
				{
					existingStat.win = stat.win;
					existingStat.tie = stat.tie;
					existingStat.total = stat.total;
				}
				else
				{
					_sportsData.StatTeamWeekTotals.InsertOnSubmit(stat);
				}
			}
			_sportsData.SubmitChanges();
		}

		public void AddPlayerIfNeeded(IEnumerable<Player> players, GameInfo game)
		{
			foreach (Player player in players)
			{
				//if (!league.GameInfo.PlayerInfos.Any(p => p.player_key == player.PlayerKey))
				if (!_sportsData.PlayerInfos.Any(p => p.game_key == game.game_key && p.player_key == player.PlayerKey))
				{
					PlayerInfo playerInfo = player.CreatePlayerInfo();
					_sportsData.PlayerInfos.InsertOnSubmit(playerInfo);
				}
			}
			_sportsData.SubmitChanges();
		}

		public List<NBAWeeklyPlayerStat> AddWeeklyIndividualData(LeagueInfo league, TeamInfo team, WeekInfo week)
		{
			List<WeekPlayerStats> weekStats = WeekPlayerStats.GetWeeklyPlayerStats(_yahoo, team.team_key, week.week);

			AddPlayerIfNeeded(weekStats.Select(s => s.Player).Where(p => p.IsStarting.HasValue && p.IsStarting.Value), league.GameInfo);

			List<DatePlayerStats> dateStats = new List<DatePlayerStats>();
			DateTime startDate = week.startDate.Date;
			DateTime endDate = week.endDate.Date > DateTime.Now.Date ? DateTime.Now.Date : week.endDate.Date;
			while(startDate <= endDate)
			{
				dateStats.AddRange(DatePlayerStats.GetDatePlayerStats(_yahoo, team.team_key, startDate));
				startDate = startDate.AddDays(1);
			}

			List<NBAWeeklyPlayerStat> retVal = new List<NBAWeeklyPlayerStat>();
			List<NBAWeeklyPlayerStat> weeklyPlayerStats = week.CreateWeeklyPlayerStats(weekStats, dateStats);
			foreach(NBAWeeklyPlayerStat weeklyPlayerStat in weeklyPlayerStats)
			{
				NBAWeeklyPlayerStat existingStats = _sportsData.NBAWeeklyPlayerStats.SingleOrDefault(s => s.week_id == weeklyPlayerStat.week_id && s.player_key == weeklyPlayerStat.player_key);
				if(existingStats != null)
				{
					existingStats.games_played = weeklyPlayerStat.games_played;
					existingStats.games_missed = weeklyPlayerStat.games_missed;
					List<StatPlayerWeekTotal> playerStats = weekStats.Single(s => s.PlayerKey == existingStats.player_key).CreateStatPlayerWeekTotal(existingStats);
					AddOrUpdateStatPlayerWeekTotal(playerStats);
					retVal.Add(existingStats);
				}
				else
				{
					_sportsData.NBAWeeklyPlayerStats.InsertOnSubmit(weeklyPlayerStat);
					_sportsData.SubmitChanges();
					List<StatPlayerWeekTotal> playerStats = weekStats.Single(s => s.PlayerKey == weeklyPlayerStat.player_key).CreateStatPlayerWeekTotal(weeklyPlayerStat);
					AddOrUpdateStatPlayerWeekTotal(playerStats);
					retVal.Add(weeklyPlayerStat);
				}
			}				
			_sportsData.SubmitChanges();
			return retVal;
		}
		private void AddOrUpdateStatPlayerWeekTotal(IEnumerable<StatPlayerWeekTotal> stats)
		{
			foreach (StatPlayerWeekTotal stat in stats)
			{
				StatPlayerWeekTotal existingStat = _sportsData.StatPlayerWeekTotals.SingleOrDefault(s => s.nba_weekly_player_id == stat.nba_weekly_player_id && s.stat_type_id == stat.stat_type_id);
				if (existingStat != null)
				{
					existingStat.total = stat.total;
				}
				else
				{
					_sportsData.StatPlayerWeekTotals.InsertOnSubmit(stat);
				}
			}
			_sportsData.SubmitChanges();
		}

		public void AddAdvancedWeeklyStats(LeagueInfo league, WeekInfo week)
		{
			foreach(TeamInfo team in league.TeamInfos)
			{
				//NBAWeeklyTeamStat teamWeek = team.NBAWeeklyTeamStats.Single(s => s.week_id == week.id);
				NBAWeeklyTeamStat teamWeek = _sportsData.NBAWeeklyTeamStats.Single(s => s.team_key == team.team_key && s.week_id == week.id);

				foreach (NBAWeeklyPlayerStat playerWeek in team.NBAWeeklyPlayerStats.Where(s => s.week_id == week.id))
				{
					NBAAdvWeeklyPlayerStat advPlayerWeek = _sportsData.NBAAdvWeeklyPlayerStats.SingleOrDefault(s => s.player_key == playerWeek.player_key && s.week_id == playerWeek.week_id && s.team_key == playerWeek.team_key);
					if (advPlayerWeek == null)
					{
						advPlayerWeek = new NBAAdvWeeklyPlayerStat();
						advPlayerWeek.player_key = playerWeek.player_key;
						advPlayerWeek.team_key = team.team_key;
						advPlayerWeek.week_id = week.id;
						_sportsData.NBAAdvWeeklyPlayerStats.InsertOnSubmit(advPlayerWeek);
						_sportsData.SubmitChanges();
					}

					IEnumerable<StatPlayerWeekTotal> playerStats = playerWeek.StatPlayerWeekTotals;

					List<StatAdvPlayerWeek> advPlayerStats = new List<StatAdvPlayerWeek>();
					foreach (StatPlayerWeekTotal playerStat in playerStats)
					{
						StatTeamWeekTotal teamStat = teamWeek.StatTeamWeekTotals.Single(s => s.stat_type_id == playerStat.stat_type_id);
						StatAdvPlayerWeek advPlayerStat = _sportsData.StatAdvPlayerWeeks.SingleOrDefault(s => s.nba_adv_weekly_player_id == advPlayerWeek.id && s.stat_type_id == playerStat.stat_type_id);
						if (advPlayerStat == null)
						{
							advPlayerStat = new StatAdvPlayerWeek();
							advPlayerStat.nba_adv_weekly_player_id = advPlayerWeek.id;
							advPlayerStat.stat_type_id = playerStat.stat_type_id;
							_sportsData.StatAdvPlayerWeeks.InsertOnSubmit(advPlayerStat);
						}
						advPlayerStat.percentage = teamStat.total == 0 ? 0 : (decimal)playerStat.total / teamStat.total;
						advPlayerStat.win = teamStat.win.ToInt() + ((decimal)teamStat.tie.ToInt() / 2);
						advPlayerStat.win_share = advPlayerStat.percentage * advPlayerStat.win;
						_sportsData.SubmitChanges();
						advPlayerStats.Add(advPlayerStat);
					}

					advPlayerWeek.pct_contribution = advPlayerStats.Sum(s => s.percentage);
					advPlayerWeek.wins = advPlayerStats.Sum(s => s.win);
					advPlayerWeek.win_share_contribution = advPlayerStats.Sum(s => s.win_share);
					advPlayerWeek.wins_responsibility = advPlayerWeek.wins > 0 ? advPlayerWeek.win_share_contribution / advPlayerWeek.wins : 0;
					_sportsData.SubmitChanges();
				}
			}
		}

		public void UpdatePlayerSeasonStats(LeagueInfo league)
		{
			foreach(PlayerInfo player in league.GameInfo.PlayerInfos)
			{
				IEnumerable<NBAAdvWeeklyPlayerStat> advPlayerWeeks = _sportsData.NBAAdvWeeklyPlayerStats.Where(s => s.player_key == player.player_key);
				IEnumerable<StatAdvPlayerWeek> advPlayerStats = advPlayerWeeks.SelectMany(s => s.StatAdvPlayerWeeks);

				// Get NBAAdvTotalPlayerStat if it exists
				NBAAdvTotalPlayerStat playerSeason = _sportsData.NBAAdvTotalPlayerStats.SingleOrDefault(s => s.player_key == player.player_key && s.league_key == league.league_key);

				// If it doesn't exist create it and persist to the DB
				if(playerSeason == null)
				{
					playerSeason = new NBAAdvTotalPlayerStat();
					playerSeason.league_key = league.league_key;
					playerSeason.player_key = player.player_key;
					_sportsData.NBAAdvTotalPlayerStats.InsertOnSubmit(playerSeason);
					_sportsData.SubmitChanges();
				}
				
				// Create the individual advanced stat entries for each type of statistic. Determine types of statistic by group on the stat_type_id.
				foreach(var statsByType in advPlayerStats.GroupBy(s => s.stat_type_id))
				{
					// Get the StatAdvPlayerSeason for this player and stat if it exists
					StatAdvPlayerSeason seasonStat = _sportsData.StatAdvPlayerSeasons.SingleOrDefault(s => s.nba_adv_total_player_id == playerSeason.id && s.stat_type_id == statsByType.Key);

					// If it doesn't exist, create it and persist it to the DB
					if(seasonStat == null)
					{
						seasonStat = new StatAdvPlayerSeason();
						seasonStat.nba_adv_total_player_id = playerSeason.id;
						seasonStat.stat_type_id = statsByType.Key;
						_sportsData.StatAdvPlayerSeasons.InsertOnSubmit(seasonStat);
					}

					// Set the additional values that need to be set (new or updating existing)
					seasonStat.percentage = statsByType.Sum(s => s.percentage);
					seasonStat.win = statsByType.Sum(s => s.win);
					seasonStat.win_shares= statsByType.Sum(s => s.win_share);
				}
				_sportsData.SubmitChanges();

				// Calculate the advanced stats
				playerSeason.weeks_started = advPlayerWeeks.Count();
				playerSeason.percentage = advPlayerWeeks.Sum(s => s.pct_contribution);
				playerSeason.wins = advPlayerWeeks.Sum(s => s.wins);
				playerSeason.win_shares_contribution = advPlayerWeeks.Sum(s => s.win_share_contribution);
				playerSeason.win_shares_contribution_per_start = playerSeason.win_shares_contribution / playerSeason.weeks_started;
				playerSeason.win_shares_contribution_per_win = playerSeason.wins == 0 ? 0 : playerSeason.win_shares_contribution / playerSeason.wins;
				playerSeason.player_win_pct = advPlayerStats.Count() == 0 ? 0 : playerSeason.wins / advPlayerStats.Count();
				_sportsData.SubmitChanges();
			}
		}
		private decimal CalcWinPctContribution(bool? catWin, bool? catTie, decimal? perGamePct)
		{
			decimal retVal = 0;
			if (catWin.HasValue && catWin.Value && perGamePct.HasValue)
			{
				retVal = perGamePct.Value;
			}
			else if (catTie.HasValue && catTie.Value && perGamePct.HasValue)
			{
				retVal = perGamePct.Value / 2;
			}
			return retVal;
		}
	}
}
