using System;
using System.Xml;
using System.Timers;
using System.Collections;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using xCon;
using MsgParser;
using Axosoft.Common.Utilities;
using AIMLBot;

namespace server
{
	/// <summary>
	/// Summary description for BotDaemon.
	/// </summary>
	public class BotDaemon
	{
		private string m_strBotName;
        public string BotName { get { return m_strBotName; } }

		private Hashtable m_imers;
		private Hashtable m_fuers;

		private System.Timers.Timer userManTimer;
		private int userManTimerInterval = 29; // minutes

		private System.Timers.Timer reminderManTimer;
		private int reminderManTimerInterval = 31; // minutes

		private MessageParser msgParser;

		private bool m_bLoadingUsers = false;
		private bool m_bLoadingReminders = false;

        private cBot _aliceBot;
        private bool _aliceBotReady = false;

		private ConnectionManager conMgr;
		public ConnectionManager connectionManager
		{
			get { return conMgr; }
		}

		private UserManager userMgr;
		public UserManager userManager
		{
			get { return userMgr; }
		}

		private ReminderManager reminderMgr;
		public ReminderManager reminderManager
		{
			get { return reminderMgr; }
		}

        private DataManager _dataManager;
        public DataManager DataManagerObj { get { return _dataManager; } }

		private AdManager adMgr;
		public AdManager adManager
		{
			get { return adMgr; }
		}
		
		private HelpManager helpMgr;
		public HelpManager helpManager
		{
			get { return helpMgr; }
		}

        private ConfigFactory _configFactory;
        public ConfigFactory ConfigFactoryObj { get { return _configFactory; } }

		public BotDaemon(ConfigFactory configFactory)
		{
            _configFactory = configFactory;

			userMgr = new UserManager();
            _dataManager = configFactory.CreateDataManager();
			//reminderMgr = new ReminderManager();

			m_imers = new Hashtable();
			m_fuers = new Hashtable();

            // TODO: this is bad design, need to rearchitec the concept and code for the botName. is it still needed?
            m_strBotName = Log.Instance.BotName;
		}


		ResourceManager m_rm;
		public ResourceManager resourceMgr { get { return m_rm; }}


		public bool InitializeBot(string xmlFile, string mpserverConf, string strStartAll)
		{
			// create reminder mgr
            reminderMgr = new ReminderManager(m_strBotName);
            xConsole.Title += " (" + m_strBotName + ")";

			// initialize the message parser
			string strMPPort;
			string strMPPath;

			Log.Instance.WriteStatus("Initializing MessageParser Server...");
			try 
			{
				XmlDocument mpDoc = new XmlDocument();
				mpDoc.Load(mpserverConf);
			
				XmlNode mpRoot = mpDoc.DocumentElement;
				strMPPort = mpRoot.SelectSingleNode("//port").InnerText;
				strMPPath = mpRoot.SelectSingleNode("//path").InnerText;
			}
			catch (Exception e)
			{
				Log.Instance.WriteError("Failed to read mpserver configuration: "+e.Message);	
				return false;
			}

			if (strMPPort.Length == 0 || strMPPath.Length == 0)
			{
				Log.Instance.WriteError("Invalid port or path setting");	
				return false;
			}

			msgParser = new MessageParser(strMPPort,strMPPath);
			if (!msgParser.PingServer())
			{
				Log.Instance.WriteError("Failed to ping mpserver");	
				return false;
			}

			Log.Instance.WriteStatus("mpserver found at port:"+strMPPort+" path:"+strMPPath);

            // resource manager
			m_rm = new ResourceManager("server.botMsgs", this.GetType().Assembly); 

            // initialize the datamanager
            adMgr = new AdManager(DataManagerObj);

			XmlDocument doc = new XmlDocument();
			doc.Load(xmlFile);

			if (doc == null)
			{
				Log.Instance.WriteError("XML Parser Error");
				return false;
			}
			
			XmlElement root = doc.DocumentElement;
            XmlNode botNode = root.SelectSingleNode("//bot[@name=\"" + m_strBotName + "\"]");

			if (botNode == null)
			{
				Log.Instance.WriteError("Bot not found in XML file");
				return false;
			}

			if (!InitializeConMgr(botNode.CloneNode(true)))
				return false;

			if (!InitializeUserManager())
				return false;
			
			if (!InitializeReminderManager())
				return false;

			// initialize the help manager
			helpMgr = new HelpManager(@"help.xml");

//#if !DEBUG 
            Thread t = new Thread(new ThreadStart(LoadAliceBot));
            t.Start();

//            Log.Instance.WriteLine("Initializing Alice Bot");
//            DateTime start = DateTime.Now;
//            _aliceBot = new cBot(false);
//            DateTime stop = DateTime.Now;

//            TimeSpan span = stop - start;
//            Log.Instance.WriteLine("Bot initialization took {0} sec", span.TotalSeconds);
//#endif

			reminderMgr.OnReminder += new ReminderManager.OnReminderHandler(OnDeliverReminder);
			
			if (strStartAll != null)
				StartupAll();

			return true;
		}

