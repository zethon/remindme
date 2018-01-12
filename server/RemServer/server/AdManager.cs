using System;

//SELECT ad_info.id,ad_text.text,ad_text.html FROM ad_info 
//LEFT JOIN ad_text ON ad_info.id=ad_text.id 
//LEFT JOIN ad_history ON ad_info.id=ad_history.id
//WHERE (ad_info.maxcount > ad_info.currentcount)
//ORDER BY ad_history.datetime ASC
//LIMIT 1

namespace server
{
	public class AdHistory
	{
		public AdHistory(int id,DateTime dt, string userName,ConnectionType ct)
		{
			m_iId = id;
			m_dt = dt;
			m_strUserName = userName;
			m_ConnType = ct;
		}

		private int m_iId;
		public int ID { get { return m_iId; }}

		private DateTime m_dt;
		public DateTime TimeStamp { get { return m_dt; }}

		private string m_strUserName;
		public string UserName { get { return m_strUserName; }}

		private ConnectionType m_ConnType;
		public ConnectionType ConnType { get { return m_ConnType; }}
	}

	public class AdText
	{
		private int m_iId;
		public int ID { get { return m_iId; }}
		
		private string m_strText;
		public string Text 
		{ 
			get { return m_strText; }
			set { m_strText = value; }
		}	  
        		
		private string m_strHTML;
		public string HTML 
		{ 
			get { return m_strHTML; } 
			set { m_strHTML = value; }
		}

		public AdText(int id,string text,string html)
		{
			m_iId = id;
			m_strText = text;
			m_strHTML = html;
		}
	}

	public class AdManager
	{
        public AdManager(DataManager dbm)
		{
			dbMgr = dbm;
		}

        private DataManager dbMgr;

		public AdText GetNextAd()
		{
			AdText retval = dbMgr.GetNextAd();

			if (retval != null && retval.ID > 0)
			{
				retval.Text = "\r\n\r\n *** "+retval.Text+" (sponsor)";
				retval.HTML = "\r\n\r\n *** "+retval.HTML+" (sponsor)";
			}

//			retval = new AdText(1,
//				"\r\n\r\n *** Check out the new features of RemindMe by visiting the website! http://www.remindme.cc",
//				"\r\n\r\n *** <a href='http://www.remindme.cc'>RemindMe.CC</a> - Check out the new features of RemindMe!");

			return (retval);
		}

		public string GetNextAdText(User user,ConnectionType cType)
		{
            //if (user != null && user.PlanNumber > 100)
            //    return "";

            //AdText adtxt = GetNextAd();
			string retVal = "";

            //if (cType == ConnectionType.AIM)
            //    retVal = adtxt.HTML;
            //else
            //    retVal = adtxt.Text;

            //string userId = "";
            //if (user == null)
            //    userId = "anonymous";
            //else
            //    userId = user.UserID;

            //dbMgr.SaveAd(new AdHistory(adtxt.ID,DateTime.Now,userId,cType));
			return retVal;
		}
					
	}
}
