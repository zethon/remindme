using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Resources;

namespace server
{
    class RMConsoleCommands
    {
        private class RMCommandMethodAttribute : System.Attribute
        {
            private string _name = string.Empty;
            public string Name
            {
                get { return _name; }
            }

            private string _summary = string.Empty;
            public string Summary
            {
                get { return _summary; }
            }

            private string _details = string.Empty;
            public string Details
            {
                get { return _details; }
            }

            public RMCommandMethodAttribute(string strName, string strSummary, string strDetails)
            {
                _name = strName;
                _summary = strSummary;
                _details = strDetails;
            }

            public RMCommandMethodAttribute(string strName, string strSummary)
            {
                _name = strName;
                _summary = strSummary;
            }
        }

        private BotDaemon _bot = null;
        private Connection _currentConnnection = null;

        public RMConsoleCommands(BotDaemon bot)
        {
            _bot = bot;
        }

#if DEBUG
        public void CreateDataManager(RMCommandParser parser)
        {
            try
            {
                DataManager tempData = _bot.ConfigFactoryObj.CreateDataManager();

                //ConfigFactory cfgFactory = new XMLConfigFactory(parser.Parameters[0], true);

            }
            catch (Exception ex)
            {
                Log.Instance.WriteConsoleError(@"Exception", ex);
            }
        }
#endif

        [RMCommandMethod("setconnection", "sets the sessions connection object to the service of the passed type")]
        public void SetConnection(RMCommandParser parser)
        {
            try
            {
                ConnectionType ct = (ConnectionType)Enum.Parse(typeof(ConnectionType), parser.Parameters[0], true);
                Connection conn = _bot.connectionManager.GetConnection(ct);
                if (conn != null)
                {
                    _currentConnnection = conn;
                    Log.Instance.WriteConsoleLine("Current Connection: {0}", _currentConnnection.GetType().ToString());
                }
                else
                {
                    Log.Instance.WriteConsoleLine("Connection type not found: {0}", parser.Parameters[0]);
                }
            }
            catch (Exception e)
            {
                Log.Instance.WriteConsoleLine("Could not load connection type: {0}", parser.Parameters[0]);
            }
        }

        [RMCommandMethod("ShowConnection", "sets the sessions connection object to the service of the passed type")]
        public void ShowConnection()
        {
            if (_currentConnnection != null)
            {
                Log.Instance.WriteConsoleLine("Current Connection:");
                Log.Instance.WriteConsoleLine("{0} {1} ({2})", _currentConnnection.GetType().ToString(), _currentConnnection.m_type.ToString(), _currentConnnection.Status.ToString());
            }
            else
            {
                Log.Instance.WriteConsoleLine("No connection set.");
            }
        }

        [RMCommandMethod("sendconnection", "USE AT YOUR OWN RISK")]
        public void SendConnection(RMCommandParser parser)
        {
            if (_currentConnnection != null)
                _currentConnnection.SendServerData(parser.WorkingString);
            else
                Log.Instance.WriteConsoleLine("No connection set.");
        }


        [RMCommandMethod("break", "break into the debugger")]
        public void Break()
        {
            System.Diagnostics.Debugger.Break();
        }

        [RMCommandMethod("dbcalls", "prints the total number of queries made to the datastore")]
        public void DbCalls()
        {
            Log.Instance.WriteConsoleLine("Total number of database calls: " + _bot.DataManagerObj.NumberOfCalls);
        }

        #region DBManager Functions
        public void GetReminder(RMCommandParser parser)
        {
            if (parser.Parameters.Length > 0)
            {
                Reminder rem = _bot.DataManagerObj.GetReminder(int.Parse(parser.Parameters[0]));

                Log.Instance.WriteConsoleLine("ReminderID: " + rem.ID);
                Log.Instance.WriteConsoleLine("UserID: " + rem.UserID);
                Log.Instance.WriteConsoleLine("Message: " + rem.Message);
                Log.Instance.WriteConsoleLine("(Server) Delivery Time: " + rem.ServerDeliveryTimeString);
            }
        }

