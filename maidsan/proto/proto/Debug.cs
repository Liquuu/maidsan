using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace madesan
{
	class Debug
	{
#if DEBUG
		public static void Log(string msg)
		{
			Console.WriteLine("DEBUG:" + msg);
		}
#else
		public static void Log(string msg) { }
#endif

	}
}
