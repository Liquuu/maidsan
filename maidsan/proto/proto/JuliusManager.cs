using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Text;

namespace madesan
{
	class JuliusManager
	{
		//--------------------------------------------------------------------
		//--------------------------------------------------------------------
		//デリゲート
		public delegate void ResponseDelegate(string res);
		public ResponseDelegate OnResponse;

		//プロセス実行状態
		public bool IsRun
		{
			set; get;
		}
		//クライアント準備
		public bool IsReady
		{
			set; get;
		}

		//--------------------------------------------------------------------
		//--------------------------------------------------------------------
		//初期化
		public bool Initialize()
		{
			IsRun = false;
			IsReady = false;

			//juliusサーバーを起動
			IsRun = open_julius_server_prosess();
			if (!IsRun)
			{
				//error log
				Debug.Log("ERROR: could not run julius!");
			}

			return IsRun;
		}
		
		//更新処理
		public void Update()
		{
			//初期化チェック
			if (!IsRun)
			{
				return;
			}
			else if (!IsReady)
			{
				IsReady = initialize_julius_client();
				if (IsReady)
				{
					Debug.Log("Ready!");
				}
				else
				{
					Debug.Log("Initializing...");
					Thread.Sleep(1000); //wait
					return;
				}
			}

			//通常処理
			if (tmp_words != words)
			{
				words = tmp_words;

				//デリゲートを呼び出す
				OnResponse(words);
				Debug.Log("Julius response: " + words);
			}
			else
			{
				//Result = string.Empty;
			}
		}

		//終了処理
		void Finish()
		{
			if (IsReady)
			{
				//juliusサーバーを切断
				close_julius_client();
			}
			if (IsRun)
			{
				//juliusサーバーのプロセスを強制終了
				kill_julius_server_prosess();
			}
		}

		//--------------------------------------------------------------------
		// 以下julius処理
		//--------------------------------------------------------------------
		//------------------------------------------------------
		// julius処理用変数
		//------------------------------------------------------
		//レスポンス
		private string words = "HogeHoge";

		//julius
		private bool windowtype_hidden = false;
		private string program_name = "julius_server.exe";
		private string file = @".\julius_442\core";
		private string IPAddress = "localhost";
		private int port = 10500;
		private string command = "-C main.jconf -C am-gmm.jconf -input mic -48 -module -charconv utf-8 sjis";

		//TCP/IP用
		private TcpClient tcpip = null;
		private NetworkStream net;
		private string stream;

		//XML処理用
		private string regular = "WORD=\"([^。\"]+)\"";
		private string tmp_words = string.Empty;
		private byte[] data = new byte[10000];
		private Match sampling;
		private Regex xml_data;

		//プロセス
		private System.Diagnostics.Process julius_process;

		//スレッド
		private Thread julius_thread;

		//--------------------------------------------------------------------
		// julius Prosess
		//--------------------------------------------------------------------
		/*外部プログラムjuliusをコマンド付きで起動*/
		private bool open_julius_server_prosess()
		{
			System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
			info.FileName = program_name;
			info.WorkingDirectory = file;
			info.Arguments = command;
			if (windowtype_hidden)
			{
				info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			}
			//juliusプロセスをjulius_processに登録
			try
			{
				julius_process = System.Diagnostics.Process.Start(info);
			}
			catch (System.ComponentModel.Win32Exception w)
			{
				Debug.Log("Not Found: " + w);
				return false;
			}
			return true;
		}
		

		/*juliusサーバーへ接続する*/
		private bool initialize_julius_client()
		{
			Debug.Log("Julius Initialize...");
			//TCP/IPの初期化＆juliusサーバーへ接続
			tcpip = new TcpClient(IPAddress, port);
			//クライアントが取得出来たかどうか
			if (tcpip == null)
			{
				Debug.Log("Connect Fall.");
				return false;
			}
			else
			{
				Debug.Log("Connect Success.");
				//ストリームの取得
				net = tcpip.GetStream();
				//マルチスレッドへ登録＆開始
				julius_thread = new Thread(new ThreadStart(get_stream));
				julius_thread.Start();
				return true;
			}
		}

		/*外部プログラムjulisのプロセスを強制終了*/
		private void kill_julius_server_prosess()
		{
			//プロセスの強制終了
			julius_process.Kill();
			if (julius_process.HasExited)
			{
				Debug.Log("Kill julius server.");
			}
			else
			{
				julius_process.Kill();
			}
			julius_process.Close();
			julius_process.Dispose();
		}

		/*juliusサーバーから切断*/
		private void close_julius_client()
		{
			//TCP/IPの切断処理
			net.Close();
			//マルチスレッドの終了
			julius_thread.Abort();
		}
		
		//--------------------------------------------------------------------
		// Stream Thread
		//--------------------------------------------------------------------
		/*juliusサーバーから受信*/
		private void get_stream()
		{//**マルチスレッド関数**
			while (true)
			{
				//マルチスレッドの速度？
				Thread.Sleep(0);
				//ストリームの受信
				net.Read(data, 0, data.Length);
				stream = System.Text.Encoding.Default.GetString(data);
				//Debug.Log (stream);
				
				tmp_words = string.Empty;
				//XMLデータから文字列の抽出
				tmp_words = XML_search(stream);

			}
		}

		/*juliusサーバーへ送信*/
		private void send_stream(string msg)
		{
			//net = tcpip.GetStream ();
			byte[] send_byte = Encoding.UTF8.GetBytes(msg);
			//ストリームの送信
			net.Write(send_byte, 0, send_byte.Length);
			//Debug.Log ("Send Message -> "+msg);
		}

		/*ストリーム情報から正規表現を利用して文字列を抽出する*/
		private string XML_search(string stream)
		{
			string tmp = string.Empty;

			//正規表現
			xml_data = new Regex(regular);
			//初回抽出(NextMatch()を使うため)
			sampling = xml_data.Match(stream);
			while (sampling.Success)
			{//最後まで抽出
			 //結合処理
				for (int i = 1; i < sampling.Groups.Count; i++)
				{//なぜかi = 1にしたらうまく行った
					tmp += sampling.Groups[i].Value;
				}
				//順次抽出していく
				sampling = sampling.NextMatch();
			}
			//最終的に結合した文字列を返す
			return tmp;
		}
		//--------------------------------------------------------------
		//--------------------------------------------------------------
	}
}
