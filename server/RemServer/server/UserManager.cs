using System;
using System.Collections;
using System.Data;
using System.Threading;

namespace server
{
	public class AllowBuddy
	{
		private string m_owner = "";
		public string Owner { get { return m_owner; } }

		private string m_buddy = "";
		public string Buddy { get { return m_buddy; } }

		private int m_maxreminders = 0;
		public int MaxReminders { get { return m_maxreminders; } }

		public AllowBuddy(string strBuddy,string strOwner, int maxrem)
		{
			m_owner = strOwner;
			m_buddy = strBuddy;
			m_maxreminders = maxrem;
		}
	}

	public class IMContact
	{
		private string _username = "";
		public string UserName { get { return _username; } }

		private ConnectionType _serviceType = 0;
		public ConnectionType ConnectionType { get { return _serviceType; } }

		private int _priority = 0;
		public int Priority { get { return _priority; } }

		public bool Verified = false;

		public IMContact(string strUserName, ConnectionType type, int Priority)
		{
			_username  = strUserName;
			_serviceType = type;
			_priority = Priority;
		}

		public IMContact(string strUserName, ConnectionType type, int Priority, bool bVerified)
		{
			_username  = strUserName;
			_serviceType = type;
			_priority = Priority;
			Verified = bVerified;
		}
	}

	public enum UserClassType {USER =0, ADMIN = 255};

	public class User
	{
		private UserClassType _class;
		public UserClassType Class { get { return _class; } }

		private string _botname = "";
		public string BotName { get { return _botname; } }

		//private string _userid = "";
		//public string UserID { get { return _userid; } }

        private int _userid = 0;
        public int UserID { get { return _userid; } }

        private string _userName = string.Empty;
        public string Username { get { return _userName; } }

		private string _timezone = "";
		public string TimeZone { get { return _timezone; } }

		private string _email = "";
		public string Email { get { return _email; } }

		private ArrayList _contacts;
		public ArrayList Contacts 
        { 
            get { return _contacts; }
            set { _contacts = value; }
        } 

		private ArrayList m_buddies;
		public ArrayList Buddies { get { return m_buddies; }} 

		private int _planID = 0;
		public int PlanNumber { get { return _planID; }}

		private int _maxNumReminders = 0;
		public int MaxNumReminders { get { return _maxNumReminders; }}

		private int _maxNumPTPReminders = 0;
		public int MaxNumPTPReminders { get { return _maxNumPTPReminders; }}

		private bool _dls = false;
		public bool DLS { get { return _dls; } }

		public void AddContact(IMContact cont)
		{	
			_contacts.Add(cont);
		}

//		public IMContact GetIMContact(ConnectionType serviceType)
//		{
//			foreach (IMContact cont in _contacts)
//			{
//				if ((cont.ConnectionType == serviceType) && (cont.UserName.Length > 0))
//					return cont;
//			}
//
//			return null;
//		}

		public ArrayList GetConnectionNames(ConnectionType conType)
		{
			ArrayList retVal = new ArrayList();
			foreach (IMContact cont in _contacts)
			{
				if ((cont.ConnectionType == conType) && (cont.UserName.Length > 0))
				{
					retVal.Add(cont);
				}
			}
			return retVal;
		}

		public User(int iUserID, string strUsername, string strEmail, UserClassType strClass, 
			string strTimeZone, bool Dls, int iPlan, int iMaxRems, int iMaxPTPRems, string strBotName)
		{
			_contacts = new ArrayList(3);
			
			_email = strEmail;
			_class = strClass;
			_timezone = strTimeZone;
			_planID = iPlan;
			_dls = Dls;
			_maxNumReminders = iMaxRems;
			_botname = strBotName;
			m_buddies = new ArrayList();
			_maxNumPTPReminders = iMaxPTPRems;

            
            // TODO: change this when bot is switched over to use userid as primary key and not the user's "login name"
            _userName = strUsername;
            _userid = iUserID;
		}

	}

	/// <summary>
	/// Summary description for UserManager.
	/// </summary>
	public class UserManager
	{
        public int LoadUsers(DataManager dbMgr)
		{
			m_users.Clear();

			ArrayList theUsers = dbMgr.LoadUsers();

			if (theUsers == null)
				return -1;

			foreach (User user in theUsers)
			{
				AddUser(user);
			}

			return m_users.Count;
		}

		public ArrayList Users
		{
			get { return m_users; }
		}
		private ArrayList m_users;
		public int NumberOfUsers { get { return m_users.Count; } }

		public UserManager()
		{
			m_users = new ArrayList();
		}

		public void AddUser(User user)
		{
			m_users.Add(user);
		}

		public void ClearUsers()
		{
			lock(this)
				m_users.Clear();
		}

		public User GetUserByService(ConnectionType type, string strName)
		{
			lock(this) 
			{
				foreach (User user in m_users)
				{
					ArrayList contacts = user.GetConnectionNames(type);

					foreach (IMContact cont in contacts)
					{
						if (cont == null)
							continue;
						else if ((cont.ConnectionType == type) && (cont.UserName == strName))
							return user;
					}
				}
				return null;
			}	
		}

		public User GetUserByID(int iUserID)
		{
			lock(this)
			{
				foreach (User user in m_users)
				{
                    if (user.UserID == iUserID)
                        return user;
					//if (user.UserID.ToLower().ToLower().Trim() == strID.ToLower().Trim())
					//	return user;
				}

				return null;
			}
		}

		public string [] GetServiceNames(ConnectionType type)
		{
			lock(this)
			{
				ArrayList names = new ArrayList();

				foreach (User user in m_users)
				{
					ArrayList contacts = user.GetConnectionNames(type);

					foreach (IMContact cont in contacts)
					{
						if (cont != null && cont.UserName.Length > 0)
							names.Add(cont.UserName);
					}
				}
				return (string []) names.ToArray(typeof(string));
			}
		}

//		public User [] GetServiceUsers(ConnectionType type)
//		{
//			lock (this)
//			{
//				ArrayList users = new ArrayList();
//
//				foreach (User user in m_users)
//				{
//					ArrayList contacts = user.GetConnectionNames(type);
//					bool bAdded = false;
//
//					foreach (IMContact cont in contacts)
//					{
//						if (cont != null)
//						{
//							if (cont.UserName.Length > 0 && !bAdded)
//							{
//								users.Add(user);
//								bAdded = true;
//							}
//						}
//					}
//				}
//				return (User []) users.ToArray(typeof(User));
//			}
//		}
	}
}
