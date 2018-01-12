using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using DotOSCAR;
//using DotMSN;
using dotICQ;
using dotEmail;
using dotYahoo;
using dotTOC2;
using System.Web;
using System.Net;
using System.Text;
using Axosoft.Common.Utilities; // used in ConnMail -- should probably integrated with dotEmail
//using DotMSN = XihSolutions.DotMSN;
using dotMSN = DNBSoft.MSN.ClientController;
using jabber.client;

namespace server
{
	public enum ConnectionStatus { Offline, Online };
	public enum ConnectionType {AIM = 0, MSN = 1 , ICQ = 2, YAHOO = 3, EMAIL = 4, SMS = 5, JABBER = 6};

	public class InstantMessage
	{
		private string _text;
		public string Text
		{
			get { return _text; }
		}

		private string _user;
		public string User
		{
			get { return _user; }
		}
		

		public InstantMessage(string User, string Text)
		{
			_text = Text;
			_user = User;
		}
	}

	public class BuddyStatus
	{
		public BuddyStatus(string strName, bool bOnline)
		{
			_name = strName;
			_online = bOnline;
		}

		private string _name;
		public string Name
		{
			get { return _name; }
		}

		private bool _online;
		public bool Online
		{
			get { return _online; }
		}
	}

	public class Connection
	{
		public delegate void OnMessageHandler(Connection conn,InstantMessage msg);
		public event OnMessageHandler OnMessage;

		public delegate void OnSignedOnHandler(Connection conn);
		public event OnSignedOnHandler OnSignedOn;

		public delegate void OnDisconnectHandler(Connection conn);
		public event OnDisconnectHandler OnDisconnect;

		public delegate void OnSendMessageHanlder(Connection conn,InstantMessage msg);
		public event OnSendMessageHanlder OnSendMessage;

		public delegate void OnUpdateBuddyHanlder(Connection conn,BuddyStatus stat);
		public event OnUpdateBuddyHanlder OnUpdateBuddy;

		public delegate void OnErrorHandler(Connection conn,string strError);
		public event OnErrorHandler OnError;

		public delegate void OnReconnectHandler(Connection conn);
		public event OnReconnectHandler OnReconnect;


		public ConnectionType m_type;

		public string m_strLogin;
		public string m_strPassword;

		protected ConnectionStatus m_status;
		public ConnectionStatus Status
		{
			get { return m_status; }
		}

		public string [] m_BuddyList;
		Hashtable m_buddies;
						
		public Connection()
		{
			m_buddies = new Hashtable();
			m_type = new ConnectionType();
			m_status = ConnectionStatus.Offline;
		}

		public virtual void UpdateBuddyList()
		{}

		public virtual bool Connect()
		{
            Log.Instance.WriteLine("Connecting {0} User {1}", m_type.ToString(), m_strLogin);
			return m_status == ConnectionStatus.Offline;
		}

		public virtual void Disconnect()
		{
			m_status = ConnectionStatus.Offline;
		}

		public virtual void AddBuddies(string [] strBuddies)
		{}

		public virtual void SendMessage(string strUser, string strMessage)
		{
			if (OnSendMessage != null)
				OnSendMessage(this,new InstantMessage(strUser,strMessage));
		}

        public virtual void SendServerData(object data)
        {}

		public virtual bool IsBuddyOnline(string strBuddy)
		{
			return (m_buddies[strBuddy] != null);
		}

		// used by the sub class connections for handlers
		#region subclass_functions
		public void _OnReconnect()
		{
			if (OnReconnect != null)
				OnReconnect(this);
		}

		public void _OnError(Connection conn,string strError)
		{
			if (OnError != null)
				OnError(conn,strError);
		}

		public void _OnDisconnect()
		{
			m_status = ConnectionStatus.Offline;

			if (OnDisconnect != null)
				OnDisconnect(this);
		}

		public void _OnMessageReceived(string strUser,string strMsg, bool bAuto)
		{
			_OnUpdateBuddy(strUser, true);

			if (OnMessage != null)
				OnMessage(this,new InstantMessage(strUser.ToLower().Trim(),strMsg.Trim()));
		}

		protected void _OnSignedOn()
		{
			m_status = ConnectionStatus.Online;

			if (OnSignedOn != null)
				OnSignedOn(this);
		}

		public void _OnUpdateBuddy(string strName, bool bOnline)
		{
			if (m_buddies == null)
				return;

			bool bExists = (m_buddies[strName] != null);
			if (bOnline && !bExists)
			{
				m_buddies[strName] = true;

				if (OnUpdateBuddy != null)
					OnUpdateBuddy(this, new BuddyStatus(strName,bOnline));
			}
			else if (bExists && !bOnline)
			{
				m_buddies[strName] = null;

				if (OnUpdateBuddy != null)
					OnUpdateBuddy(this, new BuddyStatus(strName,bOnline));
			}
		}
		#endregion

	}

