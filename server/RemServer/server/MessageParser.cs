using System;
using System.Xml;
using System.Net;
using System.IO;
using System.Text;

namespace server
{
	class MessageParser
	{
		public string Message;
		public string UserTime;
		public string EpochTime;

		public string botTimeZone;
		public string ParserUrl = @"http://laptop/parser.cgi?";

		private int _lastError;
		public int LastError 
		{
			get { return _lastError; }
		}

		public MessageParser()
		{
			botTimeZone = @"0000";
		}

		public MessageParser(string strBotTZ)
		{
			botTimeZone = strBotTZ;
		}

		private XmlNode RawParse(string strData, string UserTZ, bool UserDLS, bool bMsg)
		{
			string strDLS = UserDLS ? "1" : "0";

			string Uri;
			
			
			if (bMsg) 
				Uri = ParserUrl + "action=parsemessage&data="+System.Web.HttpUtility.UrlEncode(strData)+"&tz="+System.Web.HttpUtility.UrlEncode(UserTZ)+"&udls="+System.Web.HttpUtility.UrlEncode(strDLS)+"&btz="+System.Web.HttpUtility.UrlEncode(botTimeZone);
			else
				Uri = ParserUrl + "action=parsetime&data="+System.Web.HttpUtility.UrlEncode(strData)+"&tz="+System.Web.HttpUtility.UrlEncode(UserTZ)+"&udls="+System.Web.HttpUtility.UrlEncode(strDLS)+"&btz="+System.Web.HttpUtility.UrlEncode(botTimeZone);

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Uri);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			Stream resStream = response.GetResponseStream();
			
			byte[] buf = new  byte[8192];
			int count = resStream.Read(buf, 0, buf.Length);
			resStream.Close(); 

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(Encoding.ASCII.GetString(buf, 0, buf.Length));
			return (XmlNode) doc.DocumentElement;
		}

		public bool ParseTime(string strTime,string UserTZ, bool UserDLS)
		{
			XmlNode root = RawParse(strTime,UserTZ,UserDLS,false);
			XmlNode errorNode = root.SelectSingleNode(@"error_code");

			if (errorNode != null)
			{
				_lastError = int.Parse(errorNode.InnerText);
				return false;
			}
			else
			{
				XmlNode epochtimeNode = root.SelectSingleNode(@"epochtime");
				
				if (epochtimeNode == null)
				{
					_lastError = 3;
					return false;
				}
				
				EpochTime = epochtimeNode.InnerText;
			}

			return true;
		}
		
		public bool ParseMessage(string strMessage,string UserTZ, bool UserDLS)
		{
			XmlNode root = RawParse(strMessage,UserTZ,UserDLS,true);

			XmlNode errorNode = root.SelectSingleNode(@"error_code");

			if (errorNode != null)
			{
				_lastError = int.Parse(errorNode.InnerText);
				return false;
			}
			else
			{
				XmlNode messageNode = root.SelectSingleNode(@"message");
				XmlNode usertimeNode = root.SelectSingleNode(@"usertime");
				XmlNode epochtimeNode = root.SelectSingleNode(@"epochtime");
				
				if (messageNode == null || usertimeNode == null || epochtimeNode == null)
				{
					_lastError = 3;
					return false;
				}
	
				Message = messageNode.InnerText;
				UserTime = usertimeNode.InnerText;
				EpochTime = epochtimeNode.InnerText;
			}
			return true;
		}
	}

}