        public void LoadAliceBot()
        {
            Log.Instance.WriteLine("Initializing AliceBot");
            DateTime start = DateTime.Now;
            _aliceBot = new cBot(false);
            TimeSpan span = DateTime.Now - start;

            Log.Instance.WriteLine("AliceBot initialization took {0} sec", span.TotalSeconds);
            _aliceBotReady = true;
        }

		public bool InitializeConMgr(XmlNode botNode)
		{
			if (conMgr != null)
				return true;

			Log.Instance.WriteStatus("Initializing Connection Manager...");
			ConMgrFactory conFact = new ConMgrFactory();
			conMgr = conFact.MakeManager(botNode);

			if (conMgr == null)
			{
				Log.Instance.WriteError("Connection Manager failed to initialize");	
				return false;
			}

			Array oa = Enum.GetValues(typeof(ConnectionType));
			foreach (ConnectionType ct in oa)
			{
				Connection conn = conMgr.GetConnection(ct);
				
				if (conn == null)
					continue;

				conn.OnMessage += new Connection.OnMessageHandler(MessageHandler);
				conn.OnSignedOn += new Connection.OnSignedOnHandler(SignedOnHandler);
				conn.OnSendMessage += new Connection.OnSendMessageHanlder(SendMessageHandler);
				conn.OnUpdateBuddy += new Connection.OnUpdateBuddyHanlder(OnUpdateBuddy);
				conn.OnError += new Connection.OnErrorHandler(ErrorHandler);
				conn.OnDisconnect += new Connection.OnDisconnectHandler(DisconnectHandler);
				conn.OnReconnect += new server.Connection.OnReconnectHandler(ReconnectHandler);
			}

			Log.Instance.WriteStatus("Connections loaded successfully.");	
			return true;
		}

		public bool InitializeUserManager()
		{
			//			if (userMgr != null)
			//				return true;

			userManTimer = new  System.Timers.Timer((1000*60)*userManTimerInterval);
			userManTimer.Elapsed += new System.Timers.ElapsedEventHandler(LoadUsers);

			LoadUsers(null,null);
			return userMgr != null;
		}


		public bool InitializeReminderManager()
		{
			reminderManTimer = new System.Timers.Timer((1000*60)*reminderManTimerInterval);
			reminderManTimer.Elapsed += new System.Timers.ElapsedEventHandler(LoadReminders);

			LoadReminders(null,null);
			return reminderMgr != null;
		}

		// connects to the services and starts the timers
		public void StartupAll()
		{
			Log.Instance.WriteStatus("Connecting all IM services...");
			conMgr.ConnectAll();
			Log.Instance.WriteStatus("Starting UserManager timer...");
			userManTimer.Start();
			Log.Instance.WriteStatus("Starting ReminderManager timer...");
			reminderManTimer.Start(); // the timer that refreshes the reminders in the db
			reminderMgr.Start(); // start the timer that delivers reminders
		}

		#region DelegateHandlers

		public void ReconnectHandler(Connection conn)
		{
			Log.Instance.WriteError(conn.m_type.ToString()+" is reconnecting...");
		}
		