	#region ConnSMS Class
	public class ConnSMS : Connection
	{
		private string m_strFromName;

		private string m_strMessage;
		private string m_strToUser;

		public ConnSMS(string strFromName)
		{
			m_strFromName = strFromName;
			m_type = ConnectionType.SMS;
		}

		public override bool Connect()
		{
			if (!base.Connect())
				return false;

			_OnSignedOn();
			return true;
		}
		
		public override bool IsBuddyOnline(string strBuddy)
		{
			return true; // assume that we can always send an SMS
		}

		private void _SendMessage()
		{
			if (m_strToUser == "" || m_strMessage == "")
				return;

			if (m_strMessage.Length  > 124)
				m_strMessage = m_strMessage.Substring(0,124);

			string postData = @"xmlsms=
			<SMS_EVERYWHERE>
				<auth>
					<user>Demo Page</user>
					<key>[sms][demo][20040523120917]8c6ed09bbcd13a043ce7c01e6c2393d9841caef037d43c485fc56559d511a099d0e44197be4de607</key>
				</auth>
				<message>
					<recipients><phone>%user%</phone></recipients>
					<subject>RemindMe Message</subject>
					<msgbody>%msg%</msgbody>
					<fromemail>%from%</fromemail>
					<priority></priority>
				</message>
			</SMS_EVERYWHERE>";

			postData = postData.Replace(@"%user%",m_strToUser);
			postData = postData.Replace(@"%msg%",m_strMessage);
			postData = postData.Replace(@"%from%",m_strFromName);

			// clear these as soon as we're done with 'em
			m_strToUser = "";
			m_strMessage = "";

			IPHostEntry IPHost = Dns.Resolve("http://api.smseverywhere.com");
			string []aliases = IPHost.Aliases; 

			IPAddress[] addr = IPHost.AddressList;
			EndPoint ep = new IPEndPoint(addr[0],80); 

			Socket sock = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
			sock.Connect(ep);

			string Get = "POST /v2/xml/ HTTP/1.0\r\n" +
				"Host: api.smseverywhere.com:80\r\n"+
				"Content-Type: application/x-www-form-urlencoded\r\n"+
				"Content-Length: "+postData.Length.ToString()+"\r\n\r\n"+
				postData;

			Encoding ASCII = Encoding.ASCII;
			Byte[] ByteGet = ASCII.GetBytes(Get);
			Byte[] RecvBytes = new Byte[256];
			sock.Send(ByteGet, ByteGet.Length, 0);
			Int32 bytes = sock.Receive(RecvBytes, RecvBytes.Length, 0);
			String strRetPage = null;
			strRetPage = strRetPage + ASCII.GetString(RecvBytes, 0, bytes);
			while (bytes > 0)
			{
				bytes = sock.Receive(RecvBytes, RecvBytes.Length, 0);
				strRetPage = strRetPage + ASCII.GetString(RecvBytes, 0, bytes);
			}

			sock.Shutdown(SocketShutdown.Both);
			sock.Close();
		}


		public override void SendMessage(string strUser, string strMessage)
		{	
			m_strToUser = strUser;
			m_strMessage = strMessage;
			Thread t = new Thread(new ThreadStart(_SendMessage));
			t.Start();
			base.SendMessage(strUser,strMessage);
		}
	}
	#endregion

	public class ConnEmail : Connection
	{
		private System.Timers.Timer checkMailTimer;
		private int checkMailInterval = 1; // in minutes

		private Pop3 m_pop3;
		private string m_strServer;
		
		public ConnEmail(string strLogin, string strPW, string strServer) : base()
		{	
			m_strLogin = strLogin;
			m_strPassword = strPW;
			m_strServer = strServer;
			m_type = ConnectionType.EMAIL;
		}

		public override bool Connect()
		{
			if (!base.Connect())
				return false;

			
			checkMailTimer = new System.Timers.Timer((1000*60)*checkMailInterval);
			checkMailTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckEmail);
			checkMailTimer.Start();
            CheckEmail(null, null);
			
			_OnSignedOn();

			return true;
		}	

		private void CheckEmail(object sender,System.Timers.ElapsedEventArgs args)
		{
			try
			{
				m_pop3 = new Pop3();
				m_pop3.Connect(m_strServer, m_strLogin, m_strPassword);
				ArrayList list = m_pop3.List();

				foreach (Pop3Message msg in list)
				{
					Pop3Message msg2 = m_pop3.Retrieve(msg);
                    m_pop3.Delete(msg);

                    _OnMessageReceived(msg2.From,msg2.Subject,false);
				}

				m_pop3.Disconnect();
			}
			catch(Pop3Exception e)
			{
				_OnError(this,e.Message);
				return;
			}
			
		}

