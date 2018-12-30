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

		public static MatchupInfo CreateMatchup(this Matchup matchup)
		{
			MatchupInfo m = new MatchupInfo();
			m.league_key = matchup.LeagueKey;
			m.team1_key = matchup.Team1key;
			m.team2_key = matchup.Team2key;
			m.week = matchup.Week;
			m.startDate = matchup.StartDate;
			m.endDate = matchup.EndDate;
			m.isPlayoffs = matchup.IsPlayoffs;
			m.isConsolation = matchup.IsConsolation;
			if((m.startDate.HasValue) && (m.endDate.HasValue))
			{
				if((m.endDate.Value - m.startDate.Value).TotalDays < 6)
				{
					m.isShortWeek = true;
					m.isLongWeek = false;
				}
				else if ((m.endDate.Value - m.startDate.Value).TotalDays > 6)
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

		public static NBAWeeklyTeamStat CreateWeeklyTeamStats(this WeeklyTeamStats teamStats)
		{
			NBAWeeklyTeamStat wts = new NBAWeeklyTeamStat();
			wts.team_key = teamStats.Teamkey;
			wts.week = teamStats.Week;
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
			return wts;
		}

		public static PlayerInfo CreatePlayerInfo(this Player player)
		{
			PlayerInfo pi = new PlayerInfo();

			return pi;
		}

	}
}