		public void ErrorHandler(Connection conn,string strError)
		{
			Log.Instance.WriteError("ERROR ("+conn.m_type.ToString()+") >> "+strError);
		}


		public void MessageHandler(Connection conn,InstantMessage im)
		{
			Log.Instance.WriteStatus("INMSG ("+conn.m_type.ToString()+") << "+im.User+" : "+im.Text);
			
			string strUniqueID = im.User + conn.m_type.ToString();

			if (m_bLoadingUsers || m_bLoadingReminders)
			{
				conn.SendMessage(im.User,"RemindMe is performing maintenance. Please try again in a few minutes. If the problem persists, please contact support@remindme.cc");

                // TODO: fiond out why the bot gets stuck with one of these two being true all the time
                Log.Instance.WriteLine("m_bLoadingUsers = {0} , m_bLoadingReminders = {1}", m_bLoadingUsers.ToString(), m_bLoadingReminders.ToString());
				return;
			}

			User user = userMgr.GetUserByService(conn.m_type,im.User);

			// unknown user? Then get out ohere quickly
			if (user == null)
			{
				// we don't want to send email back to spammers
				if ((m_imers[strUniqueID] == null || (int)m_imers[strUniqueID] < 1))// && conn.m_type != ConnectionType.EMAIL)
					conn.SendMessage(im.User,m_rm.GetString("unknown_user")+adMgr.GetNextAdText(null,conn.m_type));

				// protect against spamming the bot, max of 1 im
				if (m_imers[strUniqueID] == null)
					m_imers[strUniqueID] = 1;
				else
					m_imers[strUniqueID] = (int)m_imers[strUniqueID] + 1; 

				return;
			}

			if ((im.Text.ToLower() == @"/reset aim" || im.Text.ToLower() == @"/reset icq" || 
				im.Text.ToLower() == @"/reset yahoo" || im.Text.ToLower() == @"/reset msn") 
				&& user.Class == UserClassType.ADMIN)
			{
				string [] strList = im.Text.Split(' ');
				ConnectionType ct = (ConnectionType)Enum.Parse(typeof(ConnectionType),strList[1],true);

				Connection resetcon = conMgr.GetConnection(ct);
				if (resetcon != null)
				{
					resetcon.Disconnect();
					Thread.Sleep(1000);
					resetcon.Connect();		
				}

				return;
			}

			if (im.Text.ToLower() == @"/whoami")
			{
				conn.SendMessage(im.User,"\r\nUser: "+user.Username + "(" + user.UserID + ")" +
									"\r\nClass: "+user.Class.ToString()+
									"\r\nEmail: "+user.Email+
									"\r\nPlanNumber: "+user.PlanNumber);
				return;
			}

            Regex reg = new Regex(@"^fuck\s+you", RegexOptions.IgnoreCase);
            Match m = reg.Match(im.Text);
            if (m.Success)
            {
                conn.SendMessage(im.User, "That's really not nice!");
                m_fuers[im.User] = 1;
                return;
            }
			
			// /help commands
			reg = new Regex(@"^[\/\-\:]+help\s*[\w\s]*$",RegexOptions.IgnoreCase);
			m = reg.Match(im.Text);
			if (m.Success)
			{
				string strResponse = helpMgr.GetResponse(im.Text);

				if (strResponse.Length > 1)
					conn.SendMessage(im.User,strResponse);

				return;
			}

			// /list
			reg = new Regex(@"^\/list",RegexOptions.IgnoreCase);
			m = reg.Match(im.Text);
			if (m.Success)
			{
				string strResponse = "\nCurrent Pending Reminders:\n";

                foreach (Reminder aRem in DataManagerObj.GetUsersReminders(user.UserID))
					strResponse += aRem.ID+"   "+aRem.UserTimeString+"\n";			
				
				conn.SendMessage(im.User,strResponse);
				return;
			}
			
			// /cancel 1234 -- show a reminder
			reg = new Regex(@"^/cancel\s+(\d+)\s*$",RegexOptions.IgnoreCase);
			m = reg.Match(im.Text);
			if (m.Success)
			{
				string remId = m.Groups[1].Captures[0].ToString();
                int iRemID = int.Parse(remId);
                Reminder aRem = DataManagerObj.GetReminder(iRemID);

				if (aRem != null && aRem.UserID == user.UserID)
				{
					aRem.Delivered = true;
                    DataManagerObj.SaveReminder(aRem);

					string strResponse = m_rm.GetString("cancel_reminder_conf");
					strResponse = strResponse.Replace("%d",aRem.ID);

					conn.SendMessage(im.User,strResponse);
				}

				return;
			}

			// /show 1234 - display the details of reminder 1234
			reg = new Regex(@"^/show\s+(\d+)\s*$",RegexOptions.IgnoreCase);
			m = reg.Match(im.Text);
			if (m.Success)
			{
				string remId = m.Groups[1].Captures[0].ToString();
                int iRemID = int.Parse(remId);
                Reminder aRem = DataManagerObj.GetReminder(iRemID);

				if (aRem != null && aRem.UserID == user.UserID)
				{
					string strResponse = m_rm.GetString(@"reminder_info_IM");
                    User creatorObj = _dataManager.GetUser(aRem.CreatorID);

					strResponse = strResponse.Replace("%d",aRem.ID.ToString());
					strResponse = strResponse.Replace("%a",creatorObj.Username);
					strResponse = strResponse.Replace("%b",aRem.UserTimeString);
					strResponse = strResponse.Replace("%c",aRem.Message);

					conn.SendMessage(im.User,strResponse);
				}
				return;
			}

			// remind me .......
			reg = new Regex(@"^remind\s+\w+\s+\w+",RegexOptions.IgnoreCase);
			m = reg.Match(im.Text);

			if (m.Success)
			{
				ProcessParse(conn,im,user,@"creation_parse");
				return;
			}
			
			// in 20 minutes remind me to transfer
			reg = new Regex(@"^.*?\s+remind\s+[\w_,]+\s+.*",RegexOptions.IgnoreCase);
			m = reg.Match(im.Text);

			if (m.Success)
			{
				ProcessParse(conn,im,user,@"creation_parse");
				return;
			}

			reg = new Regex(@"^repeat\s+\w+",RegexOptions.IgnoreCase);
			m = reg.Match(im.Text);

			if (m.Success)
			{
				ProcessParse(conn,im,user,@"repeat_parse");
				return;
			}

            if (user != null && conn.m_type != ConnectionType.EMAIL)
            {
                if (_aliceBotReady)
                {
                    cResponse reply = _aliceBot.chat(im.Text, user.Username);
                    conn.SendMessage(im.User, reply.getOutput());
                }
                else
                {
                    string strMsg = m_rm.GetString("known_user_default");
                    strMsg = strMsg.Replace("%u", user.Username);
                    conn.SendMessage(im.User, strMsg);
                }
            }

		}

