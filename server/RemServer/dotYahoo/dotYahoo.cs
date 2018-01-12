using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace dotYahoo
{
	/// <summary>
	/// Summary description for Yahoo.
	/// </summary>
	public class Yahoo
	{
		// properties
		private bool m_bAutoReconnect = false;
		public bool AutoReconnect
		{
			get { return m_bAutoReconnect; }
			set { m_bAutoReconnect = value; }
		}

		public bool Connected
		{
			get { return m_socket.Connected; }
		}

		// delegates & callbacks
		public delegate void OnReconnectHandler();
		public event OnReconnectHandler OnReconnect;

		public delegate void OnDisconnectHandler();
		public event OnDisconnectHandler OnDisconnect;

		public delegate void OnIMInHandler(string strUser, string strMsg, bool bAuto);
		public event OnIMInHandler OnIMIn;

		public delegate void OnUpdateBuddyHandler(string strUser, bool bOnline);
		public event OnUpdateBuddyHandler OnUpdateBuddy;

		public delegate void OnSignedOnHandler();
		public event OnSignedOnHandler OnSignedOn;

		// private variables
		private bool m_bDCOnPurpose = false;
		private string m_uid = "";
		private string m_pw = "";

		private bool m_bSignedOn = false;

		private Socket m_socket;
		private Byte[] m_byBuff = new Byte[32767];
		private byte[] m_sessionId = new byte[4];
		
		public Yahoo(string strSN, string strPW)
		{
			m_uid = Normalize(strSN);
			m_pw = Normalize(strPW);
		}

		public bool Connect()
		{
			IPAddress ip = Dns.Resolve("scs.msg.yahoo.com").AddressList[0];
			int port = 5050;
		
			IPEndPoint remote = new IPEndPoint(ip,port);

			try 
			{
				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				m_socket.Blocking = false ;	
				m_socket.BeginConnect(remote , new AsyncCallback(OnConnect), m_socket);
			}
			catch (Exception er)
			{
				return false;
			}

			return true;
		}

		public void OnConnect(IAsyncResult ar)
		{
			Socket sock = (Socket)ar.AsyncState;

			try
			{
				if (sock.Connected)
				{
					m_bDCOnPurpose = false;
					SendOnConnectString();
					SetupRecieveCallback(sock);
				}
				else if (OnDisconnect != null)
					OnDisconnect();
			}
			catch(Exception ex)
			{
				
			}  
		}

		private void SetupRecieveCallback(Socket sock)
		{
			if (sock == null)
				return;

			try
			{
				AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
				sock.BeginReceive(m_byBuff, 0, m_byBuff.Length, SocketFlags.None,recieveData, sock);
			}
			catch( Exception ex )
			{
				//DispatchError(ex.Message);
			}
		}

		private void SendOnConnectString()
		{
			byte [] data = {0x59,0x4d,0x53,0x47,0x00,0x0b,0x00,0x00,0x00,0x00,
							   0x00,0x4c,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};
			m_socket.Send(data);
		}


		private void SendRequestSalt()
		{
			short slen = (short)(m_uid.Length+5);
			
			byte [] header = {0x59,0x4d,0x53,0x47,0x00,0x0b,0x00,0x00};//0x00,0x00,
			byte [] length = BitConverter.GetBytes(IPAddress.NetworkToHostOrder(slen));// seperator data and the ascii "1"
			byte [] stub =  {0x00,0x57,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};

			byte [] packethdr = new byte[stub.Length+length.Length+header.Length];
            
			Array.Copy(header,0,packethdr,0,header.Length);
			Array.Copy(length,0,packethdr,header.Length,length.Length);
			Array.Copy(stub,0,packethdr,header.Length+length.Length,stub.Length);

			byte [] seperator = {0xc0,0x80};
			byte [] msgbody = new byte[slen];

			Array.Copy(new byte [] {0x31},msgbody,1);
			Array.Copy(seperator,0,msgbody,1,2);
			Array.Copy(Encoding.Default.GetBytes(m_uid),0,msgbody,3,m_uid.Length);
			Array.Copy(seperator,0,msgbody,m_uid.Length+3,2);

			byte [] packet = new byte[msgbody.Length+packethdr.Length];
			Array.Copy(packethdr,0,packet,0,packethdr.Length);
			Array.Copy(msgbody,0,packet,packethdr.Length,msgbody.Length);

			m_socket.Send(packet);
				
		}

		[DllImport("yahoodll.dll")]
		static extern void GetPasswordEncrypt(string sn, string pw, string salt, StringBuilder res6, StringBuilder res96);

		private void SendEncryptedPW(string salt)
		{
			StringBuilder r6 = new StringBuilder(100);
			StringBuilder r96 = new StringBuilder(100);
			GetPasswordEncrypt(m_uid,m_pw,salt,r6,r96);

			byte[] msgBody = MakeMessageBody(new object[] {"6",r6.ToString()},
				new object[] {"96",r96.ToString()},
				new object[] {"0",m_uid},
				new object[] {"2","1"},
				new object[] {"1",m_uid},
				new object[] {"135","5, 6, 0, 1347"},
				new object[] {"148","300"});

			byte[] msgHeader = MakeMessageHeader((short)msgBody.Length,0x54,0x0b,new byte [] {0x5a,0x55,0xaa,0x55});

			byte [] message = new byte[msgHeader.Length+msgBody.Length];
			Array.Copy(msgHeader,0,message,0,msgHeader.Length);
			Array.Copy(msgBody,0,message,msgHeader.Length,msgBody.Length);
			m_socket.Send(message);
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
					byte [] byteTemp = new byte[2];
					byteTemp[1] = m_byBuff[nBytesRead+10];
					byteTemp[0] = m_byBuff[nBytesRead+11];
					short shCmd = BitConverter.ToInt16(byteTemp,0);

					byteTemp = new byte[2];
					byteTemp[1] = m_byBuff[nBytesRead+8];
					byteTemp[0] = m_byBuff[nBytesRead+9];
					short shBodyLen = BitConverter.ToInt16(byteTemp,0);

					byte [] body = new byte[shBodyLen];
					Array.Copy(m_byBuff,20,body,0,shBodyLen);
					ArrayList msgBody = ParseMessageBody(body);

					string strTemp = "";

					switch (shCmd)
					{
						case 0x01: // buddy logged on
						case 0x02: // buddy logged off
							for (int idx=0; idx<msgBody.Count; idx++)
							{
								string str = (string)msgBody[idx];
								if (str == "7" && msgBody[idx+1] != null)
								{
									strTemp = (string)msgBody[idx+1];
								}
							}

							strTemp = Normalize(strTemp);
							if (OnUpdateBuddy != null && strTemp !="")
								OnUpdateBuddy(strTemp,shCmd == 0x01);
							break;

						case 0x0f: // new friend?
							bool bFound97 = false;
							for (int idx=0; idx<msgBody.Count; idx++)
							{
								string str = (string)msgBody[idx];
								if (str == "97" && msgBody[idx+1] != null)
								{
									bFound97 = true;
									continue;
								}

								if (bFound97 && str == "3" && msgBody[idx+1] != null)
								{
									strTemp = (string)msgBody[idx+1];
									break;
								}
							}

							strTemp = Normalize(strTemp);
							if (OnUpdateBuddy != null && bFound97 && strTemp != "")
								OnUpdateBuddy(strTemp,true);
							break;

						case 0x06: // im in?
							string strUserName = "";
							string strMessage = "";
							for (int idx=0; idx<msgBody.Count; idx++)
							{
								string str = (string)msgBody[idx];
								if (str == "4" && msgBody[idx+1] != null)
									strUserName = (string)msgBody[idx+1];
								else if (str == "14" && msgBody[idx+1] != null)
									strMessage = (string)msgBody[idx+1];
								
								if (strUserName != "" && strMessage != "")
									break;
							}
							strUserName = Normalize(strUserName);
							strMessage = StripMarkup(strMessage);

							if (OnIMIn != null && strUserName.Length >=3 && strMessage.Length >= 1)
								OnIMIn(strUserName,strMessage,false);
							break;

						case 76: // response to our first message after connecting
							SendRequestSalt();
						break;
						
						case 0x57: // salt response
							m_sessionId[0] = m_byBuff[nBytesRead+16];
							m_sessionId[1] = m_byBuff[nBytesRead+17];
							m_sessionId[2] = m_byBuff[nBytesRead+18];
							m_sessionId[3] = m_byBuff[nBytesRead+19];

							SendEncryptedPW((string)msgBody[3]);					
							break;

						case 0x55: // looks like we've logged in, is this the buddy list?
							if (!m_bSignedOn && OnSignedOn != null)
								OnSignedOn();
							break;

						default:
							//System.Windows.Forms.MessageBox.Show("BYTES!!");
							break;

					}
					
					SetupRecieveCallback (sock);
				}
				else if (!m_bDCOnPurpose)
					HandleReconnect(); // looks like we disconnect, so reconnect
					
			}
			catch (Exception e)
			{
				// looks like the connection dropped
				if (!m_bDCOnPurpose)
					HandleReconnect();
			}
		}

		private void HandleReconnect()
		{
			m_socket.Shutdown(SocketShutdown.Both);
			m_socket.Close();
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

		private ArrayList ParseMessageBody(byte [] rawBody)
		{
			ArrayList retval = new ArrayList();
			string strTemp = "";

			for (int i = 0; i<rawBody.Length; i++)
			{
				if (rawBody[i] == 0x80)
					continue;
				  
				if (rawBody[i] == 0xc0 && ((i+1) <= rawBody.Length) && rawBody[i+1] == 0x80)
				{
					retval.Add(strTemp);
					strTemp = "";
					continue;
				}

				strTemp += (char)rawBody[i];
			}

			return retval;
		}


		byte [] MakeMessageBody(params object [] args)
		{
			ArrayList retval = new ArrayList();

			foreach (object obj in args)
			{
				Array arg = (Array)obj;
				
				foreach (object o in arg) 
				{
					if (o == null)
						goto abort;

					string strTemp = (string)o;

					foreach (char c in strTemp)
					{
						retval.Add((byte)c);
					}

				abort: {}
					retval.Add((byte)0xc0);
					retval.Add((byte)0x80);
				}
			}
			return (byte []) retval.ToArray(typeof(byte));
		}


		byte [] MakeMessageHeader(short iBodyLen, short ServiceID, short BValue, byte [] SomeFlag)
		{
			byte [] retval = new byte[20];

			retval[0] = 0x59; retval[1] = 0x4d; retval[2] = 0x53; retval[3] = 0x47; // YMSG
			Array.Copy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(BValue)),0,retval,4,2); // 00 - after login ,0b - first connected, 0c - to add a buddy
			Array.Copy(new byte[] {0x00,0x00},0,retval,6,2); // ?
			Array.Copy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(iBodyLen)),0,retval,8,2); // length of msgbody
			Array.Copy(BitConverter.GetBytes(IPAddress.NetworkToHostOrder(ServiceID)),0,retval,10,2); // service id?
			
			Array.Copy(SomeFlag,0,retval,12,4); // not too sure

			// sessionId is received when we receive the salt for the pw
			Array.Copy(m_sessionId,0,retval,16,4);

			return retval;
		}


		public void AddBuddy(string strBuddy,string strGroup)
		{
			if (!m_socket.Connected && !m_bDCOnPurpose)
				HandleReconnect(); 
			else
			{
				byte[] packetBody = MakeMessageBody(
					new object[] {"1",m_uid}, // how the bot is seen by others
					new object[] {"7",strBuddy},//name of buddy to add
					new object[] {"14",null}, // ?
					new object[] {"65",strGroup}, // group name of which to add the buddy
					new object[] {"97","1"}, // ?
					new object[] {"216",null});

				byte [] packetHdr = MakeMessageHeader((short)packetBody.Length,0x83,0x0c,new byte [] {0x00,0x00,0x00,0x00});

				SendPacket(packetHdr,packetBody);
			}
		}

		public void SendMessage(string strBuddy,string Message)
		{
			if (Message.Length > 1024)
				Message = Message.Substring(0,1024);

			byte[] packetBody = MakeMessageBody(
				new object[] {"1",m_uid}, // how the bot is seen by others
				new object[] {"5",strBuddy},//name of buddy receiving the message
				new object[] {"14",Message}); // the message

			byte [] packetHdr = MakeMessageHeader((short)packetBody.Length,0x06,0x0b,new byte [] {0x5a,0x55,0xaa,0x56});

			SendPacket(packetHdr,packetBody);
		}

		private void SendPacket(byte [] packetHdr,byte [] packetBody)
		{
			byte [] packet = new byte[packetHdr.Length+packetBody.Length];
			Array.Copy(packetHdr,0,packet,0,packetHdr.Length);
			Array.Copy(packetBody,0,packet,packetHdr.Length,packetBody.Length);
			m_socket.Send(packet);
		}

		public static string Normalize(string strScreenName)
		{
			string strName= strScreenName;
			strName = Regex.Replace(strName," ","");
			strName = strName.ToLower();
			return strName;
		}

		private static string StripMarkup(string strText)
		{
			string result = Regex.Replace(strText,@"<(.|\n)*?>",string.Empty);
			result = Regex.Replace(result,@"\[x*\d+m",string.Empty);
			result = Regex.Replace(result,@"\[\#[\d|a-f]+m",string.Empty);
			return result.Trim();
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

	}
}
