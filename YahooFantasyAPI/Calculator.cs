using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SportsDataAccess;
using YahooFantasyAPI.Extensions;

namespace YahooFantasyAPI
{
	public class Calculator
	{
		SportsDataDataContext _sportsData;

		public Calculator()
		{
			_sportsData = new SportsDataDataContext();
		}

		public LeagueInfo GetCurrentNBA()
		{
			GameInfo game = _sportsData.GameInfos.Single(g => g.code.Equals("nba") && g.season.Equals("2018"));
			return game.LeagueInfos.Single();
		}

		public Dictionary<TeamInfo, StatLine> GetTotalYearStats(LeagueInfo league)
		{
			Dictionary<TeamInfo, StatLine> stats = new Dictionary<TeamInfo, StatLine>();
			foreach (TeamInfo team in league.TeamInfos)
			{
				var teamIndPastStats = _sportsData.StatTeamWeekTotals.Where(s => s.NBAWeeklyTeamStat.team_key == team.team_key && s.NBAWeeklyTeamStat.WeekInfo.endDate < DateTime.Now);
				var teamWeekPastStats = _sportsData.NBAWeeklyTeamStats.Where(s => s.team_key == team.team_key && s.WeekInfo.endDate < DateTime.Now);
				int? pts = teamIndPastStats.Where(s => s.stat_type_id == 1 ).Sum(s => s.total);
				int? rebs = teamIndPastStats.Where(s => s.stat_type_id == 2).Sum(s => s.total);
				int? asts = teamIndPastStats.Where(s => s.stat_type_id == 3).Sum(s => s.total);
				int? stls = teamIndPastStats.Where(s => s.stat_type_id == 4).Sum(s => s.total);
				int? blks = teamIndPastStats.Where(s => s.stat_type_id == 5).Sum(s => s.total);
				int? gp = teamWeekPastStats.Sum(s=> s.games_played);
				int? gm = teamWeekPastStats.Sum(s => s.games_missed);
				int? totalGames = gp + gm;
				StatLine stat = new StatLine(pts ?? 0, rebs ?? 0, asts ?? 0, stls ?? 0, blks ?? 0);
				stats.Add(team, stat);
			}
			return stats;
		}

		public Dictionary<TeamInfo, StatLine> GetAvgYearStats(LeagueInfo league)
		{
			Dictionary<TeamInfo, StatLine> stats = new Dictionary<TeamInfo, StatLine>();
			foreach (TeamInfo team in league.TeamInfos)
			{
				var teamPastStats = _sportsData.StatTeamWeekTotals.Where(s => s.NBAWeeklyTeamStat.team_key == team.team_key && s.NBAWeeklyTeamStat.WeekInfo.endDate < DateTime.Now);
				double? pts = teamPastStats.Where(s => s.stat_type_id == 1).Average(s => s.total);
				double? rebs = teamPastStats.Where(s => s.stat_type_id == 2).Average(s => s.total);
				double? asts = teamPastStats.Where(s => s.stat_type_id == 3).Average(s => s.total);
				double? stls = teamPastStats.Where(s => s.stat_type_id == 4).Average(s => s.total);
				double? blks = teamPastStats.Where(s => s.stat_type_id == 5).Average(s => s.total);

				StatLine stat = null;// = new StatLine(pts ?? 0, rebs ?? 0, asts ?? 0, stls ?? 0, blks ?? 0);
				stats.Add(team, stat);
			}
			return stats;
		}
	}
}