		public override void SendMessage(string strUser, string strMessage)
		{
			if (strMessage.Length == 0)
				return;

			base.SendMessage(strUser,strMessage);
			
			MailMessage msg = new MailMessage();
			msg.EmailFrom = m_strLogin;
			msg.AddEmailTo(strUser);
			msg.EmailMessageType = MessageType.Text;
			//msg.EmailMessage = strMessage+"\r\n\r\nThis email is an automated response to a message sent to RemindMe from your account. You can change your account settings by logging in at www.remindme.cc";
			msg.EmailMessage = strMessage;
			msg.EmailSubject = "RemindMe Message";

			Smtp smtp = new Smtp();
			smtp.SmtpServer = "remindme.cc";
			smtp.SmtpUser = m_strLogin;
			smtp.SmtpPassword = m_strPassword;
            smtp.SmtpPort = 26;
			smtp.SendEmail(msg);

		}

		public override bool IsBuddyOnline(string strBuddy)
		{
			return true; // we can always send email
		}

	}

    public class ConnJabber : Connection
    {
        private jabber.client.JabberClient _client = new JabberClient();
        private jabber.client.PresenceManager presenceManager = new PresenceManager();
        private jabber.client.RosterManager rosterManager = new RosterManager();

        public ConnJabber(string strLogin, string strPW)
        {
            _client.User = strLogin;
            _client.Password = strPW;

            _client.Server = @"gmail.com";

            //_client.AutoReconnect = 3F;
            //_client.AutoStartCompression = true;
            //_client.AutoStartTLS = true;
            ////_client.AutoPresence = true;
            //_client.KeepAlive = 30F;
            //_client.LocalCertificate = null;

            //_client.OnRegisterInfo += new jabber.client.RegisterInfoHandler(_client_OnRegisterInfo);
            _client.OnMessage += new jabber.client.MessageHandler(_client_OnMessage);
            _client.OnError += new bedrock.ExceptionHandler(_client_OnError);
            _client.OnConnect += new jabber.connection.StanzaStreamHandler(_client_OnConnect);
            
            //_client.OnIQ += new jabber.client.IQHandler(_client_OnIQ);
            //_client.OnAuthenticate += new bedrock.ObjectHandler(_client_OnAuthenticate);
            //_client.OnStreamError += new jabber.protocol.ProtocolHandler(_client_OnStreamError);

            //_client.OnDisconnect += new bedrock.ObjectHandler(_client_OnDisconnect);
            //_client.OnAuthError += new jabber.protocol.ProtocolHandler(_client_OnAuthError);
            //_client.OnRegistered += new jabber.client.IQHandler(_client_OnRegistered);
            //_client.OnBeforePresenceOut += new PresenceHandler(_client_OnBeforePresenceOut);
            
            _client.OnPresence += new PresenceHandler(_client_OnPresence);
            _client.OnReadText += new bedrock.TextHandler(_client_OnReadText);
            
            

            m_strLogin = strLogin;
            m_strPassword = strPW;
            m_type = ConnectionType.JABBER;
        }

        void _client_OnBeforePresenceOut(object sender, jabber.protocol.client.Presence pres)
        {
            //throw new NotImplementedException();
        }

        void _client_OnRegistered(object sender, jabber.protocol.client.IQ iq)
        {
            //throw new NotImplementedException();
        }

        void _client_OnAuthError(object sender, System.Xml.XmlElement rp)
        {
            //throw new NotImplementedException();
        }

        void _client_OnDisconnect(object sender)
        {
            base.Disconnect();

            Connect();
        }

        void _client_OnStreamError(object sender, System.Xml.XmlElement rp)
        {
            //throw new NotImplementedException();
        }

        void _client_OnAuthenticate(object sender)
        {
            //throw new NotImplementedException();
        }

        void _client_OnIQ(object sender, jabber.protocol.client.IQ iq)
        {
            //throw new NotImplementedException();
        }

        bool _client_OnRegisterInfo(object sender, jabber.protocol.iq.Register register)
        {
            //throw new NotImplementedException();
            return true;
        }

        void _client_OnError(object sender, Exception ex)
        {
            
        }

        void _client_OnReadText(object sender, string txt)
        {
            //throw new NotImplementedException();
        }

        void _client_OnPresence(object sender, jabber.protocol.client.Presence pres)
        {
            if (pres.From == null)
                return;

            if (pres.Type == jabber.protocol.client.PresenceType.available ||
                        pres.Type == jabber.protocol.client.PresenceType.subscribe ||
                        pres.Type == jabber.protocol.client.PresenceType.subscribed)
            {
                _OnUpdateBuddy(pres.From.Bare, true);
            }
            else
                _OnUpdateBuddy(pres.From.Bare, false);
        }

        void _client_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            try
            {
                if (msg.Body != null && msg.From != null)
                {
                    _OnMessageReceived(msg.From.Bare, msg.Body, false);
                }
            }
            catch (Exception ex)
            {
                
            }
            //if (msg.X != null && msg.X.NextSibling != null &&
            //        msg.X.NextSibling.Name.ToLower() == @"active" || msg.X.NextSibling.Name.ToLower() == @"body"
            //            || msg.X.NextSibling.Name.ToLower() == @"#text")
            //{
            //    //_OnMessageReceived(msg.From.Bare, msg.Body, false);
            //}
        }

        void _client_OnConnect(object sender, jabber.connection.StanzaStream stream)
        {
            if (m_status == ConnectionStatus.Offline)
                _OnSignedOn();
        }
 
        public override bool Connect()
        {
            if (!base.Connect())
                return false;

            _client.Connect();

            return true;
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _client.Close();
        }

        public override void SendMessage(string strUser, string strMessage)
        {
            base.SendMessage(strUser, strMessage);
            _client.Message(strUser, strMessage);
        }

        public override void AddBuddies(string[] strBuddies)
        {
            foreach (string strBuddy in strBuddies)
            {
                _client.Subscribe(new jabber.JID(strBuddy), strBuddy, new string[0]);
            }
        }

        public override void UpdateBuddyList()
        {
            AddBuddies(m_BuddyList);
        }
    }

	public class ConnTOC2 : Connection
	{
		TOC2 m_toc2;

		public ConnTOC2(string strLogin, string strPW) : base()
		{
			m_strLogin = strLogin;
			m_strPassword = strPW;
			m_toc2 = new TOC2(m_strLogin,m_strPassword);

			m_toc2.OnUpdateBuddy +=new dotTOC2.TOC2.OnUpdateBubbyHandler(_OnUpdateBuddy);
			m_toc2.OnSignedOn += new dotTOC2.TOC2.OnSignedOnHandler(_OnSignedOn);
			m_toc2.OnIMIn +=new dotTOC2.TOC2.OnIMInHandler(_OnMessageReceived);
			m_toc2.OnReconnect +=new dotTOC2.TOC2.OnReconnectHandler(_OnReconnect);
			m_toc2.OnDisconnect += new dotTOC2.TOC2.OnDisconnectHandler(_OnDisconnect);
			m_toc2.AutoReconnect = true;
			m_type = ConnectionType.AIM;
		}

		public override bool Connect()
		{
			if (!base.Connect())
				return false;

			m_toc2.Connect();

			return true;
		}
		
		public override void Disconnect()
		{
			base.Disconnect();
			m_toc2.Disconnect();
		}
		
		public override void SendMessage(string strUser, string strMessage)
		{
			if (strMessage.Length == 0)
				return;

			base.SendMessage (strUser, strMessage);
			m_toc2.SendMessage(strUser,strMessage);
		}

		public override void AddBuddies(string [] strBuddies)
		{
			if (m_toc2.Connected)
				m_toc2.AddBuddies(strBuddies);
			else
			{
				m_toc2.Disconnect();
				Thread.Sleep(1000);
				m_toc2.Connect();
			}
		}
		
		public override void UpdateBuddyList()
		{
			AddBuddies(m_BuddyList);
		}

        public override void SendServerData(object data)
        {
            string strData = data as string;

            if (strData != null)
            {
                m_toc2.Send(strData);
            }
        }
	}

