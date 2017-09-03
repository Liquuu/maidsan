using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace SoftalkTest
{
    class Talk
    {
        private static string name = "メルセデス";
        private static string myMaster = "ご主人様";

        public static string Aisatu()
        {
            string line;
            DateTime dt = DateTime.Now;

            if(dt.Hour > 5 && dt.Hour <= 11)
            {
                line = "おはようございます、";
            }
            else if(dt.Hour > 11 && dt.Hour <= 18)
            {
                line = "こんにちは、";
            }
            else{
                line = "こんばんは、";
            }

            return line + myMaster + "、" + name + "です。";
        }

        public static void Say(string line)
        {
            // パラメータを指定して実行
            Process.Start("softalk\\Softalk.exe", "/W:" + line);
        }

        public static void Close()
        {
            Process.Start("softalk\\Softalk.exe", @"/play");
            Process.Start("softalk\\Softalk.exe", @"/close");
        }


#if false
        static void Main(string[] args)
        {
            while (true)
            {
                const int speed = 100;
                int size = 0;

                Console.WriteLine("何かひらがなで入力してください...");
                string koe = Console.ReadLine();

                //音声ファイルとしてそのまま保存可能なバイト列の先頭ポイントを取得
                IntPtr wavPtr = AquesTalk_Synthe(koe, speed, ref size);

                //成功判定
                if (wavPtr == IntPtr.Zero)
                {
                    Console.WriteLine("ERROR: 音声生成に失敗しました。不正な文字が使われた可能性があります");
                    continue;
                }

                //C#で扱えるようにマネージド側へコピー
                byte[] wav = new byte[size];
                Marshal.Copy(wavPtr, wav, 0, size);

                //アンマネージドポインタは用が無くなった瞬間に解放
                AquesTalk_FreeWave(wavPtr);

                //そのまま再生
                using (var ms = new MemoryStream(wav))
                using (var sp = new SoundPlayer(ms))
                {
                    sp.Play();
                }
            }
        }

        const string dllName = "dll\\AquesTalk.dll";

        [DllImport(dllName)]
        extern static IntPtr AquesTalk_Synthe(string koe, int speed, ref int size);

        [DllImport(dllName)]
        extern static void AquesTalk_FreeWave(IntPtr wavPtr);
    }
#endif
    }
}
