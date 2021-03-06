using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Timers;

namespace dotTOC2
{

	/// <summary>
	/// The first 6 bytes of every message sent from Client -> TOC and TOC -> Client
	/// </summary>
	struct flap_header 
	{
		public char asterisk;
		public byte frametype;
		public short seqno;
		public short datalen;
	};

	public enum NameFormat { Raw,Normalized }
	public enum PasswordFormat { Raw,Roasted }

	/// <summary>
	/// class for the screen name signed on using the TOC class
	/// </summary>
	public class TOCUser
	{


		private string m_strName;
		private string m_strPW;
		
		public TOCUser()
		{
		}

		public TOCUser(string strName, string strPW)
		{
			m_strName = strName;
			m_strPW = strPW;
		}

		public string GetName()
		{
			return GetName(NameFormat.Normalized);
		}

		public string GetName(NameFormat nt)
		{
			if (nt == NameFormat.Normalized)
				return Normalize(m_strName);

			return m_strName;
		}

		public string GetPassword()
		{
			return GetPassword(PasswordFormat.Roasted);
		}

		public string GetPassword(PasswordFormat pt)
		{
			if (pt == PasswordFormat.Roasted)
				return RoastedString(m_strPW);

			return m_strPW;
		}


		/// <summary>
		/// Returns a normalized version of the string, will concate the string to 16 chars
		/// if necessary
		/// </summary>
		public static string Normalize(string strScreenName)
		{
			string strName= strScreenName;
			strName = Regex.Replace(strName," ","");
			strName = strName.ToLower();
			
			if (strName.Length > 16)
                strName = strName.Remove(16,strName.Length-16);

			return strName;
		}

