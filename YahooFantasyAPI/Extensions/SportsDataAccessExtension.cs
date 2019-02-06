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

		public static NBAWeeklyTeamStat CreateWeeklyTeamStats(this WeeklyTeamStats teamStats, WeekInfo week, int gamesPlayed, int gamesMissed)
		{
			return teamStats.CreateWeeklyTeamStats(week.id, gamesPlayed, gamesMissed);
		}

		public static int YahooStatIDToSportsDataStatID(string YahooStatID)
		{
			switch(YahooStatID)
			{
				case StatLine.Pts_Stat_ID:
					return 1;
				case StatLine.Rebs_Stat_ID:
					return 2;
				case StatLine.Asts_Stat_ID:
					return 3;
				case StatLine.Stls_Stat_ID:
					return 4;
				case StatLine.Blks_Stat_ID:
					return 5;
				default:
					return -1;
			}
		}

		public static NBAWeeklyTeamStat CreateWeeklyTeamStats(this WeeklyTeamStats teamStats, int weekId, int gamesPlayed, int gamesMissed)
		{
			NBAWeeklyTeamStat wts = new NBAWeeklyTeamStat();
			wts.team_key = teamStats.Teamkey;
			//wts.week_id = teamStats.Week;
			wts.week_id = weekId;
			wts.games_played = gamesPlayed;
			wts.games_missed = gamesMissed;
			return wts;
		}
		public static List<StatTeamWeekTotal> CreateAllTeamWeekTotals(this WeeklyTeamStats teamStats, NBAWeeklyTeamStat NbaWeeklyTeamStat)
		{
			List<StatTeamWeekTotal> stats = new List<StatTeamWeekTotal>();
			foreach (string yahooStatID in teamStats.StatsWinTieInfo.Keys)
			{
				StatTeamWeekTotal stat = new StatTeamWeekTotal();
				stat.nba_weekly_team_id = NbaWeeklyTeamStat.id;
				stat.stat_type_id = SportsDataAccessExtension.YahooStatIDToSportsDataStatID(yahooStatID);
				stat.win = teamStats.StatsWinTieInfo[yahooStatID].IsWin;
				stat.tie = teamStats.StatsWinTieInfo[yahooStatID].IsTie;
				stat.total = teamStats.TeamStats.GetStatValue(yahooStatID);
				stats.Add(stat);
			}
			return stats;
		}

		public static List<NBAWeeklyPlayerStat> CreateWeeklyPlayerStats(this WeekInfo week, List<WeekPlayerStats> weekStats, List<DatePlayerStats> dailyStats)
		{
			List<NBAWeeklyPlayerStat> weeklyPlayerStats = new List<NBAWeeklyPlayerStat>();
			//foreach(Player player in roster.Where(p => p.IsStarting))
			foreach (Player player in weekStats.Select(s => s.Player).Where(p => p.IsStarting.HasValue && p.IsStarting.Value))
			{
				foreach(WeekPlayerStats playerWeeklyStats in weekStats.Where(s => s.PlayerKey == player.PlayerKey))
				{
					List<DatePlayerStats> playerDailyStats = dailyStats.Where(s => s.PlayerKey == player.PlayerKey).ToList();
					int gamesPlayed = playerDailyStats.Count(s => s.Status == PlayedStatus.Played);
					int gamesMissed = playerDailyStats.Count(s => s.Status == PlayedStatus.DNP);
					weeklyPlayerStats.Add(week.CreateWeeklyPlayerStat(playerWeeklyStats, gamesPlayed, gamesMissed, player.SelectedPosition));
				}
			}
			return weeklyPlayerStats;
		}

		public static NBAWeeklyPlayerStat CreateWeeklyPlayerStat(this WeekInfo week, WeekPlayerStats playerWeeklyStats, int gamesPlayed, int gamesMissed, string position)
		{
			NBAWeeklyPlayerStat weeklyPlayerStats = new NBAWeeklyPlayerStat();
			weeklyPlayerStats.player_key = playerWeeklyStats.PlayerKey;
			weeklyPlayerStats.team_key = playerWeeklyStats.TeamKey;
			weeklyPlayerStats.week_id = week.id;
			weeklyPlayerStats.games_played = gamesPlayed;
			weeklyPlayerStats.games_missed = gamesMissed;
			weeklyPlayerStats.position = position;
			return weeklyPlayerStats;
		}

		public static List<StatPlayerWeekTotal> CreateStatPlayerWeekTotal(this WeekPlayerStats playerWeeklyStats, NBAWeeklyPlayerStat nbaStats)
		{
			List<StatPlayerWeekTotal> stats = new List<StatPlayerWeekTotal>();
			foreach (KeyValuePair<string, int?> stat in playerWeeklyStats.Stats.AllStats)
			{
				StatPlayerWeekTotal statPlayer = new StatPlayerWeekTotal();
				statPlayer.nba_weekly_player_id = nbaStats.id;
				statPlayer.stat_type_id = YahooStatIDToSportsDataStatID(stat.Key);
				statPlayer.total = stat.Value;
				stats.Add(statPlayer);
			}
			return stats;
		}

		public static PlayerInfo CreatePlayerInfo(this Player player)
		{
			PlayerInfo pi = new PlayerInfo();
			pi.player_key = player.PlayerKey;
			pi.player_id = player.PlayerID;
			pi.first_name = player.FirstName;
			pi.last_name = player.LastName;
			pi.game_key = player.PlayerKey.Split('.')[0];
			return pi;
		}

		public static int Wins(this NBAWeeklyTeamStat teamStats)
		{
			return teamStats.StatTeamWeekTotals.Sum(s => s.win.ToInt());
			//return teamStats.points_win.ToInt() + teamStats.rebounds_win.ToInt() + teamStats.assists_win.ToInt() + teamStats.steals_win.ToInt() + teamStats.blocks_win.ToInt();
		}

		public static int Losses(this NBAWeeklyTeamStat teamStats)
		{
			return teamStats.StatTeamWeekTotals.Sum(s => 1 - (s.win.ToInt() + s.tie.ToInt()));
			//int losses = teamStats.points_win.Value || teamStats.points_tie.Value ? 0 : 1;
			//losses = losses + (teamStats.rebounds_win.Value || teamStats.rebounds_tie.Value ? 0 : 1);
			//losses = losses + (teamStats.assists_win.Value || teamStats.assists_tie.Value ? 0 : 1);
			//losses = losses + (teamStats.steals_win.Value || teamStats.steals_tie.Value ? 0 : 1);
			//losses = losses + (teamStats.blocks_win.Value || teamStats.blocks_tie.Value ? 0 : 1);
			//return losses;
		}

		public static int Ties(this NBAWeeklyTeamStat teamStats)
		{
			return teamStats.StatTeamWeekTotals.Sum(s => s.tie.ToInt());
			//return teamStats.points_tie.ToInt() + teamStats.rebounds_tie.ToInt() + teamStats.assists_tie.ToInt() + teamStats.steals_tie.ToInt() + teamStats.blocks_tie.ToInt();
		}

		public static int ToInt(this bool? boolVal)
		{
			int retVal = 0;
			if(boolVal.HasValue && boolVal.Value)
			{
				retVal = 1;
			}
			return retVal;
		}

	}
}