/*	#region ConnOscar Class
	public class ConnOscar : Connection
	{
		OSCAR m_oscar;

		public ConnOscar(string strLogin, string strPW) : base()
		{	
			m_strLogin = strLogin;
			m_strPassword = strPW;
			m_oscar = new OSCAR(strLogin,strPW);

			m_oscar.OnUpdateBuddy += new DotOSCAR.OSCAR.OnUpdateBubbyHandler(_OnUpdateBuddy);
			m_oscar.OnSignedOn += new DotOSCAR.OSCAR.OnSignedOnHandler(_OnSignedOn);
			m_oscar.OnIMIn += new DotOSCAR.OSCAR.OnIMInHandler(_OnMessageReceived);
			m_oscar.OnReconnect += new DotOSCAR.OSCAR.OnReconnectHandler(_OnReconnect);
			m_oscar.OnDisconnect += new DotOSCAR.OSCAR.OnDisconnectHandler(_OnDisconnect);
			m_oscar.AutoReconnect = true;
			m_type = ConnectionType.AIM;
		}

		public override bool Connect()
		{
			if (!base.Connect())
				return false;

			return m_oscar.Connect();
		}	

		public override void Disconnect()
		{
			base.Disconnect();
			m_oscar.Disconnect();			
		}

		public override void SendMessage(string strUser, string strMessage)
		{
			if (strMessage.Length == 0)
				return;

			base.SendMessage(strUser,strMessage);
			m_oscar.SendMessage(strUser,strMessage);
		}

		public override void AddBuddies(string [] strBuddies)
		{
			if (m_oscar.Connected)
				m_oscar.AddBuddies(strBuddies);
			else if (!m_oscar.Connected)
			{
				m_oscar.Disconnect();
				Thread.Sleep(1000);
				m_oscar.Connect();
			}
		}

		public override void UpdateBuddyList()
		{
			AddBuddies(m_BuddyList);
		}
	}
	#endregion
*/
	#region ConnYahoo Class
	public class ConnYahoo : Connection
	{
		Yahoo m_yahoo;

		public ConnYahoo(string strLogin, string strPW) : base()
		{
			m_strLogin = strLogin;
			m_strPassword = strPW;
			m_yahoo = new Yahoo(strLogin,strPW);

			m_yahoo.OnUpdateBuddy += new dotYahoo.Yahoo.OnUpdateBuddyHandler(_OnUpdateBuddy);
			m_yahoo.OnSignedOn += new dotYahoo.Yahoo.OnSignedOnHandler(_OnSignedOn);
			m_yahoo.OnIMIn += new dotYahoo.Yahoo.OnIMInHandler(_OnMessageReceived);
			m_yahoo.OnReconnect += new dotYahoo.Yahoo.OnReconnectHandler(_OnReconnect);
			m_yahoo.OnDisconnect += new dotYahoo.Yahoo.OnDisconnectHandler(_OnDisconnect);
			m_yahoo.AutoReconnect = true;

			m_type = ConnectionType.YAHOO;
		}

		public override bool Connect()
		{
			if (!base.Connect())
				return false;
 
			return m_yahoo.Connect();
		}	

		public override void Disconnect()
		{
			base.Disconnect();
			m_yahoo.Disconnect();			
		}

		public override void SendMessage(string strUser, string strMessage)
		{
			if (strMessage.Length == 0)
				return;

			base.SendMessage(strUser,strMessage);
			m_yahoo.SendMessage(strUser,strMessage);
		}


		public override void AddBuddies(string [] strBuddies)
		{
			if (!m_yahoo.Connected)
			{
				m_yahoo.Disconnect();
				Thread.Sleep(1000);
				m_yahoo.Connect();
			}
			else
			{
				foreach (string buddy in strBuddies)
				{
					m_yahoo.AddBuddy(buddy,"Friends");
					Thread.Sleep(1500);
				}
			}
		}

		public override void UpdateBuddyList()
		{
			AddBuddies(m_BuddyList);
		}

	}
	#endregion

	#region ConnICQ Class
	public class ConnICQ : Connection
	{
		ICQ m_icq;

		public ConnICQ(string strLogin, string strPW) : base()
		{
			m_strLogin = strLogin;
			m_strPassword = strPW;
			m_icq = new ICQ(strLogin,strPW);

			m_icq.OnUpdateBuddy += new dotICQ.ICQ.OnUpdateBuddyHandler(_OnUpdateBuddy);
			m_icq.OnSignedOn += new dotICQ.ICQ.OnSignedOnHandler(_OnSignedOn);
			m_icq.OnIMIn += new dotICQ.ICQ.OnIMInHandler(_OnMessageReceived);
			m_icq.OnReconnect += new dotICQ.ICQ.OnReconnectHandler(_OnReconnect);
			m_icq.OnDisconnect += new dotICQ.ICQ.OnDisconnectHandler(_OnDisconnect);
			m_icq.AutoReconnect = true;
			m_type = ConnectionType.ICQ;
		}

		public override bool Connect()
		{
			if (!base.Connect())
				return false;
 
			m_icq.Connect();
			return true; // what?!?
		}	

		public override void Disconnect()
		{
			base.Disconnect();
			m_icq.Disconnect();			
		}

		public override void SendMessage(string strUser, string strMessage)
		{
			if (strMessage.Length == 0)
					return;

			base.SendMessage(strUser,strMessage);
			m_icq.SendMessage(strUser,strMessage);
		}
		
		public override void AddBuddies(string [] strBuddies)
		{
			if (m_icq.Connected)
				m_icq.AddBuddies(strBuddies);
			else
			{
				m_icq.Disconnect();
				Thread.Sleep(1000);
				m_icq.Connect();
			}
		}

		public override void UpdateBuddyList()
		{
			AddBuddies(m_BuddyList);
		}
	}
	#endregion

    #region ConnMSN Class (OLD)