		public static string RoastedString(string strOrig)
		{
			const string roaster = "Tic/TocTic/TocTic/TocTic/Toc";
			string retStr = "0x";

			for (int i=0 ; i < strOrig.Length; i++) 
			{
				retStr += string.Format("{0:x2}",strOrig[i] ^ roaster[i]);
			}

			return retStr;
		}
	}


	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class TOC2
	{
		// properties
		public string m_strInfo = "dotTOC2 - .NET TOC2 Library";

		public bool Connected
		{
			get { 
					if (m_socket != null ) 
						return m_socket.Connected; 
					else
						return false;
				}
		}

		public bool AutoReconnect
		{
			get { return m_bAutoReconnect; }
			set { m_bAutoReconnect = value; }
		}

		private bool m_bAutoAddBuddies = false;
		public bool AutoAddBuddies
		{
			get { return m_bAutoAddBuddies; }
			set { m_bAutoAddBuddies = value; }
		}

		// delegates/callbacks
		public delegate void OnErrorHandler(string strError);
		public event OnErrorHandler OnError;

		public delegate void OnDisconnectHandler();
		public event OnDisconnectHandler OnDisconnect;

		public delegate void OnReconnectHandler();
		public event OnReconnectHandler OnReconnect;

		public delegate void OnSignedOnHandler();
		public event OnSignedOnHandler OnSignedOn;

		public delegate void OnIMInHandler(string strUser, string strMsg, bool bAuto);
		public event OnIMInHandler OnIMIn;

		public delegate void OnUpdateBubbyHandler(string strUser, bool bOnline);
		public event OnUpdateBubbyHandler OnUpdateBuddy;

		public delegate void OnEviledHandler(int iLvl, bool bAnonymous, string strSender);
		public event OnEviledHandler OnEviled;

		public delegate void OnIncomingHandler(string strMessage);
		public event OnIncomingHandler OnIncoming;

		public delegate void OnServerMessageHandler(string strIncoming);
		public event OnServerMessageHandler OnServerMessage;

		public delegate void OnChatJoinedHandler(string strRoomID, string strRoomName);
		public event OnChatJoinedHandler OnChatJoined;

		// data types received from server
		private const byte FT_SIGNON	= 1;
		private const byte FT_DATA		= 2;
		private const byte FT_ERROR		= 3;	// not used by TOC
		private const byte FT_SIGNOFF	= 4;	// not used by TOC
		private const byte FT_KEEPALIVE	= 5;

		// privates
		private bool m_bDCOnPurpose = false;
		private bool m_bAutoReconnect = false;
		private Socket m_socket;
		private TOCUser user;
		private string m_strServer = "toc.oscar.aol.com";
		private int m_iPort = 9898;
		private Byte[] m_byBuff = new Byte[32767];
		private int m_iSeqNum;

		private bool m_bGetLineFailure = false;
		private bool m_bGetLineWait = false;
		private string m_GetLineString = "";
		

		private System.Timers.Timer m_Timer;


		#region contructors
		public TOC2()
		{
			user = new TOCUser();
		}

		public TOC2(string strName, string strPW)
		{
			user = new TOCUser(strName,strPW);
		}

		public TOC2(string strServer, int iPort)
		{
			m_strServer = strServer;
			iPort = m_iPort;
		}
		#endregion constructors

		#region private_functions
		public string [] GetConfigBuddies(string strConfig)
		{	
			ArrayList names = new ArrayList();

			foreach(string strLine in strConfig.Split('\n'))
			{	
				if (strLine.ToLower().Trim() == "done:")
					break;
				
				if (strLine.StartsWith("b"))
				{
					string strTemp = strLine.Replace("\r",null);
					int i = strTemp.IndexOf(":",0);
					strTemp = strTemp.Remove(0,i+1);
					
					i = strTemp.IndexOf(":",0);
					strTemp = strTemp.Remove(i,strTemp.Length-(i-1));

					names.Add(TOCUser.Normalize(strTemp));
				}
				
			}

			return (string []) names.ToArray(typeof(string));
		}

		private void SendFlapSignOn()
		{
			const int FLAP_VERSION = 1;
			const int TLV_VERSION = 1;
			
			byte [] packet = new byte[255];
			
			int msglen = 8 + user.GetName().Length;
			int packetlen = 6 + msglen;

			Array.Copy(GetFlapHeader(msglen,1),packet,6);
			packet[6] = 0;
			packet[7] = 0;
			packet[8] = 0;
			packet[9] = (byte)BitConverter.ToChar(BitConverter.GetBytes(FLAP_VERSION),0);

			packet[10] = 0;
			packet[11] = (byte)BitConverter.ToChar(BitConverter.GetBytes(TLV_VERSION),0);

			packet[12] = (byte)BitConverter.ToChar(BitConverter.GetBytes(user.GetName().Length),1);
			packet[13] = (byte)BitConverter.ToChar(BitConverter.GetBytes(user.GetName().Length),0);
			
			Array.Copy(Encoding.ASCII.GetBytes(user.GetName()),0,packet,14,user.GetName().Length);
			m_socket.Send(packet,packetlen,0);
		}

		private void SendUserSignOn()
		{	
			string strLogin = user.GetName();
			string strPassword = user.GetPassword(PasswordFormat.Raw);

			int code1 = (strLogin[0] - 96) * 7696 + 738816;
			int code2 = ((strPassword[0] - 96) - 1) * code1 + (strLogin[0] - 96) * 746512 + 71665152;

			string strMsg;
			strMsg = string.Format("toc2_signon {0} {1} {2} {3} {4} {5} {6} {7}",
				"login.oscar.aol.com",
				5190,
				user.GetName(),
				user.GetPassword(),
				"english",
				"\"TIC:QuickBuddy\"",
				160,
				code2);

			Send(strMsg);
		}

		private void Dispatch(string strIncoming)
		{
			if (OnServerMessage != null)
				OnServerMessage(strIncoming);

			Regex r = new Regex("(:)"); // Split on colon
			string[] strArray = r.Split(strIncoming);

			switch (strArray[0])
			{

				case "CONFIG2":
					//if (AutoAddBuddies)
					//	AddBuddies(GetConfigBuddies(strIncoming));
					break;

				case "SIGN_ON":
					Send("toc_add_buddy "+user.GetName());
					Send("toc_set_info \""+m_strInfo+"\"");
					Send("toc_init_done");
					if (OnSignedOn != null)
						OnSignedOn();
					break;

				case "IM_IN2":
					if (OnIMIn != null)
					{
						string strMsg = string.Join("",strArray,8,strArray.Length-8);
						//string strMsg = strArray[8];
						OnIMIn(TOCUser.Normalize(strArray[2]),Regex.Replace(strMsg,@"<(.|\n)*?>",string.Empty),strArray[4] == "T");

						if (m_bGetLineWait)
						{
							m_GetLineString = Regex.Replace(strMsg,@"<(.|\n)*?>",string.Empty);
							m_bGetLineWait = false;
							m_Timer.Stop();
						}
					}
					break;

				case "UPDATE_BUDDY2":
					if (OnUpdateBuddy != null)
					{
						OnUpdateBuddy(TOCUser.Normalize(strArray[2]),strArray[4] == "T");
					}
					break;

				case "EVILED":
					if (OnEviled != null)
					{
						int iLvl = int.Parse(strArray[2]);

						if (strArray.Length == 5)
							OnEviled(iLvl,false,TOCUser.Normalize(strArray[4]));
						else if (strArray.Length == 4)
							OnEviled(iLvl,true,"");
					}
					break;

				case "ERROR":
					if (OnError != null)
					{
						switch(strArray[2])
						{
							case "980":
								OnError(@"Invalid login");
							break;

							default:
								OnError(@"ERROR "+strArray[2]+": There was an unknown error");
							break;
						}
						//OnError(
					}
				break;

				case "CHAT_JOIN":
					if (OnChatJoined != null)
						OnChatJoined(strArray[2],strArray[4]);
				break;
					
				default:
					break;
			}
		}

		private byte[] GetFlapHeader(int iMsgLen, int iFlapType)
		{
			byte [] retVal = new byte[6];
			retVal[0] = (byte)Encoding.ASCII.GetBytes("*")[0];
			retVal[1] = (byte)iFlapType;

			if (iFlapType == 1)
			{
				Random R =new Random();
				m_iSeqNum = R.Next(1,100);
			}
			else
				m_iSeqNum++;

			retVal[2] = (byte)BitConverter.ToChar(BitConverter.GetBytes(m_iSeqNum),1);
			retVal[3] = (byte)BitConverter.ToChar(BitConverter.GetBytes(m_iSeqNum),0);

			retVal[4] = (byte)BitConverter.ToChar(BitConverter.GetBytes(iMsgLen),1);
			retVal[5] = (byte)BitConverter.ToChar(BitConverter.GetBytes(iMsgLen),0);

			return retVal;
		}


		private byte[] GetFlapHeader(int iMsgLen)
		{
			return GetFlapHeader(iMsgLen,2);
		}


		
		private void SendFlapInit()
		{
			byte[] query = Encoding.Default.GetBytes("FLAPON\r\n\r\n");
			m_socket.Send(query);
		}

		private void SetupRecieveCallback (Socket sock)
		{
			try
			{
				AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
				sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None,recieveData, sock);
			}
			catch( Exception ex )
			{
				DispatchError(ex.Message);
			}
		}