		private bool ProcessParse(Connection conn,InstantMessage im, User user, string strAction)
		{
			bool bSendMessage = true;
			bool bRetVal = false; // assume failure
			string strMsg = "";

			if (!msgParser.SendParseRequest(im.Text,user.TimeZone,user.DLS,strAction))
			{
				strMsg = m_rm.GetString("mpserver_request_error"); //0x01
			}
			else if (!msgParser.ProcessResponse())
			{
				strMsg = m_rm.GetString("mpserver_response_error"); // 0x02
			}
			else
				switch (msgParser.ParserErrorCode)
				{
					case "1": 
						strMsg = m_rm.GetString("mpserver_parser_error_1");
					break;
					case "2":
						strMsg = m_rm.GetString("mpserver_parser_error_2");
					break;
					case "3":
						strMsg = m_rm.GetString("mpserver_parser_error_3");
					break;
					case "4":
						strMsg = m_rm.GetString("mpserver_parser_error_4");
					break;
					case "5":
						strMsg = m_rm.GetString("mpserver_parser_error_5");
					break;
					case "6":
						strMsg = m_rm.GetString("mpserver_parser_error_6");
					break;
					
					//case null:
					//	strMsg = "MPSERVER failure. If you are seeing this message, please report the problem to support@remindme.cc. Thank you.";
					//break;

					default: // no error -- "0" ??
						switch (strAction)
						{
							case @"creation_parse":
								bRetVal = TryCreateReminder(conn,im,user);
								bSendMessage = false;
							break;

							case @"repeat_parse":
								bRetVal =  TryRepeatReminder(conn,im,user);
								bSendMessage = false;
							break;

							default:
							break;
						}
					break;
				}

			if (bSendMessage)
				conn.SendMessage(im.User,strMsg);
			
			return bRetVal;
		}
		