//    public class ConnMSN : Connection
//    {
//        DotMSN.Messenger m_msn;
//        Hashtable m_convos;

//        public ConnMSN(string strLogin, string strPW) : base()
//        {
//            m_convos = new Hashtable();

//            m_strLogin = strLogin;
//            m_strPassword = strPW;
			
//            InitMSN();
//        }

//        public void InitMSN()
//        {
//            m_msn = new Messenger();
//            m_msn.ConnectionFailure += new DotMSN.Messenger.ConnectionFailureHandler(OnConnectionFailure);
//            m_msn.ConversationCreated += new Messenger.ConversationCreatedHandler(ConversationCreated);
//            m_msn.ContactOffline += new DotMSN.Messenger.ContactOfflineHandler(UserSignedOff);
//            m_msn.ContactOnline += new DotMSN.Messenger.ContactOnlineHandler(UserSignedOn);
//            m_msn.SynchronizationCompleted += new DotMSN.Messenger.SynchronizationCompletedHandler(OnSynchronizationCompleted);
//            m_type = ConnectionType.MSN;
//        }

//        private void OnConnectionFailure(Messenger sender, ConnectionErrorEventArgs ar)
//        {
//            _OnDisconnect();
//        }

//        private void OnSynchronizationCompleted(Messenger sender, EventArgs e)
//        {
//            m_msn.SetStatus(MSNStatus.Online);
//        }