		public static string Encode(string strMessage)
		{
			string strRetStr = "";

			for (int i=0;i < strMessage.Length;i++)
			{
				switch(strMessage[i]) 
				{
					case '$':
					case '{':
					case '}':
					case '[':
					case ']':
					case '(':
					case ')':
					case '"':
					case '\\':
						strRetStr += '\\';
						break;
			
					default: 
						break;
				}
				strRetStr += strMessage[i];
			}

			return strRetStr;
		}
	
		private void DispatchError(string strError)
		{
			if (OnError != null)
				OnError(strError);
		}
		
		#endregion private_functions

		#region public_functions

		// TODO: this can be arranged more eloquently
		public bool GetLine(ref string strLine)
		{
			m_bGetLineWait = true;
			m_Timer.Start();

			// TODO: add timeout functionality
			while (m_bGetLineWait);
			m_Timer.Stop();

			if (m_bGetLineFailure)
			{
				strLine = "";
				m_bGetLineFailure = false;
				return false;
			}

			strLine = m_GetLineString;
			return true;
		}

		public void Send(string szMsg)
		{
			const int TOC_BUFFER = 4096;

			byte [] packet = new byte[TOC_BUFFER];
			int msgLen = szMsg.Length+1;
			szMsg += (char)0;

			Array.Copy(GetFlapHeader(msgLen),packet,6);
			Array.Copy(Encoding.Default.GetBytes(szMsg),0,packet,6,msgLen);

			if (m_socket != null && m_socket.Connected)
				m_socket.Send(packet,msgLen+6,0);
		}

		public void SendMessage(string strUser, string strMsg)
		{
			SendMessage(strUser,strMsg,false);
		}

		public void SendMessage(string strUser,string strMsg, bool bAuto)
		{
			string strText;
			strText = string.Format("toc2_send_im {0} \"{1}\"{2}",
				TOCUser.Normalize(strUser),Encode(strMsg),
				bAuto ? " auto" : "");
			Send(strText);
		}

		public void Connect(string strName, string strPW)
		{
			user = new TOCUser(strName,strPW);
			Connect();
		}