		private bool TryCreateReminder(Connection conn, InstantMessage im, User user)
		{
			string strMsg = "";
			int iNewId = 0;
			
			try 
			{
                iNewId = reminderMgr.CreateReminder(DataManagerObj, user, msgParser, userMgr);

				strMsg = m_rm.GetString("reminder_scheduled");
				strMsg = strMsg.Replace("%d",msgParser.UserTimeString);
				strMsg = strMsg.Replace("%t",user.TimeZone);
				strMsg = strMsg.Replace("%r",iNewId.ToString());

                Log.Instance.WriteString("Reminder Created: ID (" + iNewId.ToString() + ") USER (" + user.UserID + ")", xCon.ConsoleColor.GreenForte, false);
				conn.SendMessage(im.User,strMsg+adMgr.GetNextAdText(user,conn.m_type));
				return true;
			}
			catch (ReminderException re)
			{
				strMsg = re.Message;
			}
			catch (Exception e)
			{
				Log.Instance.WriteError("Exception in TryCreateReminder: "+e.Message);
				strMsg = "There has been an unknown error.";
			}

			conn.SendMessage(im.User,strMsg);
			return false; // assume failure
		}

		private bool TryRepeatReminder(Connection conn, InstantMessage im, User user)
		{
            int iRemId = reminderMgr.RepeatReminder(DataManagerObj, user.UserID, msgParser);
			string strMsg;

			if (iRemId == -1)
			{
				strMsg = "No reminders have been recently delivered to you.";
			}
			else if (iRemId > 0)
			{
				strMsg = m_rm.GetString("reminder_scheduled");
				strMsg = strMsg.Replace("%d",msgParser.UserTimeString);
				strMsg = strMsg.Replace("%t",user.TimeZone);
				strMsg = strMsg.Replace("%r",iRemId.ToString());

                Log.Instance.WriteString("Reminder Repeat: ID (" + iRemId.ToString() + ") USER (" + user.UserID + ")", xCon.ConsoleColor.GreenForte, false);
				conn.SendMessage(im.User,strMsg);
				return true;
			}
			else 
				strMsg = m_rm.GetString("create_reminder_failed"); //0x03

			conn.SendMessage(im.User,strMsg);
			return false;
		}
		

		public void DisconnectHandler(Connection conn)
		{
			Log.Instance.WriteError(conn.m_type.ToString()+" disconnected...");
		}

		public void SignedOnHandler(Connection conn)
		{
			Log.Instance.WriteStatus(conn.m_type.ToString()+" connected....");

			// this was causing Yahoo to log off once it signed on
            //if (conn.m_type != ConnectionType.YAHOO)
            //{
            //    conn.m_BuddyList = userMgr.GetServiceNames(conn.m_type);
            //    Thread t = new Thread(new ThreadStart(conn.UpdateBuddyList));
            //    t.Start();
            //}
		}

		public void SendMessageHandler(Connection conn,InstantMessage im)
		{
			Log.Instance.WriteStatus("OUTMSG ("+conn.m_type.ToString()+") >> "+im.User+" : "+im.Text);
		}

		public void OnUpdateBuddy(Connection conn, BuddyStatus stat)
		{
			string strAction = stat.Online ? "Signed on" : "Signed Off";
			Log.Instance.WriteStatus("UPBUD ("+conn.m_type.ToString()+") << "+stat.Name+" : "+strAction);
		}