        [RMCommandMethod("getuser", "(id) shows user info ")]
        public void GetUser(RMCommandParser parser)
        {
            if (parser.Parameters.Length > 0)
            {
                User user = _bot.DataManagerObj.GetUser(int.Parse(parser.Parameters[0].ToLower()));
                if (user != null)
                {
                    Log.Instance.WriteConsoleLine("Userid: " + user.UserID);
                    Log.Instance.WriteConsoleLine("Username: " + user.Username);
                    Log.Instance.WriteConsoleLine("Email: " + user.Email);
                }
                else
                    Log.Instance.WriteConsoleError("Unknown user");
            }
        }

        public void GetUserReminderCount(RMCommandParser parser)
        {
            if (parser.Parameters.Length > 0)
            {
                int iCount = 0;

                if (parser.Parameters.Length > 1 && parser.Parameters[1].ToLower() == @"p2p")
                    iCount = _bot.DataManagerObj.GetUserPTPReminderCount(int.Parse(parser.Parameters[0].ToLower()));
                else
                    iCount = _bot.DataManagerObj.GetUserReminderCount(int.Parse(parser.Parameters[0].ToLower()));

                Log.Instance.WriteConsoleLine("User: " + parser.Parameters[0]);
                Log.Instance.WriteConsoleLine("Reminders: " + iCount.ToString());
            }
        }

        public void GetUsersReminders(RMCommandParser parser)
        {
            if (parser.Parameters.Length > 0)
            {
                ArrayList reminders = _bot.DataManagerObj.GetUsersReminders(int.Parse(parser.Parameters[0].ToLower()));
                Log.Instance.WriteConsoleLine("Total Reminders:"+reminders.Count.ToString());
            }
        }

        public void GetLastDeliveredReminder(RMCommandParser parser)
        {
            if (parser.Parameters.Length > 0)
            {
                Reminder rem = _bot.DataManagerObj.GetLastDeliveredReminder(int.Parse(parser.Parameters[0].ToLower()));
                Log.Instance.WriteConsoleLine("ReminderID: " + rem.ID);
                Log.Instance.WriteConsoleLine("UserID: " + rem.UserID);
                Log.Instance.WriteConsoleLine("Message: " + rem.Message);
                Log.Instance.WriteConsoleLine("(Server) Delivery Time: " + rem.ServerDeliveryTimeString);
            }
        }
        #endregion

        [RMCommandMethod("riq","\tlists reminders in queue")]
        public void RIQ()
        {
            Log.Instance.WriteConsoleLine("\r\nREMINDERS IN QUEUE:", xCon.ConsoleColor.PurpleForte, true);
            Log.Instance.WriteConsoleLine("------------------------------------------", true);
            foreach (Reminder rem in _bot.reminderManager.ReminderQueue)
            {
                Log.Instance.WriteConsoleLine("Reminder ID: {0}", rem.ID.ToString());
                Log.Instance.WriteConsoleLine("User ID: {0}", rem.UserID);
                Log.Instance.WriteConsoleLine("Servertime: {0}", rem.ServerDeliveryTimeString);
                Log.Instance.WriteConsoleLine("Message: {0}", rem.Message);
                Log.Instance.WriteConsoleLine("---", true);
            }
        }

