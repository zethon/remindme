using System;
using System.Xml;
using System.Text.RegularExpressions;

namespace server
{
	/// <summary>
	/// Summary description for HelpManager.
	/// </summary>
	public class HelpManager
	{
		private string m_strXMLFile = "";
		
		public HelpManager(string strXMLFile)
		{
			m_strXMLFile = strXMLFile;
		}

		public string GetResponse(string strCmd)
		{
            string strRetVal = string.Empty;;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(m_strXMLFile);

                XmlNode xRoot = doc.DocumentElement;
                XmlNodeList list = xRoot.SelectNodes(@"//command");

                Regex reg;
                Match m;

                foreach (XmlNode xNode in list)
                {
                    string strRegEx = xNode.Attributes[@"regex"].Value.ToString();

                    reg = new Regex(strRegEx, RegexOptions.IgnoreCase);
                    m = reg.Match(strCmd);

                    if (m.Success)
                    {
                        XmlNode xResponse = xNode.SelectSingleNode(@"response");
                        strRetVal = xResponse.InnerText;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                strRetVal = @"Could not get response: " + e.Message;
                return strRetVal;
            }

            return strRetVal;
		}
	}
}
