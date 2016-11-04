using System;
using System.Xml;

namespace Voise
{
    public class Config
    {
        private static string FILENAME_FULLPATH = "./config.xml";

        // General
        private static int DEFAULT_PORT = 8100;

        // Classifier
        private static string DEFAULT_CLASSIFIERS_PATH = "./classifiers/";

        // Tunning
        private static bool DEFAULT_TUNNING_ENABLED = false;
        private static string DEFAULT_TUNNING_PATH = "./tunning/";

        private XmlElement _element;

        public Config()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(FILENAME_FULLPATH);

            _element = doc.DocumentElement;
        }

        internal int Port
        {
            get
            {
                try
                {
                    return Convert.ToInt32(_element.SelectSingleNode("port").InnerText);
                }
                catch (Exception)
                {
                    return DEFAULT_PORT;
                }
            }
        }

        internal string ClassifiersPath
        {
            get
            {
                try
                {
                    return _element.SelectSingleNode("classifiers_path").InnerText;
                }
                catch(Exception)
                {
                    return DEFAULT_CLASSIFIERS_PATH;
                }                
            }
        }

        internal bool TunningEnabled
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(_element.SelectSingleNode("tunning").SelectSingleNode("enabled").InnerText);
                }
                catch (Exception)
                {
                    return DEFAULT_TUNNING_ENABLED;
                }
            }
        }

        internal string TunningPath
        {
            get
            {
                try
                {
                    return _element.SelectSingleNode("tunning").SelectSingleNode("tunning_path").InnerText;
                }
                catch(Exception)
                {
                    return DEFAULT_TUNNING_PATH;
                }
            }
        }
    }
}
