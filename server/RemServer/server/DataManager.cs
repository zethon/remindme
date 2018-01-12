using System;
using MySQLDriverCS;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace server
{
    abstract public class DataManager
    {
        protected long _lCalls = 0;
        public long NumberOfCalls { get { return _lCalls; } }

        abstract public ArrayList LoadUsers();
        abstract public ArrayList GetReminders();
        abstract public ArrayList GetUsersReminders(int iUserID);

        abstract public Reminder GetReminder(int iRemID);
        abstract public Reminder GetLastDeliveredReminder(int iUserID);

        abstract public User GetUser(int iUserID);
        abstract public User GetUserByUsername(string strUsername);

        abstract public AdText GetNextAd();

        abstract public int GetUserReminderCount(int iUserID);
        abstract public int GetUserPTPReminderCount(int iUserID);
        
        abstract public int CreateReminder(ref Reminder rem, string strBotName);

        abstract public bool SaveReminder(Reminder rem);
        abstract public bool SaveReminderDeliveryInfo(Reminder rem);

        abstract public void SaveAd(AdHistory adHist);
    }

	/// <summary>
	/// Summary description for DBManager.
	/// </summary>
//    //public class DBManager : DataManager
//    {

//        MySQLConnection m_conn;
		
//        //public Output m_log = null;
//        public string strLastError = "";

//        protected string _BotName;
//        public Hashtable serviceTable = new Hashtable();

//        public DBManager(string strBotName)
//        {
//            //m_log = log;
//            _BotName = strBotName;
//            m_conn = new MySQLConnection(new MySQLConnectionString("216.189.0.240", "remindme_reminder", "nhbarmy_zethon", "").AsString);
//        }

//        private bool m_bWorking = false;

//        private void OpenDB(string strFuncName)
//        {
//            while (m_bWorking);
//            m_bWorking = true;

//            Log.Instance.WriteString("OpenDB: "+strFuncName,xCon.ConsoleColor.WhiteForte,false);

//            try
//            {
//                m_conn.Open();
//            }
//            catch(Exception e)
//            {
//                Log.Instance.WriteError("OpenDB Error: "+e.Message);
//                m_bWorking = false;
//            }
//        }

//        private void CloseDB(string strFuncName)
//        {
//            Log.Instance.WriteString("CloseDB: "+strFuncName,xCon.ConsoleColor.WhiteForte,false);

//            try
//            {
//                m_conn.Close();	
//            }
//            catch (Exception e)
//            {
//                Log.Instance.WriteError("CloseDB Error: "+e.Message);
//            }

//            m_bWorking = false;
//        }

//        override public AdText GetNextAd()
//        {
//            AdText adText = new AdText(0,"","");
			
//            try
//            {
//                OpenDB(@"GetNextAd()");

//                MySQLCommand cmd = new MySQLCommand(
//                    @"SELECT ad_info.id,ad_text.text,ad_text.html FROM ad_info 
//						LEFT JOIN ad_text ON ad_info.id=ad_text.id 
//						WHERE (ad_info.maxcount > ad_info.currentcount) AND (ad_info.delivered = 0) 
//						ORDER BY ad_info.id ASC 
//						LIMIT 1",m_conn);

////						@"SELECT ad_info.id,ad_text.text,ad_text.html FROM ad_info 
////						LEFT JOIN ad_text ON ad_info.id=ad_text.id 
////						LEFT JOIN ad_history ON ad_info.id=ad_history.id 
////						WHERE (ad_info.maxcount > ad_info.currentcount) AND (ad_info.delivered = 0) 
////						ORDER BY ad_history.datetime ASC 
////						LIMIT 1",m_conn);

		
//                IDataReader userRow = cmd.ExecuteReader();

//                if (!userRow.Read())
//                {
//                    // update all the rows and try again
//                    MySQLCommand cmd2 = new MySQLCommand(
//                        @"UPDATE ad_info SET delivered = 0",m_conn);
//                    cmd2.ExecuteNonQuery();

//                    cmd = new MySQLCommand(
//                        @"SELECT ad_info.id,ad_text.text,ad_text.html FROM ad_info 
//						LEFT JOIN ad_text ON ad_info.id=ad_text.id 
//						WHERE (ad_info.maxcount > ad_info.currentcount) AND (ad_info.delivered = 0) 
//						ORDER BY ad_info.id ASC 
//						LIMIT 1",m_conn);
//                    userRow = cmd.ExecuteReader();

//                    if (userRow.Read())
//                    {
//                        adText = new AdText(int.Parse(userRow["id"].ToString())
//                            ,userRow["text"].ToString()
//                            ,userRow["html"].ToString());
//                    }

//                }
//                else
//                {
//                    adText = new AdText(int.Parse(userRow["id"].ToString())
//                        ,userRow["text"].ToString()
//                        ,userRow["html"].ToString());
//                }
				
//                CloseDB(@"GetNextAd()");
//            }
//            catch (Exception ex)
//            {
//                Log.Instance.WriteError("GetNextAd() Error: "+ex.Message);
//            }

//            return adText;
//        }

//        override public void SaveAd(AdHistory adHist)
//        {
//            if (adHist.ID <= 0)
//                return;

//            try
//            {
//                OpenDB(@"SaveAd()");
////				new MySQLInsertCommand(m_conn,
////					new object[,] {{"id",adHist.ID},
////									{"datetime",adHist.TimeStamp.ToString("u")},
////									{"touser",adHist.UserName},
////									{"toconn",adHist.ConnType.ToString()}
////								  },
////					"ad_history");


//                MySQLCommand cmd = new MySQLCommand(
//                    @"SELECT currentcount FROM ad_info WHERE (id = '"+adHist.ID+"')",m_conn);
//                IDataReader userRow = cmd.ExecuteReader();
//                userRow.Read();
//                int iCount = int.Parse(userRow["currentcount"].ToString()) + 1;

//                new MySQLUpdateCommand(m_conn,
//                    new object[,] {	{"currentcount",iCount.ToString()},
//                                    {"delivered",1} },
//                    "ad_info",
//                    new object[,] {{"id","=",adHist.ID}},
//                    null);
				
//                CloseDB(@"SaveAd()");
//            }
//            catch (Exception ex)
//            {
//                Log.Instance.WriteError("SaveAd() Error: "+ex.Message);
//            }
//        }

//        #region subclassed functions
//        override public ArrayList LoadUsers()
//        {
//            ArrayList retUsers = new ArrayList();

//            try
//            {
//                OpenDB(@"LoadUsers()");

//                MySQLCommand cmd = new MySQLCommand(
//                    @"SELECT system_users.id,class,email,bot,timezone,DLS,plan,plan_name,num_reminders,num_ptp_reminders FROM system_users LEFT JOIN system_subscribe_plans ON system_subscribe_plans.id = system_users.plan WHERE bot = '" + _BotName + "'", m_conn);
//                IDataReader userRow = cmd.ExecuteReader();

//                while (userRow.Read())
//                {
//                    UserClassType uct = new UserClassType();
//                    uct = (UserClassType)Enum.Parse(typeof(UserClassType), userRow["class"].ToString(), true);

//                    User user = new User(
//                        int.Parse(userRow["userid"].ToString()),
//                        userRow["username"].ToString().ToLower().Trim(),
//                        userRow["email"].ToString().ToLower().Trim(),
//                        uct,
//                        userRow["timezone"].ToString(),
//                        int.Parse(userRow["DLS"].ToString()) == 1 ? true : false,
//                        int.Parse(userRow["plan"].ToString()),
//                        int.Parse(userRow["num_reminders"].ToString()),
//                        int.Parse(userRow["num_ptp_reminders"].ToString()),
//                        userRow["bot"].ToString());

//                    MySQLCommand contactcmd = new MySQLCommand(
//                        @"SELECT * FROM system_contacts WHERE (userid = '" + user.UserID + "') ORDER BY priority", m_conn);
//                    IDataReader contactreader = contactcmd.ExecuteReader();

//                    while (contactreader.Read())
//                    {
//                        user.AddContact(new IMContact(contactreader["login"].ToString(),
//                            (ConnectionType)Enum.Parse(typeof(ConnectionType), contactreader["service"].ToString(), true),
//                            int.Parse(contactreader["priority"].ToString())));
//                    }

//                    retUsers.Add(user);
//                }
//                CloseDB(@"LoadUsers()");
//            }
//            catch (Exception ex)
//            {
//                Log.Instance.WriteError("LoadUsers() Error: " + ex.Message);
//                return null;
//            }

//            return retUsers;
//        }

//        override public ArrayList GetReminders()
//        {
//            ArrayList retReminders = new ArrayList();

//            try
//            {
//                OpenDB(@"GetReminders()");
//                MySQLCommand cmd = new MySQLCommand(
//                    @"select * from system_reminders LEFT JOIN system_repeaters ON system_reminders.id = system_repeaters.remid where (bot = '" + _BotName + "' and delivered = 0) AND (UNIX_TIMESTAMP(servertime) - UNIX_TIMESTAMP(NOW()) < (60 * 60))", m_conn);
//                IDataReader reader = cmd.ExecuteReader();
//                CloseDB(@"GetReminders()");

//                while (reader.Read())
//                {
//                    string strPattern = "yyyy-MM-dd HH:mm:ss";
//                    DateTime datime;

//                    try
//                    {
//                        datime = DateTime.ParseExact(reader["servertime"].ToString(), strPattern, null);
//                    }
//                    catch (Exception e)
//                    {
//                        Log.Instance.WriteError("GetReminders() DateTime Parsing Error: " + e.Message);
//                        continue;
//                    }

//                    RepeaterClass newRepeater = null; //new RepeaterClass(null,null,0,false,null);
//                    if (reader["remid"].ToString() != "")
//                    {
//                        newRepeater = new RepeaterClass(reader["repid"].ToString(),
//                            reader["pattern"].ToString(),
//                            int.Parse(reader["count"].ToString()),
//                            reader["disabled"].ToString() != "0",
//                            reader["expiration"].ToString());
//                    }

//                    retReminders.Add(new Reminder(reader["id"].ToString(),
//                                                    reader["user"].ToString(),
//                                                    reader["creator"].ToString(),
//                                                    reader["msg"].ToString(),
//                                                    datime,
//                                                    reader["datetime"].ToString(),
//                                                    newRepeater));
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.Instance.WriteError("GetReminders() Error: " + ex.Message);
//                return null;
//            }

//            return retReminders;
//        }

//        override public int GetUserReminderCount(string userId)
//        {
//            int retval = 0;

//            try
//            {
//                OpenDB(@"GetUserReminderCount()");
//                MySQLCommand cmd = new MySQLCommand(
//                    @"select count(*) from system_reminders where (user = '" + userId.ToLower().Trim() + "') AND (user = creator)", m_conn);
//                IDataReader reader = cmd.ExecuteReader();
//                CloseDB(@"GetUserReminderCount()");

//                if (!reader.Read())
//                    throw new Exception("Internal DB Error 0x01");
//                else
//                    retval = int.Parse(reader[0].ToString());
//            }
//            catch (Exception e)
//            {
//                throw e;
//            }

//            return retval;
//        }

//        override public int GetUserPTPReminderCount(string userID)
//        {
//            int iRetVal = -1; // error

//            try
//            {
//                OpenDB(@"GetUserPTPCount()");
//                MySQLCommand cmd = new MySQLCommand(
//                    @"select COUNT(*) from system_reminders WHERE (user = '" + userID + "') AND (creator != user)", m_conn);
//                IDataReader dataRow = cmd.ExecuteReader();

//                if (dataRow.Read())
//                    iRetVal = int.Parse(dataRow[0].ToString());

//                CloseDB(@"GetUserPTPCount()");
//            }
//            catch (Exception ex)
//            {
//                Log.Instance.WriteError("GetUserPTPCount() Error: " + ex.Message);
//            }

//            return iRetVal;
//        }

//        override public Reminder GetReminder(string strRemID)
//        {
//            Reminder retVal = null;

//            try
//            {
//                OpenDB(@"GetReminder()");
//                MySQLCommand cmd = new MySQLCommand(
//                    @"SELECT * FROM system_reminders WHERE (id = '" + strRemID + "' and delivered = 0)", m_conn);
//                IDataReader reader = cmd.ExecuteReader();
//                CloseDB(@"GetReminder()");

//                while (reader.Read())
//                {
//                    string strPattern = "yyyy-MM-dd HH:mm:ss";
//                    DateTime datime;

//                    try
//                    {
//                        datime = DateTime.ParseExact(reader["servertime"].ToString(), strPattern, null);
//                    }
//                    catch (Exception e)
//                    {
//                        Log.Instance.WriteError("GetUsersReminders() DateTime Parsing Error: " + e.Message);
//                        continue;
//                    }

//                    retVal = new Reminder(reader["id"].ToString(),
//                        reader["user"].ToString(),
//                        reader["creator"].ToString(),
//                        reader["msg"].ToString(),
//                        datime,
//                        reader["datetime"].ToString(),
//                        null);
//                }

//            }
//            catch (Exception ex)
//            {
//                Log.Instance.WriteError("GetReminder() Error: " + ex.Message);
//                return null;
//            }

//            return retVal;
//        }

//        override public User GetUser(string strUserId)
//        {
//            User retval = null;

//            //try
//            //{
//            //    OpenDB(@"GetUser()");

//            //    MySQLCommand cmd = new MySQLCommand(
//            //        @"SELECT system_users.id,class,email,bot,timezone,DLS,plan,plan_name,num_reminders,num_ptp_reminders FROM system_users LEFT JOIN system_subscribe_plans ON system_subscribe_plans.id = system_users.plan WHERE system_users.id = '" + strUserId + "'", m_conn);
//            //    IDataReader userRow = cmd.ExecuteReader();

//            //    if (userRow.Read())
//            //    {

//            //        UserClassType uct = new UserClassType();
//            //        uct = (UserClassType)Enum.Parse(typeof(UserClassType), userRow["class"].ToString(), true);

//            //        User user = new User(
//            //            userRow["id"].ToString(),
//            //            userRow["email"].ToString(),
//            //            uct,
//            //            userRow["timezone"].ToString(),
//            //            int.Parse(userRow["DLS"].ToString()) == 1 ? true : false,
//            //            int.Parse(userRow["plan"].ToString()),
//            //            int.Parse(userRow["num_reminders"].ToString()),
//            //            int.Parse(userRow["num_ptp_reminders"].ToString()),
//            //            userRow["bot"].ToString());

//            //        DataTable ct = new MySQLSelectCommand(m_conn,
//            //            new string[] { "*" },
//            //            new string[] { "system_contacts" },
//            //            new object[,] { { "userid", "=", userRow["id"].ToString() } },
//            //            null, null).Table;

//            //        foreach (DataRow crow in ct.Rows)
//            //        {
//            //            user.AddContact(new IMContact(crow["login"].ToString(),
//            //                (ConnectionType)Enum.Parse(typeof(ConnectionType), crow["service"].ToString(), true),
//            //                int.Parse(crow["priority"].ToString())));
//            //        }

//            //        cmd = new MySQLCommand(
//            //            @"select * from system_allow_lists where (owner='" + user.UserID + "')", m_conn);
//            //        IDataReader allowRow = cmd.ExecuteReader();

//            //        while (allowRow.Read())
//            //        {
//            //            user.Buddies.Add(allowRow["buddy"].ToString().ToLower());
//            //        }

//            //        retval = user;
//            //    };

//            //    CloseDB(@"GetUser()");
//            //    Debug.Write("DBMgr.GetUser() connection used.\n");

//            //}
//            //catch (Exception ex)
//            //{
//            //    Log.Instance.WriteError("GetUser() Error: " + ex.Message);
//            //    return null;
//            //}

//            return retval;
//        }

//        override public ArrayList GetUsersReminders(string UserID)
//        {
//            ArrayList retReminders = new ArrayList();

//            try
//            {
//                OpenDB(@"GetUsersReminders()");
//                MySQLCommand cmd = new MySQLCommand(
//                    @"SELECT * FROM system_reminders WHERE (user = '" + UserID + "') AND (delivered = '0')", m_conn);
//                IDataReader reader = cmd.ExecuteReader();
//                CloseDB(@"GetUsersReminders()");

//                while (reader.Read())
//                {
//                    string strPattern = "yyyy-MM-dd HH:mm:ss";
//                    DateTime datime;

//                    try
//                    {
//                        datime = DateTime.ParseExact(reader["servertime"].ToString(), strPattern, null);
//                    }
//                    catch (Exception e)
//                    {
//                        Log.Instance.WriteError("GetUsersReminders() DateTime Parsing Error: " + e.Message);
//                        continue;
//                    }

//                    retReminders.Add(new Reminder(reader["id"].ToString(),
//                        reader["user"].ToString(),
//                        reader["creator"].ToString(),
//                        reader["msg"].ToString(),
//                        datime,
//                        reader["datetime"].ToString(),
//                        null));
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.Instance.WriteError("GetUsersReminders() Error: " + ex.Message);
//                return null;
//            }

//            return retReminders;
//        }

//        override public Reminder GetLastDeliveredReminder(string UserId)
//        {
//            Reminder retval = null;
//            try
//            {
//                //string strQuery = @"select * from system_reminders LEFT JOIN system_repeaters ON system_reminders.id = system_repeaters.remid where (user = '"+UserId+"' and delivered = 1) group by id order by deliveredtime desc limit 1";
//                string strQuery = @"select * from system_reminders LEFT JOIN system_repeaters ON system_reminders.id = system_repeaters.remid where (user = '" + UserId + "') group by id order by deliveredtime desc limit 1";
//                OpenDB(@"GetLastDeliveredReminder()");
//                MySQLCommand cmd = new MySQLCommand(
//                    //@"select * from system_reminders LEFT JOIN system_repeaters ON system_reminders.id = system_repeaters.remid where (user = '"+UserId+"' and delivered = 1) group by id order by deliveredtime desc limit 1",m_conn);
//                    strQuery, m_conn);
//                IDataReader reader = cmd.ExecuteReader();
//                CloseDB(@"GetLastDeliveredReminder()");

//                if (!reader.Read())
//                    return null;

//                RepeaterClass newRepeater = null; //new RepeaterClass(null,null,0,false,null);
//                if (reader["remid"].ToString() != "")
//                {
//                    newRepeater = new RepeaterClass(reader["repid"].ToString(),
//                        reader["pattern"].ToString(),
//                        int.Parse(reader["count"].ToString()),
//                        reader["disabled"].ToString() != "0",
//                        reader["expiration"].ToString());
//                }

//                retval = new Reminder(reader.GetString(5), reader.GetString(1), reader.GetString(8), reader.GetString(3), reader.GetDateTime(6), reader.GetString(2), newRepeater);

//                Debug.Write("DBMgr.GetLastDeliveredReminder() connection used.\n");
//            }
//            catch (Exception e)
//            {
//                Log.Instance.WriteError("GetLastDeliveredReminder() Error: " + e.Message);
//                return null;
//            }

//            return retval;
//        }

//        override public int CreateReminder(ref Reminder rem, string strBotName)
//        {
//            int iRemID;
//            string strRemBot = "";

//            if (strBotName == null)
//                strRemBot = _BotName;
//            else
//                strRemBot = strBotName;

//            try
//            {
//                OpenDB(@"CreateReminder()");
//                new MySQLInsertCommand(m_conn,
//                    new object[,] {{"delivered",rem.Delivered ? 1 : 0},{"msg",rem.Message },
//                                    {"servertime",rem.ServerDeliveryTimeString},{"bot",strRemBot},
//                                    {"user",rem.User},{"creator",rem.Creator},{"datetime",rem.UserTimeString}},
//                    "system_reminders");
//                DataTable dt = new MySQLSelectCommand(m_conn,
//                    new string[] { "LAST_INSERT_ID()" },
//                    null, null, null, null).Table;
//                CloseDB(@"CreateReminder()");

//                iRemID = int.Parse(dt.Rows[0].ItemArray[0].ToString());
//            }
//            catch (Exception e)
//            {
//                Log.Instance.WriteError("CreateReminder Error: " + e.Message);
//                return 0;
//            }

//            rem.ID = iRemID.ToString();
//            return iRemID;
//        }

//        override public bool SaveReminder(Reminder rem)
//        {
//            try
//            {
//                string s = rem.ServerDeliveryTimeString;
//                OpenDB(@"SaveReminder");
//                new MySQLUpdateCommand(m_conn,
//                    new object[,] {	{"datetime",rem.UserTimeString},
//                                    {"delivered",rem.Delivered ? 1 : 0},
//                                    {"msg",rem.Message },
//                                    {"servertime",rem.ServerDeliveryTimeString},
//                                    {"deliveredtime",rem.DeliveredTime},
//                                    {"deliveredname",rem.DeliveredName},
//                                    {"deliveredconn",rem.DeliveredName == null ? "ERROR DELIVERING REMINDER" : rem.DeliveredConnType.ToString().ToLower()}
//                                  },
//                    "system_reminders",
//                    new object[,] { { "id", "=", rem.ID } },
//                    null);
//                CloseDB(@"SaveReminder");
//            }
//            catch (Exception e)
//            {
//                Log.Instance.WriteError("SaveReminder Error: " + e.Message);
//                return false;
//            }

//            return true;
//        }

//        override public bool SaveReminderDeliveryInfo(Reminder rem)
//        {
//            try
//            {
//                OpenDB(@"SaveReminderDeliveryInfo");
//                new MySQLUpdateCommand(m_conn,
//                    new object[,] {	{"datetime",rem.UserTimeString},
//                                    {"delivered",rem.Delivered ? 1 : 0},
//                                    {"servertime",rem.ServerDeliveryTimeString},
//                                  },
//                    "system_reminders",
//                    new object[,] { { "id", "=", rem.ID } },
//                    null);

//                CloseDB(@"SaveReminderDeliveryInfo");
//            }
//            catch (Exception e)
//            {
//                Log.Instance.WriteError("UpdateReminder Error: " + e.Message);
//                return false;
//            }

//            return true;
//        }
//        #endregion
//    }

    /// <summary>
    /// ServiceDBManager is the webservice developed for service.php on the web end
    /// </summary>
    public class ServiceDBManager : DataManager
    {
        private WebClient _webClient = new WebClient();

        private string _strURL = string.Empty;
        public string ServiceUrl { get { return _strURL; } }

        private string _strBotName = string.Empty;
        public string BotName { get { return _strBotName; } }

        public ServiceDBManager(string strURL, string strBotName)
        {
            _strURL = strURL;
            _strBotName = strBotName;

            Log.Instance.WriteLine("ServiceDBManager Service URL: {0}", ServiceUrl);
            Log.Instance.WriteLine("ServiceDBManager Botname: {0}", BotName);
        }        

        private XmlDocument GetDataDocument(string strXmlCommand)
        {
            XmlDocument retDoc = new XmlDocument();

            try
            {
                DateTime startTime = DateTime.Now;
                string strResponse = _webClient.UploadString(ServiceUrl, strXmlCommand);
                DateTime stopTime = DateTime.Now;
                _lCalls++;

                // TODO: Log the command's function name
                // log the time interval for the command
                //XmlDocument cmdDoc = new XmlDocument();
                //cmdDoc.LoadXml(strXmlCommand);
                //XmlNode cmd = doc.SelectSingleNode(

                //TimeSpan span = stopTime - startTime;
                //Log.Instance.WriteLine("(Query took {0} sec)", span.TotalSeconds);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strResponse);

                XmlNode errorNode = doc.SelectSingleNode(@"/remindme/error");
                if (errorNode != null)
                    throw new Exception(errorNode.InnerText);

                XmlNode dataNode = doc.SelectSingleNode(@"/remindme/data");
                if (dataNode == null)
                    throw new Exception(@"No data node found in response from service.");

                retDoc.LoadXml(dataNode.InnerText);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return retDoc;
        }

        private Hashtable BuildHash(XmlNode infoNode)
        {
            Hashtable retHash = new Hashtable();
            foreach (XmlNode childNode in infoNode.ChildNodes)
            {
                retHash[childNode.Name] = childNode.InnerText;
            }
            return retHash;
        }

        // TODO: allow get all remiders, only those delivered, and those undelivered
        public override ArrayList GetUsersReminders(int iUserID)
        {
            ArrayList retList = new ArrayList();
            string strRequest = @"<?xml version=""1.0"" encoding=""utf-8""?>
<request version=""2.71828"">
  <function name=""GetUsersReminders"">
      <argument>" + iUserID.ToString() + @"</argument>
  </function>
</request>
";
            try
            {
                XmlDocument dataDoc = GetDataDocument(strRequest);

                foreach (XmlNode reminderNode in dataDoc.SelectNodes(@"/reminders/reminder"))
                {
                    Hashtable remInfo = BuildHash(reminderNode);

                    DateTime datime = DateTime.ParseExact(remInfo["servertime"].ToString(), @"yyyy-MM-dd HH:mm:ss", null);
                    retList.Add(new Reminder(
                        remInfo["id"].ToString(),
                        int.Parse(remInfo["userid"].ToString()),
                        int.Parse(remInfo["creatorid"].ToString()),
                        remInfo["msg"].ToString(),
                        datime,
                        remInfo["datetime"].ToString(),
                        null));
                }

            }
            catch (Exception ex)
            {
            }

            return retList;
        }

        public override User GetUser(int iUserID)
        {
            User newUser = null;
            Hashtable userinfo = new Hashtable();
            string strRequest = @"<?xml version=""1.0"" encoding=""utf-8""?>
<request version=""2.71828"">
  <function name=""Getuser"">
      <argument>" + iUserID.ToString()+ @"</argument>
  </function>
</request>
";
            try
            {
                XmlDocument dataDoc = GetDataDocument(strRequest);
                foreach (XmlNode element in dataDoc.SelectNodes(@"/user/*"))
                {
                    if (element.Name.ToLower() != "contacts" && element.Name.ToLower() != "buddies")
                        userinfo[element.Name.ToLower()] = element.InnerText;
                }

                UserClassType uct = (UserClassType)Enum.Parse(typeof(UserClassType), userinfo[@"class"].ToString(), true);
                newUser = new User(
                                int.Parse(userinfo[@"userid"].ToString()),
                                userinfo[@"username"].ToString(),
                                userinfo[@"email"].ToString(),
                                uct,
                                userinfo[@"timezone"].ToString(),
                                userinfo[@"dls"].ToString() == @"1",
                                int.Parse(userinfo[@"plan"].ToString()),
                                int.Parse(userinfo[@"num_reminders"].ToString()),
                                int.Parse(userinfo[@"num_ptp_reminders"].ToString()),
                                userinfo[@"bot"].ToString());


                foreach (XmlNode contact in dataDoc.SelectNodes(@"/user/contacts/contact"))
                {
                    Hashtable contactInfo = new Hashtable();
                    foreach (XmlNode contElement in contact.ChildNodes)
                    {
                        contactInfo[contElement.Name] = contElement.InnerText;
                    }

                    newUser.AddContact(new IMContact(
                        contactInfo[@"login"].ToString(),
                        (ConnectionType)Enum.Parse(typeof(ConnectionType), contactInfo[@"service"].ToString(), true),
                        int.Parse(contactInfo[@"priority"].ToString())));
                }



                foreach (XmlNode buddy in dataDoc.SelectNodes(@"/user/buddies/buddy"))
                {
                    Hashtable buddyInfo = new Hashtable();
                    foreach (XmlNode buddyElement in buddy.ChildNodes)
                    {
                        buddyInfo[buddyElement.Name] = buddyElement.InnerText;
                    }

                    newUser.Buddies.Add(buddyInfo["buddy"].ToString().ToLower());
                }

            }
            catch (Exception ex)
            {
            }

            return newUser;
        }

        public override User GetUserByUsername(string strUsername)
        {
            User newUser = null;
            Hashtable userinfo = new Hashtable();
            string strRequest = @"<?xml version=""1.0"" encoding=""utf-8""?>
<request version=""2.71828"">
  <function name=""GetUserByUsername"">
      <argument>" + strUsername + @"</argument>
  </function>
</request>
";
            try
            {
                XmlDocument dataDoc = GetDataDocument(strRequest);
                foreach (XmlNode element in dataDoc.SelectNodes(@"/user/*"))
                {
                    if (element.Name.ToLower() != "contacts" && element.Name.ToLower() != "buddies")
                        userinfo[element.Name.ToLower()] = element.InnerText;
                }

                UserClassType uct = (UserClassType)Enum.Parse(typeof(UserClassType), userinfo[@"class"].ToString(), true);
                newUser = new User(
                                int.Parse(userinfo[@"userid"].ToString()),
                                userinfo[@"username"].ToString(),
                                userinfo[@"email"].ToString(),
                                uct,
                                userinfo[@"timezone"].ToString(),
                                userinfo[@"dls"].ToString() == @"1",
                                int.Parse(userinfo[@"plan"].ToString()),
                                int.Parse(userinfo[@"num_reminders"].ToString()),
                                int.Parse(userinfo[@"num_ptp_reminders"].ToString()),
                                userinfo[@"bot"].ToString());


                foreach (XmlNode contact in dataDoc.SelectNodes(@"/user/contacts/contact"))
                {
                    Hashtable contactInfo = new Hashtable();
                    foreach (XmlNode contElement in contact.ChildNodes)
                    {
                        contactInfo[contElement.Name] = contElement.InnerText;
                    }

                    newUser.AddContact(new IMContact(
                        contactInfo[@"login"].ToString(),
                        (ConnectionType)Enum.Parse(typeof(ConnectionType), contactInfo[@"service"].ToString(), true),
                        int.Parse(contactInfo[@"priority"].ToString())));
                }



                foreach (XmlNode buddy in dataDoc.SelectNodes(@"/user/buddies/buddy"))
                {
                    Hashtable buddyInfo = new Hashtable();
                    foreach (XmlNode buddyElement in buddy.ChildNodes)
                    {
                        buddyInfo[buddyElement.Name] = buddyElement.InnerText;
                    }

                    newUser.Buddies.Add(buddyInfo["buddy"].ToString().ToLower());
                }

            }
            catch (Exception ex)
            {
            }

            return newUser;
        }

        public override ArrayList GetReminders()
        {
            ArrayList retList = new ArrayList();
            string strPattern = "yyyy-MM-dd HH:mm:ss";

            try
            {
                string s = 
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<request version=""2.71828"">
  <function name=""GetReminders"">
      <argument>" + BotName + @"</argument>
  </function>
</request>
";
                XmlDocument dataDoc = GetDataDocument(s);

                XmlNodeList reminderList = dataDoc.SelectNodes(@"/reminders/reminder");
                if (reminderList == null)
                {
                    // yeah, we neeed exceptions
                    return retList;
                }

                foreach (XmlNode reminderNode in reminderList)
                {
                    Hashtable reminderInfo = new Hashtable();
                    ArrayList contacts = new ArrayList();

                    foreach (XmlNode remElementNode in reminderNode.ChildNodes)
                    {
                        reminderInfo[remElementNode.Name.ToLower()] = remElementNode.InnerText;
                    }

                    DateTime datime = DateTime.ParseExact(reminderInfo[@"servertime"].ToString(),strPattern,null);

                    RepeaterClass newRepeater = null;
                    if (reminderInfo[@"repid"] != null)
                    {
                        newRepeater = new RepeaterClass(
                            reminderInfo["repid"].ToString(),
                            reminderInfo["pattern"].ToString(),
                            int.Parse(reminderInfo["count"].ToString()),
                            reminderInfo["disabled"].ToString() != "0",
                            reminderInfo["expiration"].ToString());
                    }

                    Reminder rem = new Reminder(
                        reminderInfo[@"id"].ToString(),
                        int.Parse(reminderInfo[@"userid"].ToString()),
                        int.Parse(reminderInfo[@"creatorid"].ToString()),
                        reminderInfo[@"msg"].ToString(),
                        datime,
                        reminderInfo[@"datetime"].ToString(),
                        newRepeater);

                    retList.Add(rem);
                }
            }
            catch (Exception ex)
            {
            }

            return retList;
        }

        public override ArrayList LoadUsers()
        {
            ArrayList retUsers = new ArrayList();

            try
            {
                string s = 
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<request version=""2.71828"">
  <function name=""LoadUsers"">
      <argument>"+BotName+@"</argument>
  </function>
</request>
";
                XmlDocument dataDoc = GetDataDocument(s);
                XmlNodeList userList = dataDoc.SelectNodes(@"/users/user");
                if (userList == null)
                {
                    // yeah, we neeed exceptions
                    return retUsers;
                }

                foreach (XmlNode userNode in userList)
                {
                    Hashtable userinfo = new Hashtable();
                    ArrayList contacts = new ArrayList();

                    foreach (XmlNode userElementNode in userNode.ChildNodes)
                    {
                        if (userElementNode.Name.ToLower() != "contact")
                            userinfo[userElementNode.Name.ToLower()] = userElementNode.InnerText;
                        else
                        {
                            XmlDocument tempDoc = new XmlDocument();
                            tempDoc.LoadXml(userElementNode.OuterXml);

                            XmlNode serviceNode = tempDoc.SelectSingleNode(@"/contact/service");
                            if (serviceNode == null)
                            {
                                // ignore this of throw an exception? ignore it for now
                                continue;
                            }

                            XmlNode loginNode = tempDoc.SelectSingleNode(@"/contact/login");
                            if (loginNode == null)
                            {
                                // same as above?
                                continue;
                            }

                            IMContact cont = new IMContact(
                                loginNode.InnerText,
                                (ConnectionType)Enum.Parse(typeof(ConnectionType), serviceNode.InnerText, true),
                                contacts.Count);

                            contacts.Add(cont);
                        }
                    }

					UserClassType uct = new UserClassType();
					uct = (UserClassType)Enum.Parse(typeof(UserClassType),userinfo["class"].ToString(),true);

                    User newUser = new User(
                        int.Parse(userinfo[@"userid"].ToString()),
                        userinfo[@"username"].ToString(),
                        userinfo[@"email"].ToString(),
                        uct,
                        userinfo[@"timezone"].ToString(),
                        userinfo[@"dls"].ToString() == @"1",
                        int.Parse(userinfo[@"plan"].ToString()),
                        int.Parse(userinfo[@"num_reminders"].ToString()),
                        int.Parse(userinfo[@"num_ptp_reminders"].ToString()),
                        userinfo[@"bot"].ToString());


                    newUser.Contacts = contacts;


                    retUsers.Add(newUser);
                }

            }
            catch (Exception ex)
            {
            }


            return retUsers;
        }

        public override int GetUserReminderCount(int iUserID)
        {
            ArrayList retUsers = new ArrayList();

            try
            {
                string s = @"<?xml version=""1.0"" encoding=""utf-8""?>
<request version=""2.71828"">
  <function name=""GetUserReminderCount"">
      <argument>" + iUserID.ToString() + @"</argument>
  </function>
</request>
";

                XmlDocument doc = GetDataDocument(s);
                XmlNode dataNode = doc.SelectSingleNode(@"/count");
                if (dataNode == null)
                {
                    // more error reporting (custom exceptions?)
                    return -1;
                }

                return int.Parse(dataNode.InnerText);
            }
            catch
            {
            }

            return -1;
        }

        public override int GetUserPTPReminderCount(int iUserID)
        {
            ArrayList retUsers = new ArrayList();

            try
            {
                string s = @"<?xml version=""1.0"" encoding=""utf-8""?>
<request version=""2.71828"">
  <function name=""GetUserPTPReminderCount"">
      <argument>" + iUserID.ToString() + @"</argument>
  </function>
</request>
";

                XmlDocument doc = GetDataDocument(s);
                XmlNode dataNode = doc.SelectSingleNode(@"/count");
                if (dataNode == null)
                {
                    // more error reporting (custom exceptions?)
                    return -1;
                }

                return int.Parse(dataNode.InnerText);
            }
            catch
            {
            }

            return -1;
        }

        public override Reminder GetReminder(int iRemID)
        {
            try
            {
                string s = 
                    @"<?xml version=""1.0"" encoding=""utf-8""?>
<request version=""2.71828"">
  <function name=""GetReminder"">
      <argument>" + iRemID.ToString() + @"</argument>
  </function>
</request>
";
                XmlDocument dataDoc = GetDataDocument(s);

                Hashtable reminderInfo = new Hashtable();
                foreach (XmlNode remElementNode in dataDoc.DocumentElement.ChildNodes)
                {
                    reminderInfo[remElementNode.Name.ToLower()] = remElementNode.InnerText;
                }

                DateTime datime = DateTime.ParseExact(reminderInfo[@"servertime"].ToString(), @"yyyy-MM-dd HH:mm:ss", null);

                Reminder retVal = new Reminder(
                    reminderInfo[@"id"].ToString(),
                    int.Parse(reminderInfo[@"userid"].ToString()),
                    int.Parse(reminderInfo[@"creatorid"].ToString()),
                    reminderInfo[@"msg"].ToString(),
                    datime,
                    reminderInfo[@"datetime"].ToString(),
                    null);

                return retVal;

            }
            catch (Exception ex)
            {
            }

            return null;

        }

        public override Reminder GetLastDeliveredReminder(int iUserID)
        {
            Reminder rem = null;
            string strRequest = @"<?xml version=""1.0"" encoding=""utf-8""?>
<request version=""2.71828"">
  <function name=""GetLastDeliveredReminder"">
      <argument>" + iUserID.ToString() + @"</argument>
  </function>
</request>
";
            try
            {
                XmlDocument dataDoc = GetDataDocument(strRequest);
                Hashtable reminderInfo = BuildHash(dataDoc.DocumentElement);

                RepeaterClass newRepeater = null;
                if (reminderInfo[@"repid"] != null)
                {
                    newRepeater = new RepeaterClass(
                        reminderInfo["repid"].ToString(),
                        reminderInfo["pattern"].ToString(),
                        int.Parse(reminderInfo["count"].ToString()),
                        reminderInfo["disabled"].ToString() != "0",
                        reminderInfo["expiration"].ToString());
                }

                DateTime datime = DateTime.ParseExact(reminderInfo["servertime"].ToString(), @"yyyy-MM-dd HH:mm:ss", null);
                rem = new Reminder(
                    reminderInfo[@"id"].ToString(),
                    int.Parse(reminderInfo[@"userid"].ToString()),
                    int.Parse(reminderInfo[@"creatorid"].ToString()),
                    reminderInfo[@"msg"].ToString(),
                    datime,
                    reminderInfo[@"datetime"].ToString(),
                    newRepeater);

            }
            catch (Exception ex)
            {

            }

            return rem;
        }

        public override int CreateReminder(ref Reminder rem, string strBotName)
        {
            string strRequest = @"<request>
	<function>
		<name>CreateReminder</name>
		<argument>"+rem.UserID+@"</argument>
		<argument>" + rem.CreatorID + @"</argument>
		<argument>" + rem.ServerDeliveryTimeString + @"</argument>
		<argument>" + rem.UserTimeString + @"</argument>
		<argument>" + rem.Message + @"</argument>
		<argument>" + rem.Delivered + @"</argument>
		<argument>" + strBotName + @"</argument>
	</function>
</request>";

            try
            {
                XmlDocument dataDoc = GetDataDocument(strRequest);
                Hashtable result = BuildHash(dataDoc.DocumentElement);
                
                rem.ID = result[@"#text"].ToString();
                return int.Parse(rem.ID);
            }
            catch (Exception ex)
            {
            }

            return 0;
        }

        public override bool SaveReminder(Reminder rem)
        {
            string strRequest = @"<request>
	<function>
		<name>SaveReminder</name>
		<argument>" + rem.ID + @"</argument>
        <argument>" + rem.ServerDeliveryTimeString + @"</argument>
		<argument>" + rem.UserTimeString + @"</argument>
		<argument>" + rem.Message + @"</argument>
		<argument>" + (rem.Delivered ? "1" : "0") + @"</argument>
		<argument>" + rem.DeliveredTime + @"</argument>
		<argument>" + rem.DeliveredName + @"</argument>
		<argument>" + rem.DeliveredConnType.ToString() + @"</argument>
	</function>
</request>";

            try
            {
                XmlDocument dataDoc = GetDataDocument(strRequest);
                Hashtable result = BuildHash(dataDoc.DocumentElement);

                return (int.Parse(result[@"#text"].ToString()) != 0) ? true : false;
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        public override bool SaveReminderDeliveryInfo(Reminder rem)
        {
            string strRequest = @"<request>
	<function>
		<name>SaveReminderDeliveryInfo</name>
		<argument>" + rem.ID + @"</argument>
        <argument>" + rem.ServerDeliveryTimeString + @"</argument>
		<argument>" + rem.UserTimeString + @"</argument>
		<argument>" + (rem.Delivered ? "1" : "0") + @"</argument>
	</function>
</request>";

            try
            {
                XmlDocument dataDoc = GetDataDocument(strRequest);
                Hashtable result = BuildHash(dataDoc.DocumentElement);

                return (int.Parse(result[@"#text"].ToString()) != 0) ? true : false;
            }
            catch (Exception ex)
            {
            }

            return false;            
        }

        // TODO: implent this function
        public override AdText GetNextAd()
        {
            //return base.GetNextAd();
            return new AdText(0, "", "");
        }

        // TODO: implement this function
        public override void SaveAd(AdHistory adHist)
        {
            return; // do absolutely nothing right now
        }


    }
}   
