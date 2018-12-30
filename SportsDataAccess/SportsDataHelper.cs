using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportsDataAccess
{
	public class SportsDataHelper
	{
		public List<GameInfo> GetGames()
		{
			SportsDataDataContext sportsData = new SportsDataDataContext();
			return sportsData.GameInfos.ToList();
		}
		public void RegenerateAdvTotalPlayerStats()
		{
		}
	}
}
