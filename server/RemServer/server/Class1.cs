using System;
using System.IO;
using System.Xml;
using System.Resources;
using System.Timers;
using System.Reflection;
using System.Globalization;
using xCon;
using System.Runtime.InteropServices;
using System.Collections;
using System.Web.Mail;
using System.Threading;
using log4net;

namespace server
{

	class App
	{
		// bot object
		BotDaemon RemBot;
		// system manager objects
		//private ConnectionManager conMgr;
		// private UserManager userMan;
		// private ReminderManager remMan;
		// private AdManager adMan;

		// private BotDaemon botD(botName,conMgr,userMgr,remMan,adMan);

		//public Output Log;
		public bool AppInit = false;
		private bool bAppQuit = false;
		
		//private bool bAutoStart = false;

        RMConsoleCommands _commands = null;


		public App(string[] args)
		{
            try
            {
                Arguments CommandLine = new Arguments(args);

                Assembly a = Assembly.GetExecutingAssembly();
                AssemblyName name = a.GetName();
                xConsole.Title = "RemindMe Server Version " + name.Version;

                //Log = new Output(CommandLine["b"]);
                Log.Instance.WriteString(xConsole.Title + "\r\n", xCon.ConsoleColor.SkyBlueForte, true);

                if (!File.Exists(CommandLine["xmlFile"]))
                {
                    Log.Instance.WriteError("xmlFile:(" + CommandLine["xmlFile"] + ") file not found.");
                    return;
                }
                else if (!File.Exists(CommandLine["mpserver"]))
                {
                    Log.Instance.WriteError("mpserver:(" + CommandLine["mpserver"] + ") file not found.");
                    return;
                }
                else if (!File.Exists(CommandLine["config"]))
                {
                    Log.Instance.WriteError("config:(" + CommandLine["config"] + ") file not found.");
                    return;
                }

                ConfigFactory configFactory = new XMLConfigFactory(CommandLine["config"], true);
                Log.Instance.BotName = configFactory.GetBotName();

                Log.Instance.WriteLine("CommandLine: {0}", string.Join(" ", args));
                Log.Instance.WriteLine("Config File: {0}", CommandLine[@"config"]);

                RemBot = new BotDaemon(configFactory); 
                _commands = new RMConsoleCommands(RemBot);
                AppInit = RemBot.InitializeBot(CommandLine["xmlFile"], CommandLine["mpserver"], CommandLine["autostart"]);
            }
            catch (Exception ex)
            {
                Log.Instance.WriteConsoleError("Startup Error", ex);
            }
		}

		public void MainLoop()
		{
			//Application.AddMessageFilter(Log);
			//Application.Run();

			string strInput;
			
			do
			{
				strInput = 	System.Console.ReadLine();
				
				//TODO: regex implementation and better delegation
				if (strInput.Length == 0)
					continue;


                if (strInput.ToLower() == @"quit" || strInput.ToLower() == @"exit")
                    bAppQuit = true;
               else
                    _commands.ExecuteCommand(strInput);
					
			} while (!bAppQuit);

            Environment.Exit(0);
		}

	}





	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	/// 
	class Class1
	{
        static string logConfigXml = @"
<log4net>
<appender name='full_log' type='log4net.Appender.RollingFileAppender'>
    <file value='remserver.txt' />
    <appendToFile value='true' />
    <rollingStyle value='Once' />
    <maxSizeRollBackups value='2' />
    <layout type='log4net.Layout.PatternLayout'>
        <conversionPattern value='%9timestamp %date{yyyy/MM/dd HH:mm:ss.fff} [%thread] %-5level %logger - %message%newline' />
    </layout>
</appender>

<appender name='A1' type='log4net.Appender.ConsoleAppender'>
    <layout type='log4net.Layout.PatternLayout'>
        <conversionPattern value='%date{yyyy-MM-dd HH:mm:ss} %message%newline' />
    </layout>
</appender>
<root>
        <level value='DEBUG' />
        <appender-ref ref='full_log' />
        <appender-ref ref='A1' />
</root>
</log4net>
           ";

        static ILog log = LogManager.GetLogger(typeof(Class1));

		[DllImport("kernel32.dll", SetLastError=true)] static extern int AllocConsole ();
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			AllocConsole();

            XmlDocument logConfigDocument = new XmlDocument();
            logConfigDocument.LoadXml(logConfigXml);
            log4net.Config.XmlConfigurator.Configure(logConfigDocument.DocumentElement);

            log.Info("Starting RemServer");

			App MainApp = new App(args);

            try
            {
                if (MainApp.AppInit)
                    MainApp.MainLoop();
                else
                {
                    //#if DEBUG
                    System.Console.ReadLine();
                    //#endif
                }
            }
            catch (Exception e)
            {
                log.Debug("Final Catch", e);
            }
		}
	}
}
