using System;
using System.Timers;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using MsgParser;
using System.Resources;

namespace server
{
	public class ReminderException : Exception
	{
		public string s;
		public ReminderException():base()
		{
			s=null;
		}
		public ReminderException(string message):base(message)
		{
			s=message.ToString();
		}
		public ReminderException(string message,Exception myNew):base(message,myNew)
		{
			s=message.ToString();// Stores new exception message into class member s
		}
	}

	public class Reminder
	{
		private RepeaterClass m_repeater;
		public RepeaterClass Repeater { get { return m_repeater; }}

		private bool m_bInDeliveryQue;
		public bool InDeliveryQue {get {return m_bInDeliveryQue;} set { m_bInDeliveryQue = value; }}

		private bool m_bDelivered;
		public bool Delivered {get {return m_bDelivered;} set { m_bDelivered = value; }}
		
		private string _strID = "";
		public string ID { get { return _strID; } set { _strID = value; }}

		private string _strMessage = "";
		public string Message { get { return _strMessage; } }

        // the user for whom this reminder is intended
		private int _iUserID = 0;
		public int UserID { get { return _iUserID; } }

        // the person who created the reminder
		private int _iCreatorID = 0;
        public int CreatorID { get { return _iCreatorID; } }

		private string _strUserTime = "";
		public string UserTimeString { get { return _strUserTime; } set { _strUserTime = value; } }

		private string _strDeliveredTime = "";
		public string DeliveredTime { get { return _strDeliveredTime; } set { _strDeliveredTime = value; } }

		private ConnectionType _deliveredConnType;
		public ConnectionType DeliveredConnType { get { return _deliveredConnType; } set { _deliveredConnType = value; } }
	
		private string _deliveredName;
		public string DeliveredName { get { return _deliveredName; } set { _deliveredName = value; } }

		private DateTime m_datetime;
		public DateTime ServerDeliveryTime 
		{
			get { return m_datetime; } 
			set { m_datetime = value; }
		}
		
		public string ServerDeliveryTimeString
		{
			get
			{
				if (m_datetime.Year == 0)
					return "0000-00-00 00:00:00";

				return m_datetime.ToString("u");
			}

			set
			{
				try 
				{
					m_datetime = DateTime.ParseExact(value,@"yyyy-MM-dd HH:mm:ss", null); 
				}
				catch 
				{
					//m_datetime = new DateTime(); hmm?
				}
			}
		}

		// copy constructor
//		public Reminder(Reminder rem)
//		{
//			m_bInDeliveryQue = rem.InDeliveryQue;
//			m_bDelivered = rem.Delivered;
//			_strID = rem.ID;
//			_strMessage = rem.Message;
//			_strUser = rem.User;
//			m_datetime = rem.ServerDeliveryTime;
//			_strCreator = rem.Creator;
//		}

		// used when creating a reminder for self or another user
		public Reminder(int UserId,int Creator,string strMsg,DateTime dt,string userstr)
		{
			m_datetime = dt;
			m_bDelivered = false;
			_strMessage = strMsg;
			
			m_bInDeliveryQue = false;
			_strUserTime = userstr;
            
            _iUserID = UserId;
			_iCreatorID = Creator;
		}


		// used when loading reminders
		public Reminder(string remId,
						int UserId,
						int Creator,
						string strMsg,
						DateTime dt,
						string strUserDt,
						RepeaterClass repeater)
						//ConnectionType deliveredConn,
						//string deliveredName)
		{
			_strID = remId;
			m_datetime = dt;
			m_bDelivered = false;
			_strMessage = strMsg;
			m_bInDeliveryQue = false;
			_strUserTime = strUserDt;
            m_repeater = repeater;

            _iUserID = UserId;
            _iCreatorID = Creator;
			
			//_deliveredConnType = deliveredConn;
			//_deliveredName = deliveredName;
		}
	}
	/// <summary>
	/// Summary description for ReminderManager.
	/// </summary>
	public class ReminderManager
	{
		private bool m_bTimerOn = false;
		public bool TimerOn { get { return m_bTimerOn; } }