//        public override bool Connect()
//        {
//            try
//            {
//                base.Connect();
//                //ChangeStatus(ConnectionStatus.Connecting);
//                Console.WriteLine("U: ({0}) P: ({1})", m_strLogin, m_strPassword);
//                Thread.Sleep(50); // is this really needed? 
//                m_msn.Connect(m_strLogin,m_strPassword);
//                m_msn.Owner.Privacy = MSNPrivacy.AllExceptBlocked;
//            }
//            catch (MSNException e)
//            {
//                _OnError(this,e.Message);
//                return false;
//            }

//            if (m_msn.Connected)
//            {
//                _OnSignedOn();
//                m_msn.SynchronizeList();
//                return true;
//            }
//            return false;
//        }

//        public override void Disconnect()
//        {
//            base.Disconnect();
//            m_msn.CloseConnection();

//            // reset the m_msn object
//            InitMSN();			
//        }

//        private void ConversationCreated(Messenger sender, ConversationEventArgs e)
//        {
//            e.Conversation.MessageReceived += new Conversation.MessageReceivedHandler(MessageHandler);
//            e.Conversation.ContactJoin += new Conversation.ContactJoinHandler(ContactJoined);
//            e.Conversation.ContactLeave += new Conversation.ContactLeaveHandler(ContactLeave);
//        }

//        private void ContactLeave(Conversation sender, ContactEventArgs e)
//        {
//            m_convos[e.Contact.Mail] = null;
//        }

//        private void ContactJoined(Conversation sender, ContactEventArgs e)
//        {
//            m_convos[e.Contact.Mail] = sender;
			
//            if (sender.ClientData != null)
//            {
//                SendMessage(e.Contact.Mail,(string)sender.ClientData);
//                sender.ClientData = null;
//            }
//        }

//        private void MessageHandler(Conversation sender, MessageEventArgs args)
//        {
//            DotMSN.Message msg = args.Message;
//            Contact con = args.Sender;

//            if (args.Sender.Status != MSNStatus.Away)
//            {
//                _OnMessageReceived(con.Mail,msg.Text,false);
//            }
//        }

//        public override void SendMessage(string strUser, string strMessage)
//        {
//            if (m_convos[strUser] != null)
//            {
//                Conversation conv = (Conversation) m_convos[strUser];
//                conv.SendMessage(strMessage);
//                base.SendMessage(strUser,strMessage);
//            }
//            else
//                m_msn.RequestConversation(strUser,strMessage);
//        }

//        private void UserSignedOn(Messenger sender, ContactEventArgs e)
//        {
//            _OnUpdateBuddy(e.Contact.Mail,true);
//        }

//        private void UserSignedOff(Messenger sender, ContactEventArgs e)
//        {
//            m_convos[e.Contact.Mail] = null;
//            _OnUpdateBuddy(e.Contact.Mail,false);
//        }

////		private void OnConnectionFailure(Messenger sender, ConnectionErrorEventArgs e)
////		{
////			//OnDisconnect();
////		}

//        public override void AddBuddies(string [] strBuddies)
//        {
//            foreach (string strBuddy in strBuddies)
//            {
//                //m_toc.AddBuddies(strBuddies);
//                m_msn.AddContact(strBuddy);
//                Thread.Sleep(1000);
//            }
//            //m_msn.AddContact(strBuddies[2]);
//            //m_msn.
//            //m_msn.SynchronizeList();
//        }

