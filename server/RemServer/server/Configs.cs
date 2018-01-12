using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;

namespace server
{
    abstract public class ConfigFactory
    {
        //abstract public Config CreateServerConfig();
        abstract public DataManager CreateDataManager();

        // TODO: I'm against this, but what can I do?
        abstract public string GetBotName();
    }

    public class XMLConfigFactory : ConfigFactory
    {
        private XmlDocument _rootDoc = new XmlDocument();

        private void BuildFactory(string strData, bool bLoadFromFile)
        {
            if (bLoadFromFile)
                _rootDoc.Load(strData);
            else
                _rootDoc.LoadXml(strData);
        }

        public XMLConfigFactory(string strRawXml)
        {
            BuildFactory(strRawXml, false);
        }

        public XMLConfigFactory(string strRawData, bool bLoadFromFile)
        {
            BuildFactory(strRawData, bLoadFromFile);
        }

        //public override bool CreateServerConfig()
        //{
        //    //throw new NotImplementedException();
        //    return new WebServiceDBConfig();
        //}

        /// <summary>
        /// Creates a DataManager Object
        /// </summary>
        public override DataManager CreateDataManager()
        {
            DataManager retObj = null;

            XmlNode UrlNode = _rootDoc.SelectSingleNode(@"/serverconfig/dbmanager/type");

            if (UrlNode == null)
                throw new Exception(@"XmlConfigFactory.CreateDataManager: UrlNode is null");

            switch (UrlNode.InnerText.ToLower())
            {
                case @"server.servicedbmanager":
                    if (_rootDoc.SelectSingleNode(@"/serverconfig/dbmanager/URL") == null)
                        throw new Exception(@"XmlConfigFactory.CreateDataManager: No URL defined in config");

                    if (_rootDoc.SelectSingleNode(@"/serverconfig/dbmanager/botname") == null)
                        throw new Exception(@"XmlConfigFactory.CreateDataManager: No botname defined in config");

                    string strURL = _rootDoc.SelectSingleNode(@"/serverconfig/dbmanager/URL").InnerText;
                    string strBot = _rootDoc.SelectSingleNode(@"/serverconfig/dbmanager/botname").InnerText;
                    retObj = new ServiceDBManager(strURL, strBot);
                break;

                default:
                    throw new Exception(@"Unknown object type (" + UrlNode.InnerText + ")");
                break;
            }


            return retObj;
        }

        public override string GetBotName()
        {
            if (_rootDoc.SelectSingleNode(@"/serverconfig/dbmanager/botname") == null)
                throw new Exception(@"XmlConfigFactory.GetBotName: No botname defined in config");

            return _rootDoc.SelectSingleNode(@"/serverconfig/dbmanager/botname").InnerText;
        }
    }

    public abstract class Config
    {

    }

    public class WebServiceDBConfig : Config
    {
        private string _serviceUrl = string.Empty;
        public string ServiceUrl
        {
            get { return _serviceUrl; }
            set { _serviceUrl = value; }
        }

        private string _botName = string.Empty;


        static public WebServiceDBConfig CreateConfig(string strData, bool bLoadFromFile)
        {
            XmlDocument doc = new XmlDocument();
            if (bLoadFromFile)
                doc.Load(strData);
            else
                doc.LoadXml(strData);

            XmlNode UrlNode = doc.SelectSingleNode(@"/serverconfig/dbmanager/URL");

            return new WebServiceDBConfig();
        }
    }



    class ServerConfig : Config
    {

        private WebServiceDBConfig _webConfig = null;
        public WebServiceDBConfig WebConfig
        {
            get { return _webConfig; } 
        }


        static public ServerConfig CreateConfig(string strRawXml)
        {
            return CreateConfig(strRawXml, false);
        }

        static public ServerConfig CreateConfig(string strRawXml, bool bLoadFromFile)
        {

            return new ServerConfig();
        }
    }
}
