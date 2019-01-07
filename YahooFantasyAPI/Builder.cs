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
				AddAdvancedWeeklyStats(league, league.WeekInfos.First());

				IEnumerable<WeekInfo> weeks = league.WeekInfos.Where(wi => (wi.startDate < DateTime.Now.Date && !wi.lastLoadDate.HasValue) || (wi.lastLoadDate.HasValue && wi.lastLoadDate.Value < wi.endDate.AddDays(2)));
				foreach (WeekInfo week in weeks)
				{
					//AddWeeklyTeamData(li.league_key, week);
					AddWeeklyData(league, week);
					week.lastLoadDate = DateTime.Now;
					_sportsData.SubmitChanges();

					//AddWeeklyIndividualData(li, week);
				}
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
					existingWts.UpdateWeeklyTeamStats(wts);
				}
				else
				{
					_sportsData.NBAWeeklyTeamStats.InsertOnSubmit(wts);
				}
				_sportsData.SubmitChanges();
			}
		}

		//public void AddWeeklyTeamData(string leagueKey, WeekInfo week)
		//{
		//	foreach(WeeklyTeamStats teamstats in WeeklyTeamStats.GetWeeklyTeamStats(_yahoo, leagueKey, week.week))
		//	{
		//		NBAWeeklyTeamStat wts = teamstats.CreateWeeklyTeamStats(week);
		//		NBAWeeklyTeamStat existingWts = _sportsData.NBAWeeklyTeamStats.SingleOrDefault(s => ((s.week_id == wts.week_id) && (s.team_key == wts.team_key)));
		//		if (existingWts != null)
		//		{
		//			existingWts.UpdateWeeklyTeamStats(wts);
		//		}
		//		else
		//		{
		//			_sportsData.NBAWeeklyTeamStats.InsertOnSubmit(wts);
		//		}
		//		_sportsData.SubmitChanges();
		//	}
		//}

		public List<NBAWeeklyPlayerStat> AddWeeklyIndividualData(LeagueInfo league, TeamInfo team, WeekInfo week)
		{
			List<WeekPlayerStats> weekStats = WeekPlayerStats.GetWeeklyPlayerStats(_yahoo, team.team_key, week.week);
			foreach (Player player in weekStats.Select(s => s.Player).Where(p => p.IsStarting.HasValue && p.IsStarting.Value))
			{
				//if (!league.GameInfo.PlayerInfos.Any(p => p.player_key == player.PlayerKey))
				if (!_sportsData.PlayerInfos.Any(p => p.game_key == league.game_key && p.player_key == player.PlayerKey))
				{
					PlayerInfo playerInfo = player.CreatePlayerInfo();
					_sportsData.PlayerInfos.InsertOnSubmit(playerInfo);
				}
			}
			_sportsData.SubmitChanges();

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
					existingStats.UpdateWeeklyPlayerStat(weeklyPlayerStat);
					retVal.Add(existingStats);
				}
				else
				{
					_sportsData.NBAWeeklyPlayerStats.InsertOnSubmit(weeklyPlayerStat);
					retVal.Add(weeklyPlayerStat);
				}
			}				
			_sportsData.SubmitChanges();
			return retVal;
		}

		public void AddAdvancedWeeklyStats(LeagueInfo league, WeekInfo week)
		{
			foreach(TeamInfo team in league.TeamInfos)
			{
				foreach(NBAWeeklyPlayerStat playerStats in team.NBAWeeklyPlayerStats)
				{
					NBAWeeklyTeamStat teamStats = team.NBAWeeklyTeamStats.Single(s => s.week_id == week.id);
					NBAAdvWeeklyPlayerStat advPlayerStats = new NBAAdvWeeklyPlayerStat();
					advPlayerStats.player_key = playerStats.player_key;
					advPlayerStats.team_key = team.team_key;
					advPlayerStats.week_id = week.id;
					advPlayerStats.ppg_pct = (decimal)playerStats.points.Value / teamStats.points.Value;
					advPlayerStats.rpg_pct = (decimal)playerStats.rebounds.Value / teamStats.rebounds.Value;
					advPlayerStats.apg_pct = (decimal)playerStats.assists.Value / teamStats.assists.Value;
					advPlayerStats.spg_pct = (decimal)playerStats.steals.Value / teamStats.steals.Value;
					advPlayerStats.bpg_pct = (decimal)playerStats.blocks.Value / teamStats.blocks.Value;
					advPlayerStats.points_win = (decimal)teamStats.points_win.ToInt() + ((decimal)teamStats.points_tie.ToInt() / 2);
					advPlayerStats.rebounds_win = (decimal)teamStats.rebounds_win.ToInt() + ((decimal)teamStats.rebounds_tie.ToInt() / 2);
					advPlayerStats.assists_win = (decimal)teamStats.assists_win.ToInt() + ((decimal)teamStats.assists_tie.ToInt() / 2);
					advPlayerStats.steals_win = (decimal)teamStats.steals_win.ToInt() + ((decimal)teamStats.steals_tie.ToInt() / 2);
					advPlayerStats.blocks_win = (decimal)teamStats.blocks_win.ToInt() + ((decimal)teamStats.blocks_tie.ToInt() / 2);
					advPlayerStats.pct_contribution = advPlayerStats.ppg_pct + advPlayerStats.rpg_pct + advPlayerStats.apg_pct + advPlayerStats.spg_pct + advPlayerStats.bpg_pct;
					advPlayerStats.win_pct_contribution = 0;
					advPlayerStats.win_pct_contribution = advPlayerStats.points_win * advPlayerStats.ppg_pct;
					advPlayerStats.win_pct_contribution += CalcWinPctContribution(teamStats.points_win, teamStats.points_tie, advPlayerStats.ppg_pct);
					advPlayerStats.win_pct_contribution += CalcWinPctContribution(teamStats.rebounds_win, teamStats.rebounds_tie, advPlayerStats.rpg_pct);
					advPlayerStats.win_pct_contribution += CalcWinPctContribution(teamStats.assists_win, teamStats.assists_tie, advPlayerStats.apg_pct);
					advPlayerStats.win_pct_contribution += CalcWinPctContribution(teamStats.steals_win, teamStats.steals_tie, advPlayerStats.spg_pct);
					advPlayerStats.win_pct_contribution += CalcWinPctContribution(teamStats.blocks_win, teamStats.blocks_tie, advPlayerStats.bpg_pct);
					decimal wins = teamStats.Wins() + ((decimal)teamStats.Ties() / 2);
					advPlayerStats.pct_contribution_of_wins = wins > 0 ? advPlayerStats.win_pct_contribution / wins : 0;
				}
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