		private string ReminderMsgText(Reminder rem, User user, ConnectionType ct)
		{
			string retVal = "";

			// get the from user string
			string strFromString = "";

            // if this reminder was not created by the person it's being delivered to
            // then tag along who it is from
            if (rem.UserID != rem.CreatorID)
            {
                User creatorObj = DataManagerObj.GetUser(rem.CreatorID);
                strFromString = " from " + creatorObj.Username;            
            }

				

			switch (ct)
			{
				case ConnectionType.AIM:
					retVal = "Reminder (<a href=\"http://www.remindme.cc/viewreminder?remid="+rem.ID+"\">"+rem.ID+"</a>)"+strFromString+": "+rem.Message;
				break;

				case ConnectionType.EMAIL:
					retVal = "Reminder ("+rem.ID+")"+strFromString+": "+rem.Message;
					retVal += "\r\n\r\nThis email is an automated response to a message sent to RemindMe from your account. You can change your account settings by logging in at www.remindme.cc";
				break;

				default:
					retVal = "Reminder ("+rem.ID+")"+strFromString+": "+rem.Message;
				break;
			}

			return retVal+adMgr.GetNextAdText(user,ct);
		}

		public void OnDeliverReminder(Reminder rem)
		{
			// very smiliar to the perl server's functionality here
			// should change this to thread delivery to a deliverymanager object
			
			User user = userMgr.GetUserByID(rem.UserID);  
			
			if (user == null)
			{
                // TODO: not sure what I was doing with the code below, this should throw an error or something

                rem.Delivered = true;
                goto quit;

                //if (rem.Creator.ToLower().Trim() == "admin")
                //{
                //    rem.ServerDeliveryTime += new TimeSpan(1,0,0);
                //    DataManagerObj.SaveReminder(rem);
                //    goto quit;
                //}
                //else
                //{
                //    Log.Instance.WriteError("INTERNAL ERROR 1x01: GetUserByID returned null rem.User = ("+rem.UserID+")");
                //    rem.Delivered = true; // to prevent the error from reoccuring into infinity
                //    goto quit;
                //}
			}

			ArrayList userContacts = user.Contacts;

			TimeSpan ts = (TimeSpan)(DateTime.Now - rem.ServerDeliveryTime);
			if (ts.TotalHours >= 2 && !rem.Delivered)
				userContacts.Add(new IMContact(user.Email,ConnectionType.EMAIL,4)); // max number of users needs to globalized and const'd!

			foreach (IMContact ct in userContacts)
			{
				ct.UserName.Trim();
				if (conMgr.IsBuddyOnline(ct.ConnectionType,ct.UserName) && ct.UserName.Length > 0)
				{
					Connection cntn = conMgr.GetConnection(ct.ConnectionType);
					
					if (cntn == null)
					{
						Log.Instance.WriteError("INTERNAL ERROR 1x02: GetConnection return NULL cntn.m_type = ("+cntn.m_type.ToString()+")");
						rem.Delivered = true; // to prevent the error from reoccuring into infinity
						goto quit;
					}

					try
					{
						cntn.SendMessage(ct.UserName,ReminderMsgText(rem,user,ct.ConnectionType));
                        Log.Instance.WriteString("Reminder Reciept: ID (" + rem.ID + ") USER (" + user.Username + ")", xCon.ConsoleColor.GreenForte, false);
						
						rem.DeliveredName = ct.UserName;
						rem.DeliveredConnType = ct.ConnectionType;
						rem.Delivered = true;
					}
					catch(Exception ex)
					{
						Log.Instance.WriteError("SendEmail ERROR: "+ex.Message);
						rem.DeliveredName = "SendEmail ERROR";
						rem.Delivered = true; // // to prevent the error from reoccuring into infinity						
					}
					break;
				}
			}

			if (!rem.Delivered)
				rem.InDeliveryQue = false;
			else 
			{
				rem.DeliveredTime = System.DateTime.Now.ToString("u");
			}

			quit:{}

			// only save it to the db if it's been delivered
			if (rem.Delivered)
			{
				// check to see if the reminder contains a repeater
				if (rem.Repeater != null && !rem.Repeater.HasExpired())
				{
					RepeatPattern patt = new RepeatPattern(rem.Repeater.Pattern);
					DateTime UserNextTime = patt.GetNextDate(DateTime.ParseExact(rem.UserTimeString,@"yyyy-MM-dd HH:mm:ss", null));
					
					string userTimeString = UserNextTime.ToString("u");
					userTimeString = userTimeString.Replace("Z",null);
					if (msgParser.SendParseRequest(userTimeString,user.TimeZone,user.DLS,"getservertime") && msgParser.ProcessResponse())
					{
						string strTemp = msgParser.ServerTimeString;
						strTemp = strTemp.Replace("Z",null);

						rem.ServerDeliveryTimeString = strTemp;//msgParser.ServerTimeString;
						rem.UserTimeString = userTimeString;
						rem.Delivered = false;
					}

					//Log.Instance.WriteStatus("REPEATER: NEWUSERDATE IS ["+UserNextTime.ToString("u")+"]");
					//Log.Instance.WriteStatus("REPEATER: NEWSERVERDATE IS ["+msgParser.ServerTimeString+"]");
				}

                DataManagerObj.SaveReminder(rem);
			}

			// move below the if (rem.Delivered)...
			// save the change to the reminderQue
			reminderMgr.SaveReminderToQue(rem);
		}
		#endregion