		public delegate void OnReminderHandler(Reminder rem);
		public event OnReminderHandler OnReminder;

		private System.Timers.Timer remTimer;
		private int remInterval = 5; // seconds

		private ArrayList reminders;
        public ArrayList ReminderQueue { get { return reminders; } } 

		bool m_bLocked = false;

		private string m_strBotName;

		public ReminderManager(string strBotName)
		{
			reminders = new ArrayList();
			remTimer = new System.Timers.Timer(remInterval*1000);
			remTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckReminders);
			m_strBotName = strBotName;
			//remTimer.Stop();
		}

		public void Start()
		{	
			//while (m_bLocked);
			remTimer.Start();
			m_bTimerOn = true;
		}

		public void Stop()
		{
			//while (m_bLocked);
			remTimer.Stop();
			m_bTimerOn = false;
		}

		private void CheckReminders(object sender,System.Timers.ElapsedEventArgs args)
		{
			//Debug.WriteLine("Entering CheckReminders() timer...");
			m_bLocked = true;

            try
            {
                lock (this)
                {
                    foreach (Reminder rem in reminders)
                    {
                        if (rem.ServerDeliveryTime < System.DateTime.Now && !rem.InDeliveryQue && !rem.Delivered)
                        {
                            rem.InDeliveryQue = true;
                            OnReminder(rem);
                        }
                    }
                }
            }
            catch
            {
            }

			m_bLocked = false;
		}

        public int LoadReminders(DataManager dbMgr, bool bSupressStartStop)
		{
			if (!bSupressStartStop)
				Stop();
			
			m_bLocked = true;

			// need to update dbMgr to pull the serverTimeString thingy
			ArrayList theReminders = dbMgr.GetReminders();

			if (theReminders == null)
				return 0;

			reminders.Clear();
			reminders = theReminders;
			
			// logic to check for upcoming dates should go here

			m_bLocked = false;

			if (!bSupressStartStop)
				Start();

			return theReminders.Count;
		}

        public int LoadReminders(DataManager dbMgr)
		{
			return LoadReminders(dbMgr,false);
		}



        public int CreateReminder(DataManager dbMgr, User user, MessageParser msgPar, UserManager userMgr)
		{
			// TODO: add exception handling
			string strPattern;
			DateTime dt;

			try
			{
				strPattern = "yyyy-MM-dd HH:mm:ss"; 
				dt = DateTime.ParseExact(msgPar.ServerTimeString, strPattern, null); 
			} 
			catch (Exception e)
			{
				throw e;
			}
			

			Reminder rmdr = null;
			User toUser;

			if (msgPar.ToUser.ToLower().Trim() == @"me"	|| msgPar.ToUser.ToLower().Trim() == user.Username.ToLower().Trim())
				toUser = userMgr.GetUserByID(user.UserID); // this saves precious calls to the db
			else
                toUser = dbMgr.GetUserByUsername(msgPar.ToUser);
				//toUser = dbMgr.GetUser(msgPar.ToUser);

			if (toUser == null)
				throw new ReminderException("("+msgPar.ToUser+") is not a valid RemindMe username.");
			

			if (msgPar.ToUser.ToLower().Trim() == @"me"	|| msgPar.ToUser.ToLower().Trim() == user.Username.ToLower().Trim())
			{
				//if (reminderMgr.UserReminderCount(user.UserID) >= user.MaxNumReminders)
				if (dbMgr.GetUserReminderCount(user.UserID) >= user.MaxNumReminders)
				{
					ResourceManager rm = new ResourceManager("server.botMsgs", this.GetType().Assembly); 
					string strMsg = rm.GetString("max_reminder_error");
					strMsg = strMsg.Replace("%d",user.MaxNumReminders.ToString());
					throw new ReminderException(strMsg);
				}

				// create a reminder for the user
				rmdr = new Reminder(user.UserID,user.UserID,msgPar.MessageText,dt,msgPar.UserTimeString);
			}
			else if (msgPar.ToUser.Trim().Length >= 3)
			{
				// create a person-to-person reminder
				
				// TODO: somewhere here you would do a check for commas to check for
				// something like: remind amy,bob,frank in 10 minutes to meet in conference room b

                // TODO: Change this to user.UserID
				if (toUser.Buddies.IndexOf(user.Username.ToLower()) < 0 && user.Class != UserClassType.ADMIN)
					throw new ReminderException(@"This user has not added you to his or her allow list to receive reminders.");

				// see if the touser has any ptp reminders left
				if (toUser.MaxNumPTPReminders <= dbMgr.GetUserPTPReminderCount(toUser.UserID))
					throw new ReminderException(@"That user's reminder inbox is full!");

				rmdr = new Reminder(toUser.UserID,user.UserID,msgPar.MessageText,dt,msgPar.UserTimeString);
			}
		
			if (rmdr == null)
				throw new Exception(@"ReminderManager.CreateReminder(): rmdr = null");

			// pass it a reference, dbMgr will assign the ID
			int wId = dbMgr.CreateReminder(ref rmdr,toUser.BotName);

            if (wId == 0)
            {

                throw new ReminderException(@"There has been an internal error. Please report error code 0x19 to support@zethon.net");
            }
			
			if (m_strBotName == toUser.BotName)
					reminders.Add(rmdr);
	
			return wId;
		}