//        public override void UpdateBuddyList()
//        {
//            AddBuddies(m_BuddyList);
//        }
//    }
    #endregion

    #region ConnMSN Class
    public class ConnMSN : Connection
    {
        dotMSN.MSNController m_msn;
        Hashtable m_convos;
        Hashtable m_msgque = new Hashtable();

        public ConnMSN(string strLogin, string strPW)
            : base()
        {
            m_convos = new Hashtable();

            m_strLogin = strLogin;
            m_strPassword = strPW;

            InitMSN();
        }

        public void InitMSN()
        {
            m_msn = new DNBSoft.MSN.ClientController.MSNController();

            m_msn.LoginStatusChanged += new dotMSN.MSNEventDelegates.LoginStatusChangedEventDelegate(OnSignedIn);
            m_msn.SwitchboardController.SwitchboardCreated += new dotMSN.MSNEventDelegates.SwitchboardCreated(SwitchboardController_SwitchboardCreated);
            m_msn.ContactStatusChanged += new DNBSoft.MSN.ClientController.MSNEventDelegates.ContactStatusChangedEventDelegate(m_msn_ContactStatusChanged);

            //m_msn = new DotMSN.Messenger();

            //m_msn.Credentials.ClientID = "msmsgs@msnmsgr.com";
            //m_msn.Credentials.ClientCode = "Q1P7W2E4J9R8U3S5"; 
            
            //m_msn.Nameserver.SignedIn += new EventHandler(OnSignedIn);
            //m_msn.Nameserver.SignedOff += new DotMSN.SignedOffEventHandler(OnConnectionFailure);
            //m_msn.ConversationCreated += new DotMSN.ConversationCreatedEventHandler(OnConversationCreated);	

            //m_msn.ConversationCreated += new Messenger.ConversationCreatedHandler(ConversationCreated);
            //m_msn.ContactOffline += new DotMSN.Messenger.ContactOfflineHandler(UserSignedOff);
            //m_msn.ContactOnline += new DotMSN.Messenger.ContactOnlineHandler(UserSignedOn);
            
            m_type = ConnectionType.MSN;
        }

        void m_msn_ContactStatusChanged(string username, DNBSoft.MSN.ClientController.MSNEnumerations.UserStatus status)
        {
            if (status == DNBSoft.MSN.ClientController.MSNEnumerations.UserStatus.online)
                _OnUpdateBuddy(username, true);
            else if (status == DNBSoft.MSN.ClientController.MSNEnumerations.UserStatus.offline)
                _OnUpdateBuddy(username, false);
        }

        void SwitchboardController_SwitchboardCreated(DNBSoft.MSN.ClientController.MSNSwitchboard switchboard)
        {
            switchboard.MessageRecieved += new DNBSoft.MSN.ClientController.MSNEventDelegates.MessageRecieved(switchboard_MessageRecieved);
            switchboard.UserConnected += new DNBSoft.MSN.ClientController.MSNEventDelegates.SwitchboardUserConnected(switchboard_UserConnected);
        }

        void switchboard_UserConnected(dotMSN.MSNSwitchboard switchboard, string username, bool joined)
        {
            if (joined)
            {
                m_convos[username] = switchboard;

                if (m_msgque[username] != null)
                {
                    ArrayList msgs = m_msgque[username] as ArrayList;
                    if (msgs != null)
                    {
                        foreach (string strMessage in msgs)
                        {
                            switchboard.sendMessage(new dotMSN.MSNUserOutgoingMessage(@"Times New Roman", strMessage));
                        }

                        m_msgque.Remove(username);
                    }
                }
            }
            else
            {
                if (m_convos[username] != null)
                    m_convos.Remove(username);
            }
        }


        void switchboard_MessageRecieved(DNBSoft.MSN.ClientController.MSNUserMessage message)
        {
            if (message.getMessageType() == dotMSN.MSNEnumerations.UserMessageType.incomming_text_message)
            {
                _OnMessageReceived(message.getUsername(), message.getUserPayload(), false);
            }
        }


   
        //private void OnConnectionFailure(object sender, XihSolutions.DotMSN.SignedOffEventArgs e)
        //{
        //    _OnDisconnect();
        //}

        private void OnSignedIn(DNBSoft.MSN.ClientController.MSNEnumerations.LoginStatus newStatus)
        {
            if (newStatus == DNBSoft.MSN.ClientController.MSNEnumerations.LoginStatus.LOGGED_IN)
            {
                _OnSignedOn();
                m_msn.Status = DNBSoft.MSN.ClientController.MSNEnumerations.UserStatus.online;
            }
            else if (newStatus == dotMSN.MSNEnumerations.LoginStatus.LOGGED_OUT && m_msn.Status != dotMSN.MSNEnumerations.UserStatus.online)
            {
                Connect();
            }
        }

        public override bool Connect()
        {
            try
            {
                base.Connect();
                m_msn.Username = m_strLogin;
                m_msn.Password = m_strPassword;
                m_msn.LoginStatus = DNBSoft.MSN.ClientController.MSNEnumerations.LoginStatus.LOGGED_IN;

                //ChangeStatus(ConnectionStatus.Connecting);
                //Console.WriteLine("U: ({0}) P: ({1})", m_strLogin, m_strPassword);
        //        m_msn.Credentials = new XihSolutions.DotMSN.Credentials(m_strLogin, m_strPassword, @"msmsgs@msnmsgr.com", @"Q1P7W2E4J9R8U3S5");
        //        m_msn.Connect();

        //        //m_msn.Connect(m_strLogin, m_strPassword);
        //        //m_msn.Owner.Privacy = DotMSN.PrivacyMode.AllExceptBlocked;
            }
            catch (Exception  e)
            {
                _OnError(this, e.Message);
                return false;
            }

        //    if (m_msn.Connected)
        //    {
        //        _OnSignedOn();
        //        //m_msn.SynchronizeList();
        //        m_msn.Nameserver.SynchronizeContactList();
        //        return true;
        //    }
            return false;
        }

        //public override void Disconnect()
        //{
        //    base.Disconnect();
        //    //m_msn.CloseConnection();
        //    m_msn.NameserverProcessor.Disconnect();

        //    // reset the m_msn object
        //    InitMSN();
        //}

        //private void OnConversationCreated(object sender, DotMSN.ConversationCreatedEventArgs e)
        //{
        //    // if e.Initiator is null, then this convo was created by a remote user
        //    // otherwise it was created by us!
        //    if (e.Initiator == null)
        //    {
        //        Console.WriteLine("MSN: Conversation initiated by remote user...");

        //    }
        //    else
        //    {
        //        Console.WriteLine("MSN: Conversation initiated locally...");

        //    }
            
        //    //e.Conversation.Switchboard.TextMessageReceived += new XihSolutions.DotMSN.TextMessageReceivedEventHandler(MessageHandler);

        //    //e.Conversation.MessageReceived += new Conversation.MessageReceivedHandler(MessageHandler);
        //    //e.Conversation.ContactJoin += new Conversation.ContactJoinHandler(ContactJoined);
        //    //e.Conversation.ContactLeave += new Conversation.ContactLeaveHandler(ContactLeave);
        //}

        ////private void ContactLeave(Conversation sender, ContactEventArgs e)
        ////{
        ////    m_convos[e.Contact.Mail] = null;
        ////}

        //private void ContactJoined(DotMSN.Conversation sender, DotMSN.ContactEventArgs e)
        //{
        //    //m_convos[e.Contact.Mail] = sender;

        //    //if (sender.ClientData != null)
        //    //{
        //    //    SendMessage(e.Contact.Mail, (string)sender.ClientData);
        //    //    sender.ClientData = null;
        //    //}
        //}

        //private void MessageHandler(object sender, XihSolutions.DotMSN.TextMessageEventArgs e)
        //{
            
        //    DotMSN.TextMessage msg = e.Message;
        //    DotMSN.Contact con = e.Sender;

        //    if (e.Sender.Status != XihSolutions.DotMSN.PresenceStatus.Away)
        //    {
        //        _OnMessageReceived(con.Mail, msg.Text, false);
        //    }
        //}

        public override void SendMessage(string strUser, string strMessage)
        {
            base.SendMessage(strUser, strMessage);

            if (m_convos[strUser] != null)
            {
                dotMSN.MSNSwitchboard sb = m_convos[strUser] as dotMSN.MSNSwitchboard;

                if (sb != null)
                {
                    sb.sendMessage(new dotMSN.MSNUserOutgoingMessage(@"Times New Roman", strMessage));
                }
            }
            else
            {
                List<string> users = new List<string>();
                users.Add(strUser);
                m_msn.startConversation(users);

                if (m_msgque[strUser] == null)
                {
                    ArrayList msglist = new ArrayList();
                    msglist.Add(strMessage);
                    m_msgque[strUser] = msglist;
                }
                else
                {
                    ArrayList msglist = m_msgque[strUser] as ArrayList;
                    if (msglist != null)
                        msglist.Add(strMessage);
                }
            }
        }

        public override void SendServerData(object data)
        {
            base.SendServerData(data);
            m_msn.MasterSocketWrapper.SendRawMessage(data as string);
        }

        ////private void UserSignedOn(Messenger sender, ContactEventArgs e)
        ////{
        ////    _OnUpdateBuddy(e.Contact.Mail, true);
        ////}

        ////private void UserSignedOff(Messenger sender, ContactEventArgs e)
        ////{
        ////    m_convos[e.Contact.Mail] = null;
        ////    _OnUpdateBuddy(e.Contact.Mail, false);
        ////}

        //////		private void OnConnectionFailure(Messenger sender, ConnectionErrorEventArgs e)
        //////		{
        //////			//OnDisconnect();
        //////		}

        public override void AddBuddies(string[] strBuddies)
        {
            foreach (string strBuddy in strBuddies)
            {
                m_msn.ContactsList.AddBuddy(strBuddy);
                //m_msn.ContactsList.Contacts.
                //m_toc.AddBuddies(strBuddies);
                //m_msn.AddContact(strBuddy);
                //Thread.Sleep(1000);
            }
            //m_msn.AddContact(strBuddies[2]);
            //m_msn.
            //m_msn.SynchronizeList();
        }

        public override void UpdateBuddyList()
        {
            AddBuddies(m_BuddyList);
        }
    }
    #endregion
}