		#region TimerHandlers
	
		public void LoadUsers(object sender,System.Timers.ElapsedEventArgs args)
		{
			if (m_bLoadingUsers)
				return;

			m_bLoadingUsers = true;

			try 
			{
				bool bRemTimerOn = reminderMgr.TimerOn;

				if (bRemTimerOn)
				{
					reminderMgr.Stop();
					Log.Instance.WriteStatus("LoadUsers: RemindManager stopped");
				}

				Log.Instance.WriteStatus("Loading users into the User Manager...");

                int iCount = userMgr.LoadUsers(DataManagerObj);
				if (userMgr == null)
				{
					Log.Instance.WriteError("User Manager failed to load.");
					m_bLoadingUsers = false; // bad, need to be careful not to get locked up
					return;
				}

				Log.Instance.WriteStatus("Total users loaded: "+iCount.ToString());
			
				// add users to the buddy lists if needed (and connected)
				IDictionaryEnumerator id = conMgr.GetConnectionEnumerator();
				while (id.MoveNext())
				{
					Connection conn = (Connection)id.Value;
					if (conn.Status == ConnectionStatus.Online)
					{
						conn.m_BuddyList = userMgr.GetServiceNames(conn.m_type);
						Thread t = new Thread(new ThreadStart(conn.UpdateBuddyList));
						
						Log.Instance.WriteStatus("UPDATING BUDDY LIST ("+conn.m_type.ToString()+")");
						t.Start();
					}
				}

				if (bRemTimerOn && iCount > 0)
				{
					reminderMgr.Start();
					Log.Instance.WriteStatus("LoadUsers: RemindManager started");
				}
				else if (bRemTimerOn && iCount == -1)
				{
					Log.Instance.WriteError("LoadUsers: UserManager returned -1");
					Log.Instance.WriteStatus("LoadUsers: RemindManager not started!");
				}
			}
			catch (Exception e)
			{
				Log.Instance.WriteError("LoadUsers() Exception: "+e.Message);
			}

			m_bLoadingUsers = false;
		}

		public void LoadReminders(object sender, System.Timers.ElapsedEventArgs args)
		{
			if (m_bLoadingReminders)
				return;

			m_bLoadingReminders = true;
			
			try
			{
				Log.Instance.WriteStatus("Loading reminders into the Reminder Manager...");
			
				bool bDoSupress = (sender == null && args == null);
                int iCount = reminderMgr.LoadReminders(DataManagerObj, bDoSupress);

				if (reminderMgr == null)
				{
					Log.Instance.WriteError("Reminder Manager failed to load.");
					return;
				}
				Log.Instance.WriteStatus("Total reminders loaded: "+iCount.ToString());
			}
			catch (Exception e)
			{
				Log.Instance.WriteError("LoadReminders() Exception: "+e.Message);
			}

			m_bLoadingReminders = false;
			
		}

		#endregion
	}


}
