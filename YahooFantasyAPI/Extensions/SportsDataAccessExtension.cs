using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SportsDataAccess;

namespace YahooFantasyAPI.Extensions
{
	public static class SportsDataAccessExtension
	{
		public static WeekInfo GetWeek(this SportsDataDataContext sportsData, string league_key, int weekNum)
		{
			return sportsData.WeekInfos.Single(wi => wi.league_key == league_key && wi.week == weekNum);
		}

		public static GameInfo CreateGameInfo(this Game game)
		{
			GameInfo gi = new GameInfo();
			gi.game_key = game.GameKey;
			gi.game_id = game.GameID;
			gi.code = game.Code;
			gi.name = game.Name;
			gi.season = game.Season;
			return gi;
		}

		public static LeagueInfo CreateLeagueInfo(this League league)
		{
			string[] keyPieces = league.LeagueKey.Split('.'); // {game_key}.l.{league_id}
			LeagueInfo li = new LeagueInfo();
			li.game_key = keyPieces[0];
			li.league_id = keyPieces[2];
			li.league_key = league.LeagueKey;
			li.name = league.Name;
			return li;
		}

		public static TeamInfo CreateTeamInfo(this Team team)
		{
			string[] keyPieces = team.TeamKey.Split('.'); // {game_key}.l.{league_id}.t.{team_id}
			TeamInfo ti = new TeamInfo();
			ti.league_key = string.Join(".", keyPieces[0], keyPieces[1], keyPieces[2]);
			ti.manager = team.ManagerName;
			ti.name = team.Name;
			ti.team_id = keyPieces[4];
			ti.team_key = team.TeamKey;
			return ti;
		}

		public static MatchupInfo CreateMatchup(this Matchup matchup, WeekInfo week)
		{
			return matchup.CreateMatchup(week.id);
		}

		public static MatchupInfo CreateMatchup(this Matchup matchup, int weekId)
		{
			MatchupInfo m = new MatchupInfo();
			m.week_id = weekId;
			m.league_key = matchup.LeagueKey;
			m.team1_key = matchup.Team1key;
			m.team2_key = matchup.Team2key;
			return m;
		}

		public static WeekInfo CreateWeek(this Matchup matchup)
		{
			WeekInfo m = new WeekInfo();
			m.league_key = matchup.LeagueKey;
			m.week = matchup.Week;
			m.startDate = matchup.StartDate.HasValue ? matchup.StartDate.Value : DateTime.MinValue;
			m.endDate = matchup.EndDate.HasValue ? matchup.EndDate.Value : DateTime.MaxValue;
			m.isPlayoffs = matchup.IsPlayoffs.HasValue ? matchup.IsPlayoffs.Value : false;
			m.isConsolation = matchup.IsConsolation.HasValue ? matchup.IsConsolation.Value : false;
			if ((matchup.StartDate.HasValue) && (matchup.EndDate.HasValue))
			{
				if ((m.endDate - m.startDate).TotalDays < 6)
				{
					m.isShortWeek = true;
					m.isLongWeek = false;
				}
				else if ((m.endDate - m.startDate).TotalDays > 6)
				{
					m.isShortWeek = false;
					m.isLongWeek = true;
				}
				else
				{
					m.isShortWeek = false;
					m.isLongWeek = false;
				}
			}
			return m;
		}

		public static void UpdateWeeklyTeamStats(this NBAWeeklyTeamStat existingTeamStats, NBAWeeklyTeamStat updatedTeamStats)
		{
			existingTeamStats.games_played = updatedTeamStats.games_played;
			existingTeamStats.games_missed = updatedTeamStats.games_missed;
			existingTeamStats.points = updatedTeamStats.points;
			existingTeamStats.rebounds = updatedTeamStats.rebounds;
			existingTeamStats.assists = updatedTeamStats.assists;
			existingTeamStats.steals = updatedTeamStats.steals;
			existingTeamStats.blocks = updatedTeamStats.blocks;
			existingTeamStats.points_win = updatedTeamStats.points_win;
			existingTeamStats.rebounds_win = updatedTeamStats.rebounds_win;
			existingTeamStats.assists_win = updatedTeamStats.assists_win;
			existingTeamStats.steals_win = updatedTeamStats.steals_win;
			existingTeamStats.blocks_win = updatedTeamStats.blocks_win;
			existingTeamStats.points_tie = updatedTeamStats.points_tie;
			existingTeamStats.rebounds_tie = updatedTeamStats.rebounds_tie;
			existingTeamStats.assists_tie = updatedTeamStats.assists_tie;
			existingTeamStats.steals_tie = updatedTeamStats.steals_tie;
			existingTeamStats.blocks_tie= updatedTeamStats.blocks_tie;
		}

		public static NBAWeeklyTeamStat CreateWeeklyTeamStats(this WeeklyTeamStats teamStats, WeekInfo week)
		{
			return teamStats.CreateWeeklyTeamStats(week.id);
		}

		public static NBAWeeklyTeamStat CreateWeeklyTeamStats(this WeeklyTeamStats teamStats, int weekId)
		{
			NBAWeeklyTeamStat wts = new NBAWeeklyTeamStat();
			wts.team_key = teamStats.Teamkey;
			//wts.week_id = teamStats.Week;
			wts.week_id = weekId;
			wts.games_played = teamStats.GamesPlayed;
			wts.games_missed = teamStats.GamesMissed;
			wts.points = teamStats.TeamStats.Points;
			wts.rebounds = teamStats.TeamStats.Rebounds;
			wts.assists = teamStats.TeamStats.Assists;
			wts.steals = teamStats.TeamStats.Steals;
			wts.blocks = teamStats.TeamStats.Blocks;
			wts.points_win = teamStats.PtsWin;
			wts.rebounds_win = teamStats.RebsWin;
			wts.assists_win = teamStats.AsstsWin;
			wts.steals_win = teamStats.StlsWin;
			wts.blocks_win = teamStats.BlksWin;
			wts.points_tie = teamStats.PtsTie;
			wts.rebounds_tie = teamStats.RebsTie;
			wts.assists_tie = teamStats.AsstsTie;
			wts.steals_tie = teamStats.StlsTie;
			wts.blocks_tie = teamStats.BlksTie;
			return wts;
		}

		public static PlayerInfo CreatePlayerInfo(this Player player)
		{
			PlayerInfo pi = new PlayerInfo();

			return pi;
		}

	}
}