		public void ClearReminders()
		{
			while (m_bLocked);
			reminders.Clear();
		}

		// sets the users last delivered reminder to be re-delivered
        public int RepeatReminder(DataManager dbMgr, int UserId, MessageParser mp)
		{
			Reminder rem = dbMgr.GetLastDeliveredReminder(UserId);

			if (rem == null)
				return 0;

			TimeSpan ts = (TimeSpan)(DateTime.Now - rem.ServerDeliveryTime);
			if (ts.TotalHours > 30)
				return -1; // special code for no recent reminders error

			//rem.DeliveredTime = @"0000-00-00 00:00:00";
			//rem.DeliveredName = "";
			rem.Delivered = false;
			rem.UserTimeString = mp.UserTimeString;
			rem.ServerDeliveryTimeString = mp.ServerTimeString;

			if (EditReminderDeliveryInfo(dbMgr,rem,true))
			{
				return int.Parse(rem.ID);
			}
				
			return 0;
		}

		public bool SaveReminderToQue(Reminder newRem)
		{
			bool bFound = false;

			foreach (Reminder rm in reminders)
			{
				if (rm.ID == newRem.ID)
				{
					reminders.Remove(rm);
					reminders.Add(newRem);
					bFound = true;
					break;
				}
			}

			return bFound;
		}

        public bool EditReminderDeliveryInfo(DataManager dbMgr, Reminder newRem, bool bAddToArray)
		{
			bool bFound = false;

			foreach (Reminder rm in reminders)
			{
				if (rm.ID == newRem.ID)
				{
					reminders.Remove(rm);
					reminders.Add(newRem);
					bFound = true;
					break;
				}
			}

			if (!bFound && !bAddToArray) 
				return false;
			else if (!bFound && bAddToArray)
				reminders.Add(newRem);
			
			//return dbMgr.SaveReminder(newRem);
			return dbMgr.SaveReminderDeliveryInfo(newRem);
		}

//		public bool EditReminder(DBManager dbMgr,Reminder newRem)
//		{
//			return EditReminder(dbMgr,newRem,false);
//		}

        //public int UserReminderCount(int iUserId)
        //{
        //    int iRetVal = 0;
        //    m_bLocked = true;
			
        //    foreach (Reminder rem in reminders)
        //    {
        //        if (rem.UserID == iUserId)
        //            iRetVal++;
        //    }

        //    m_bLocked = false;
        //    return iRetVal;
        //}

	}

}
