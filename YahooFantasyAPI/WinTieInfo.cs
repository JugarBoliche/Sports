using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YahooFantasyAPI
{
	public class WinTieInfo
	{
		public WinTieInfo(bool isWin, bool isTie)
		{
			IsWin = isWin;
			IsTie = IsTie;
		}
		public bool IsWin { get; private set; }
		public bool IsTie { get; private set; }
	}
}