        [RMCommandMethod("list", "\tlist users - list all users registered with this bot\r\n\tlist users online - list all users online\r\n\tlist users [aim|msn|icq|etc...] - listt users of a service")]
        public void List(RMCommandParser parser)
        {
            if (parser.Parameters.Length > 0 && parser.Parameters[0].ToLower() == @"users")
            {
                // list users
                if (parser.Parameters.Length == 1)
                {
                    Log.Instance.WriteString("\r\nALL USERS:", xCon.ConsoleColor.PurpleForte, true);
                    Log.Instance.WriteStatus("------------------------------------------", true);

                    foreach (User user in _bot.userManager.Users)
                    {
                        string strOutPut = string.Format("{0}{1}{2}{3}",
                            user.UserID.ToString().PadRight(6),
                            user.Username.PadRight(20),
                            user.Email.PadRight(40),
                            user.TimeZone.PadRight(10));

                        Log.Instance.WriteString(strOutPut, xCon.ConsoleColor.White, true);
                    }

                    Log.Instance.WriteString("\r\n", xCon.ConsoleColor.White, true);
                }
                else if (parser.Parameters.Length > 1)
                {
                    switch (parser.Parameters[1])
                    {
                        case @"online":
                            Log.Instance.WriteString("\r\nUSERS SIGNED ON NOW:", xCon.ConsoleColor.PurpleForte, true);
                            Log.Instance.WriteStatus("------------------------------------------", true);
                            Log.Instance.WriteStatus("UserID".PadRight(20) + "Service".PadRight(30) + "ScreenName\r\n", true);
                            int iCount = 0;

                            foreach (User user in _bot.userManager.Users)
                            {
                                string[] strScreenNames = new string[user.Contacts.Count];
                                ArrayList scrnams = new ArrayList();


                                foreach (IMContact im in user.Contacts)
                                {
                                    Connection conn = _bot.connectionManager.GetConnection(im.ConnectionType);

                                    if (conn == null || conn.Status == ConnectionStatus.Offline ||
                                        !conn.IsBuddyOnline(im.UserName))
                                        continue;

                                    string strOutPut = string.Format("{0}{1}{2}",
                                        user.UserID.ToString().PadRight(5),
                                        im.ConnectionType.ToString().PadRight(30),
                                        im.UserName.PadRight(20));

                                    Log.Instance.WriteString(strOutPut, xCon.ConsoleColor.White, true);
                                    iCount++;
                                }
                            }

                            Log.Instance.WriteString("TOTAL: " + iCount.ToString() + "\r\n", xCon.ConsoleColor.White, true);
                            //Log.Instance.WriteString("\r\n",ConsoleColor.White,true);
                    break;

                    case @"aim":
                    case @"jabber":
                    case @"email":
                    case @"yahoo":
                    case @"icq":
                        ConnectionType ct = (ConnectionType)Enum.Parse(typeof(ConnectionType), parser.Parameters[1], true);

                        Log.Instance.WriteString("\r\n" + parser.Parameters[1].ToUpper() + " USERS:", xCon.ConsoleColor.PurpleForte, true);
                        Log.Instance.WriteStatus("------------------------------------------", true);

                        foreach (User user in _bot.userManager.Users)
                        {
                            foreach (IMContact im in user.Contacts)
                            {
                                if (im.ConnectionType == ct && im.UserName.Length >= 3)
                                {
                                    string strOutPut = string.Format("{0}{1}{2}",
                                        user.UserID.ToString().PadRight(5),
                                        user.Email.PadRight(40),
                                        im.UserName.PadRight(10));

                                    Log.Instance.WriteString(strOutPut, xCon.ConsoleColor.White, true);

                                }
                            }
                        }

                        Log.Instance.WriteString("\r\n", xCon.ConsoleColor.White, true);

                    break;

                    default:
                    break;
                    }
                }
            }
            else if (parser.Parameters.Length > 0 && parser.Parameters[0].ToLower() == @"connections")
            {
                ListConnections();
            }


        }

        private void ListConnections()
        {
            int iCount = 0;

            IDictionaryEnumerator enumr = _bot.connectionManager.Connections.GetEnumerator();
            while (enumr.MoveNext())
            {
                Connection conn = enumr.Value as Connection;
                if (conn != null)
                {
                    Log.Instance.WriteConsoleLine("{0} {1} ({2}) {3}", conn.m_strLogin, conn.GetType().ToString(), conn.m_type.ToString(), conn.Status.ToString());
                }

            }
        }

