using System;
using System.Xml;
using System.Collections;
using System.Threading;

namespace server
{

	/// <summary>
	/// Summary description for ConnectionManager.
	/// </summary>
	public class ConnectionManager
	{
		public string xmlName = "";
		private Hashtable m_Connections;
        public Hashtable Connections
        {
            get { return m_Connections; }
        }

		public ConnectionManager()
		{
			m_Connections = new Hashtable();
		}

		public void AddConnection(Connection conn)
		{
			m_Connections[conn.m_type] = conn;
		}

		public Connection GetConnection(ConnectionType type)
		{
			return (Connection)m_Connections[type];
		}

		public void ConnectAll()
		{
			IDictionaryEnumerator id = m_Connections.GetEnumerator();
			
			while (id.MoveNext())
			{
				Connection conn = (Connection)id.Value;
				conn.Connect();
				Thread.Sleep(1000);
			}
		}

		public bool IsBuddyOnline(ConnectionType type, string strBuddy)
		{
			Connection conn = GetConnection(type);

			return conn == null ? false : conn.IsBuddyOnline(strBuddy);
		}

		public void AddBuddies(ConnectionType type, string [] Buddies)
		{
			Connection conn = GetConnection(type);
			
			if (conn != null)
				conn.AddBuddies(Buddies);
		}

		public IDictionaryEnumerator GetConnectionEnumerator()
		{
			return m_Connections.GetEnumerator();
		}
	}

	public class ConMgrFactory
	{
		//Output Log;

		public ConMgrFactory()
		{
		}

		private Connection MakeConnection(ConnectionType type,string strLogin, 
												string strPassword, string strServer)
		{
			Connection retCon;

			switch (type)
			{
				case ConnectionType.AIM:
					//retCon = new ConnOscar(strLogin,strPassword);
					retCon = new ConnTOC2(strLogin,strPassword);
				break;

				case ConnectionType.MSN:
					retCon = new ConnMSN(strLogin,strPassword);
					break;
			
				case ConnectionType.ICQ:
					retCon = new ConnICQ(strLogin,strPassword);
					break;

				case ConnectionType.EMAIL:
					retCon = new ConnEmail(strLogin,strPassword,strServer);
					break;

				case ConnectionType.YAHOO:
					retCon = new ConnYahoo(strLogin,strPassword);
					break;

				case ConnectionType.SMS:
					retCon = new ConnSMS(strLogin);
					break;

                case ConnectionType.JABBER:
                    retCon = new ConnJabber(strLogin, strPassword);
                break;

				default:
					retCon = null;
					break;
			}

			return retCon;
		}

		public ConnectionManager MakeManager(XmlNode botNode)
		{
			if (botNode == null)
				return null;

			ConnectionManager retBot = new ConnectionManager();

			XmlNode nameNode = botNode.SelectSingleNode("@name");
			//retBot.m_name = nameNode.Value;

			XmlNodeList conns = botNode.SelectNodes("//connection");
			foreach (XmlNode connNode in conns)
			{
				XmlAttributeCollection attrs = connNode.Attributes;

				XmlNode strType = attrs.GetNamedItem("type");	
				XmlNode nodeLogin = connNode.SelectSingleNode("login");
				XmlNode nodePw = connNode.SelectSingleNode("password");
				XmlNode nodeServer = connNode.SelectSingleNode("server");
				
				if (strType == null)
				{
					Log.Instance.WriteError("No connection type defined");
					continue;
				}
				else if (nodeLogin == null)
				{
					Log.Instance.WriteError("No login defined in XML file");
					continue;
				}
				else if (nodePw == null)
				{
					Log.Instance.WriteError("No password defined in XMl file");
					continue;
				}
				else if (nodeServer == null && strType.InnerText.ToLower() == "email")
				{
					Log.Instance.WriteError("No server defined for email connection");
					continue;
				}

				ConnectionType conType = new ConnectionType();
				conType = (ConnectionType)Enum.Parse(typeof(ConnectionType),strType.InnerText,true);

				string emailSrv = nodeServer == null ? "" : nodeServer.InnerText;
				Connection newCon = MakeConnection(conType,nodeLogin.InnerText,nodePw.InnerText,emailSrv);

				if (newCon != null)
					retBot.AddConnection(newCon);
			}

			return retBot;
		}
	
	}
}
