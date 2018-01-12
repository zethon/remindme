using System;
using System.Net;
using System.Text;
using System.IO;
using System.Xml;

namespace MsgParser
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public class MessageParser
	{
		private string m_strURL = @"localhost";
		private string m_strPort;
		private string m_strPath;

		private string m_strToUser;
		public string ToUser {  get { return m_strToUser; } }

		private string m_strServerTimeString;
		public string ServerTimeString {  get { return m_strServerTimeString; } }

		private string m_strUserTimeString;
		public string UserTimeString {  get { return m_strUserTimeString; } }

		private string m_strMsgText;
		public string MessageText {  get { return m_strMsgText; } }

		private string m_strParserErrorCode;
		public string ParserErrorCode { get { return m_strParserErrorCode; } }

		private string m_strRequestingUrl;
		public string RequestingUrl { get { return m_strRequestingUrl; } } 

		private string m_strRawResponse;
		public string RawResponse { get { return m_strRawResponse; } }

		private string m_strRawTime;
		public string RawTime { get { return m_strRawTime; } }

        private XmlDocument m_XmlDoc = new XmlDocument();
        public XmlDocument XmlDoc { get { return m_XmlDoc; } }

		public MessageParser(string strPort, string strPath)
		{
			m_strPort = strPort;
			m_strPath = strPath;
		}

		public bool PingServer()
		{
			string url = "http://"+m_strURL+":"+m_strPort+"/ping";
			try
			{
				Uri uri = new Uri(url,false);
				WebRequest req = WebRequest.Create(uri);

				WebResponse resp = req.GetResponse();
				Stream stream = resp.GetResponseStream();
				StreamReader sr = new StreamReader(stream);

				string strPingResponse  = sr.ReadToEnd();
				sr.Close();

				if (strPingResponse == "OK")
					return true;
			}
			catch (Exception e)
			{
				return false;
			}



			return false;
		}


		public bool SendParseRequest(string msgText,string TZ, bool DST,string strAction)
		{
			Reset();
			msgText = msgText.Replace("&","%26");
			string parameter="action="+strAction+"&msg=" + msgText + "&tz="+TZ+"&dls="+ (DST ? "1" : "0");
			string url = "http://"+m_strURL+":"+m_strPort+"/"+m_strPath+"?";
	
			try
			{
				Uri uri = new Uri(url + parameter,false);
				m_strRequestingUrl = uri.GetLeftPart(UriPartial.Authority)+ uri.PathAndQuery;
			
				WebRequest req = WebRequest.Create(uri);

				WebResponse resp = req.GetResponse();
				Stream stream = resp.GetResponseStream();
				StreamReader sr = new StreamReader(stream);

				m_strRawResponse  = sr.ReadToEnd();
				sr.Close();
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		public bool ProcessResponse()
		{
			if (m_strRawResponse.Length == 0)
				return false;
			
			try 
			{
				//XmlDocument doc = new XmlDocument();
                m_XmlDoc.LoadXml(m_strRawResponse);

                XmlElement root = m_XmlDoc.DocumentElement;
				XmlNode errorNode = root.SelectSingleNode("//parser_error");

				if (errorNode != null)
				{
					m_strParserErrorCode = errorNode.InnerText;
				}
				else
				{
					XmlNode msgNode = root.SelectSingleNode("//message");
					m_strMsgText = msgNode.SelectSingleNode("//text").InnerText;
					m_strUserTimeString = msgNode.SelectSingleNode("//user_time_string").InnerText;
					m_strServerTimeString = msgNode.SelectSingleNode("//server_time_string").InnerText;
					m_strRawTime = msgNode.SelectSingleNode("//raw_time").InnerText;
					m_strToUser = msgNode.SelectSingleNode("//to_user").InnerText;
					m_strParserErrorCode = null;
				}

			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		private void Reset()
		{
			m_strMsgText = "";
			m_strRawResponse = "";
			m_strParserErrorCode = "";
		}
	}
}