		public void Connect()
		{
			IPAddress ip;
			ip = Dns.Resolve(m_strServer).AddressList[0];
			int port = m_iPort;
			
			IPEndPoint remote = new IPEndPoint(ip,port);

			try 
			{
				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				m_socket.Blocking = false ;	
				m_socket.BeginConnect(remote , new AsyncCallback(OnConnect), m_socket);
			}
			catch (Exception er)
			{
				DispatchError(er.Message);
			}
		}

        public void OnConnect(IAsyncResult ar)
		{
			Socket sock = (Socket)ar.AsyncState;

			// Check if we were sucessfull
			try
			{
				if (sock.Connected)
				{
					m_Timer = new System.Timers.Timer(1000*10);
					m_Timer.Elapsed +=new ElapsedEventHandler(TimerElapsed);

					m_bDCOnPurpose = false;
					SendFlapInit();
					SetupRecieveCallback(sock);
				}
				else
					DispatchError("Connection failed.");
			}
			catch( Exception ex )
			{
				DispatchError(ex.Message);
			}  
		}

		public void OnRecievedData( IAsyncResult ar )
		{
			Socket sock = (Socket)ar.AsyncState;

			try
			{
				int nBytesRead = 0;
				int nBytesRec = sock.EndReceive( ar );
				if( nBytesRec > 0 )
				{
					do 	
					{
						flap_header fh = new flap_header();
						fh.asterisk = (char)m_byBuff[nBytesRead+0];
						fh.frametype = (byte)m_byBuff[nBytesRead+1];
					
						byte [] byteTemp = new byte[2];
						byteTemp[1] = m_byBuff[nBytesRead+4];
						byteTemp[0] = m_byBuff[nBytesRead+5];
						fh.datalen = BitConverter.ToInt16(byteTemp,0);
					
						switch (fh.frametype)
						{
							case FT_SIGNON:
								SendFlapSignOn();
								SendUserSignOn();
								break;
						
							case FT_DATA:
								string sRecieved = Encoding.ASCII.GetString(m_byBuff,nBytesRead+6,fh.datalen);
								if (OnIncoming != null)
									OnIncoming(sRecieved);
								Dispatch(sRecieved);
								break;

							default:
								break;
						}		
		
						nBytesRead += fh.datalen + 6;

					} while (nBytesRead < nBytesRec);

					SetupRecieveCallback (sock);
				}
			}
			catch( Exception ex )
			{
				// the connection may have dropped
				if (!sock.Connected && !m_bDCOnPurpose)
				{
					sock.Shutdown(SocketShutdown.Both);
					sock.Close();
					if (OnDisconnect != null)
						OnDisconnect();	
					
					if (AutoReconnect)
					{
						if (OnReconnect != null)
							OnReconnect();
						
						Thread.Sleep(500);

						Connect();
					}
				}
				else
					DispatchError(ex.Message);
			}
		}

		public void AddBuddies(string [] strBuddies)
		{
			// TODO: add a 'toc2_del_group test' command to reset the group?
			string strCommand = "g:test\n";
			foreach (string strName in strBuddies)
			{
				string strTemp = strCommand + "b:"+strName+"\n";

				if (strTemp.Length >= 2048)
				{
					strCommand = "toc2_new_buddies {"+strCommand+"}";
					Send(strCommand);
					Thread.Sleep(150);
					strCommand = "toc2_new_buddies g:test\n";
				}
				else
					strCommand += "b:"+strName+"\n";
			}

			strCommand = "toc2_new_buddies {"+strCommand+"}";
			Send(strCommand);

//			string strCommand = "toc_add_buddy ";
//			foreach (string strName in strBuddies)
//			{
//				string strTemp = strCommand + strName+" ";
//				
//				if (strTemp.Length >= 2048)
//				{
//					Send(strCommand);
//					Thread.Sleep(150);
//					strCommand = "toc_add_buddy ";
//				}
//				else
//					strCommand += strName+" ";
//			}
//			Send(strCommand);
		}

		public void Disconnect()
		{
			m_bDCOnPurpose = true;
			
			if (m_socket != null && m_socket.Connected)
			{
				m_socket.Shutdown(SocketShutdown.Both);
				m_socket.Close();
				if (OnDisconnect != null)
					OnDisconnect();	
			}
		}

        public enum IsAvailableReponse { FALSE, TRUE, TIMEOUT, UNKNOWN }

		#endregion public_functions

		private void TimerElapsed(object sender, ElapsedEventArgs e)
		{
			if (m_bGetLineWait)
			{
				m_bGetLineWait = false;
				m_bGetLineFailure = true;
				m_GetLineString = "";
			}
		}
	}
}
