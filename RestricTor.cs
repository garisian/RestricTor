using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

 
namespace g2wicked
{
  	class RestricTor
   {
   		public static string SSID = "";
   		public static bool torIsOn;
   		public static string yo = "";

   		static bool allowSSID(string SSID)
   		{
   			//Create Directory in Program Files to store accepted SSID file if it doesn't exist
   			if(!Directory.Exists("C:\\RestricTor"))
   			{
   				Directory.CreateDirectory("C:\\RestricTor");
   				Console.Write("\nCreated Directory C:\\RestricTor");
   			}

   			//Create accepted SSID file if doesn't exist
	    	if(!File.Exists("C:\\RestricTor\\accepted.txt"))
   			{
   				File.Create("C:\\RestricTor\\accepted.txt").Close();
   				Console.Write("\nEmpty file created for accepted SSIDs.");
   			}

   			//Get list of Accepted SSIDs
			string[] accepted = File.ReadAllLines("C:\\RestricTor\\accepted.txt");
			
			//Check if the current SSID matches any in the list
			foreach (string line in accepted)
			{
				if(line.Equals(SSID))
				{
					return true;
				}
  			}
  			if(SSID.Equals(""))
  			{
  				return true;
  			}
  			return false;
   		}
        
        static void runCMD(int cmdType)
        {
        	//Create new command prompt process
	        System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = @"cmd.exe";
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = true;
			process.StartInfo = startInfo;
			process.Start();

			//write command to prompt
			System.IO.StreamReader SR = process.StandardOutput;
			System.IO.StreamWriter SW = process.StandardInput;
			SW.WriteLine("@echo on");
			
			switch(cmdType)
			{
				//case 0:

				//0: netsh; 1: tasklist; 2: taskkill
				case 0:
				{
					SW.WriteLine("netsh wlan show interfaces | find \" SSID\"");
					
					//Advance streamreader position to current end of open buffer
					while(!SR.ReadLine().Contains("netsh")){}
					string SSIDLine = SR.ReadLine();
					if(!SSIDLine.Equals(""))
					{
						string [] SLTok = SSIDLine.Split(':');
						SSID = SLTok[1].Substring(1);
						Console.Write("\nThe current network's SSID is " + SSID);
					}
					else
					{
						SSID = "";
						Console.Write("\nYou are not connected to a network");
					}
					break;
				}
				case 1:
				{
					SW.WriteLine("tasklist | find \"" + yo + "\"");
					
					//Advance streamreader position to current end of open buffer
					while(!SR.ReadLine().Contains("tasklist")){}
					string res = SR.ReadLine();
					if(res.Contains("Console"))
					{
						torIsOn = true;
						Console.Write("\nTorrent Client is running");
					}
					else
					{
						torIsOn = false;
						Console.Write("\nTorrent Client is not running");
					}
					break;
				}
				case 2:
				{
					SW.WriteLine("taskkill /F /IM \"" + yo + "\"");
					Console.Write("\nTorrent Client closed");
					break;
				}
			}
			SW.WriteLine("exit");
    		SR.ReadToEnd();
    		SW.Close();
    		SR.Close();
        }

        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form();
            prompt.Width = 250;
            prompt.Height = 200;
            prompt.Text = caption;
            prompt.MinimizeBox = false;
            prompt.MaximizeBox = false;
            prompt.FormBorderStyle = FormBorderStyle.FixedSingle;
            Label textLabel = new Label() { Left = 20, Top = 20, Width = 230, Text = text };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 150, Text = "" };
            Button confirmation = new Button() { Text = "Okay", Left = 20, Width = 50, Top = 82 };
            Button select = new Button() { Text = "Select Files", Left = 20, Width = 100, Top = 110};
            confirmation.Click += (sender, e) => { prompt.Close(); };
            string t = "";
            select.Click += (sender, e) => { textBox.Text = FileSelector();};
            Console.Write(t);
            //textBox.Text = string.Copy(t);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(select);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.ShowDialog();
            return textBox.Text;
        }

        public static string FileSelector()
        {
        	 // Create an instance of the open file dialog box.
            OpenFileDialog fileD = new OpenFileDialog(); //create object
			fileD.Filter = "Applications (*.exe)|*.exe"; //define filter
			//fileD.InitialDirectory = @"C:\\";
			string tb = "";
			if(fileD.ShowDialog() == DialogResult.OK)
			{
				tb = "" + fileD.FileName + ""; //show dialog
				Console.Write(tb);
			}
			//MessageBox.show(fileD.fileName); //Get uploaded file
            return tb;
        }
        [STAThreadAttribute]
        static void Main(string[] args)
        {
        	yo = ShowDialog("What App do you wish to block today?", "What App Do You Hate?");
        	if(yo.Equals(""))
        	{
        		yo = "Invalid stuff";
        	}
        	Console.Write("\nCreate RegKey for startup");
        	RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        	rkApp.SetValue("RestricTor", Application.ExecutablePath.ToString());
        	
        	while(true)
        	{
        		runCMD(0);
        		runCMD(1);
        		if(!allowSSID(SSID) & torIsOn)
        		{
        			runCMD(2);
        		}
        		Console.Write(".");
        		System.Threading.Thread.Sleep(1000);
        		Console.Write(".");
        		System.Threading.Thread.Sleep(1000);
        		Console.Write(".");
        		System.Threading.Thread.Sleep(1000);
        		Console.Write(".");
        		System.Threading.Thread.Sleep(1000);
        		Console.Write("\n--------------------------------------");
        	}
        }
   }
}