        [RMCommandMethod("load", "load 'reminders' or 'users'")]
        public void Load(RMCommandParser parser)
        {
            if (parser.Parameters[0].ToLower() == @"users")
                _bot.LoadUsers(null, null);
            else if (parser.Parameters[0].ToLower() == @"reminders")
                _bot.LoadReminders(null, null);
        }

        [RMCommandMethod("startupall", "starts all services of the bot")]
        public void StartUpAll()
        {
            _bot.StartupAll();
        }

        [RMCommandMethod("bothelp", "emulates the 'help' command sent to the bot")]
        public void BotHelp(RMCommandParser parser)
        {
            string strHelp = @"/help " + parser.WorkingString;

            Log.Instance.WriteStatus("------------------------------------------", true);
            Log.Instance.WriteString(_bot.helpManager.GetResponse(strHelp), xCon.ConsoleColor.White, true);
            Log.Instance.WriteStatus("------------------------------------------", true);
        }


        [RMCommandMethod("getad", "getst the next ad queued for a service")]
        public void GetAd(RMCommandParser parser)
        {
            ConnectionType ct = (ConnectionType)Enum.Parse(typeof(ConnectionType), parser.Parameters[0], true);

            string strAdTxt = _bot.adManager.GetNextAdText(null, ct);
            Log.Instance.WriteStatus("------------------------------------------", true);
            Log.Instance.WriteString(strAdTxt, xCon.ConsoleColor.White, true);
            Log.Instance.WriteStatus("------------------------------------------", true);
        }

        [RMCommandMethod("reset", "disconnect and then reconnect a service")]
        public void Reset(RMCommandParser parser)
        {
            ConnectionType ct = (ConnectionType)Enum.Parse(typeof(ConnectionType), parser.Parameters[0], true);

            Connection conn = _bot.connectionManager.GetConnection(ct);
            if (conn != null)
            {
                conn.Disconnect();
                Thread.Sleep(1000);
                conn.Connect();
            }
        }

        [RMCommandMethod("printres", "print a resource of a passed number")]
        public void PrintRes(RMCommandParser parser)
        {
            ResourceManager rm = _bot.resourceMgr;
            string strHelp = rm.GetString(parser.Parameters[0]);
            Log.Instance.WriteStatus(strHelp == null ? "(null)" : strHelp);
        }

        [RMCommandMethod("help", "this is it")]
        public void Help()
        {
            Type t = this.GetType();

            foreach (MethodInfo info in t.GetMethods())
            {
                object[] obs = info.GetCustomAttributes(false);

                foreach (object o in obs)
                {
                    RMCommandMethodAttribute rcma = o as RMCommandMethodAttribute;

                    if (rcma != null)
                        Log.Instance.WriteConsoleLine("{0} - {1}", rcma.Name, rcma.Summary);
                        //_output.WriteLine("{0} - {1}", rcma.Name, rcma.Summary);
                }

            }
        }


        public void ExecuteCommand(string strCommand)
        {
            RMCommandParser parser = new RMCommandParser(strCommand, this);
            parser.Parse();

            MethodInfo mi = this.GetType().GetMethod(parser.ApplicationName, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);

            if (mi != null)
            {
                try
                {
                    if (mi.GetParameters().Length == 0)
                    {
                        mi.Invoke(this, null);
                    }
                    else
                    {
                        object[] oparams = new object[mi.GetParameters().Length];

                        foreach (ParameterInfo m in mi.GetParameters())
                        {
                            if (m.ParameterType == typeof(string))
                                oparams[oparams.Length - 1] = parser.Parameters[oparams.Length - 1];
                            else if (m.ParameterType == typeof(RMCommandParser))
                                oparams[oparams.Length - 1] = parser;
                        }

                        if (oparams != null)
                            mi.Invoke(this, oparams);

                    }
                }
                catch (Exception e)
                {
                    //Log.Instance.WriteConsoleLine("Execute Command Execption: ", e.Message);
                    Log.Instance.WriteConsoleError("Execute Command Exception", e);
                }
            }
            else
            {
                Log.Instance.WriteConsoleLine("Unknown command.");
            }

        }

    }
}
