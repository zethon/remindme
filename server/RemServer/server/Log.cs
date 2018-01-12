using System;
using System.IO;
using xCon;
//using System.Windows.Forms;

namespace server
{
	/// <summary>
	/// Summary description for Output.
	/// </summary>
	public sealed class Log //: IMessageFilter 
	{
        static readonly Log _instance = new Log();
        public static Log Instance
        {
            get { return _instance; }
        }

        static Log()
        {
        }

        Log()
        {
        }

        private bool _hideTime = false;
        public bool HideTime
        {
            get { return _hideTime; }
            set { _hideTime = value; }
        }
        
        private string m_strBotName = string.Empty;
        public string BotName
        {
            get { return m_strBotName; }

            set
            {
                m_strBotName = value;
            }
        }

        public string LogFile
        {
            get 
            {
                if (m_strBotName == string.Empty)
                    throw new Exception("Output class has no botname set.");

                return @"logs\" + m_strBotName + @"\" + DateTime.Now.ToString("yyyy-MM-dd") + @".log";
            }
        }


        // (2008-08-27 20:55:55): MSN connected....
        public void WriteLine(string strText, params object[] args)
        {
            lock (this)
            {
                string strTime = HideTime ? "" : GetTimeString();

                try
                {
                    StreamWriter sw = new StreamWriter(LogFile, true);
                    sw.WriteLine(strTime + strText, args);
                    sw.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Log.WriteLine() error: " + e.Message);
                }

                Console.WriteLine(strTime + strText, args);
            }
        }


        public void WriteConsoleLine(string strText, params object[] args)
        {
            Console.WriteLine(strText, args);
        }

        public void WriteConsoleError(string strText)
        {
            WriteConsoleError(strText, null);
        }

        public void WriteConsoleError(string strText, Exception e)
        {
            Console.WriteLine(strText);
            Console.WriteLine(GetExceptionDump(e));
        }

        public string GetExceptionDump(Exception e)
        {
            string strRetVal = string.Empty;
            Exception currEx = e;

            while (currEx != null)
            {
                strRetVal += string.Format("{0}: {1}\r\n----------------------\r\n{2}\r\n----------------------", 
                                        currEx.GetType().ToString(), currEx.Message, currEx.StackTrace);
                currEx = currEx.InnerException;
            }

            return strRetVal;
        }
        
        


        // TODO: Get rid of this legacy code...
		public void WriteStatus(string strData)
		{
			WriteStatus(strData,false);
		}

		public void WriteStatus(string strData,bool bHideTime)
		{
			WriteString(strData,xCon.ConsoleColor.White,bHideTime);
		}

		public void WriteError(string strData)
		{
			WriteError(strData,false);
		}

		public void WriteError(string strData,bool bHideTime)
		{
			WriteString(strData,xCon.ConsoleColor.Yellow,bHideTime);
		}

		public void WriteString(string strData, xCon.ConsoleColor color, bool bHideTime)
		{
			string strTime = bHideTime ? "" : GetTimeString();
            xConsole.SetColor(color, xCon.ConsoleColor.Black);
			System.Console.WriteLine(strTime+strData);
            xConsole.SetColor(xCon.ConsoleColor.White, xCon.ConsoleColor.Black);

			//TODO: generate HTML logs files for better readability
			if (m_strBotName != null)
			{
				if (!Directory.Exists("logs"))
					Directory.CreateDirectory("logs");

				if (!Directory.Exists(@"logs\"+m_strBotName))
					Directory.CreateDirectory(@"logs\"+m_strBotName);

				string strFileName = @"logs\"+m_strBotName+@"\"+DateTime.Now.ToString("yyyy-MM-dd")+@".log";
				StreamWriter sw = new StreamWriter(strFileName,true);
				sw.WriteLine(strTime+strData);
				sw.Close();
			}
		}

		public string GetTimeString()
		{
			return "("+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+"): ";
		}

//		public bool PreFilterMessage(ref Message m) 
//		{
//			if (m.Msg == 49366)
//				return false;
//
//			Console.WriteLine("Processing the messages : " + m.Msg);
//			return false;
//		}
	}
}
