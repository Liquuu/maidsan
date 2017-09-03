using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SoftalkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //挨拶
            var ai = Talk.Aisatu();
            Talk.Say(ai);

            //天気
            var we = Weather.GetWeatherText();
            Talk.Say(we);
            Talk.Close();

            //talk
            while (true)
            {
                Console.WriteLine("何かひらがなで入力してください...");
                string line = Console.ReadLine();

                Talk.Say(line + "、ですか？意味が分かりません。");
                Talk.Close();
            }
        }
    }
}
