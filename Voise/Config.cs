using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Voise
{
    public class Config
    {
        private static string FILENAME_FULLPATH = "./config.xml";

        // General
        private static int DEFAULT_PORT = 8102;

        // Recognizer
        private static List<string> DEFAULT_RECOGNIZERS_ENABLED = 
            new List<string>() { Recognizer.Microsoft.MicrosoftRecognizer.ENGINE_IDENTIFIER };

        // Classifier
        private static string DEFAULT_CLASSIFIERS_PATH = "./classifiers/";

        // Tunning
        private static bool DEFAULT_TUNNING_ENABLED = false;
        private static string DEFAULT_TUNNING_PATH = "./tunning/";

        private XmlElement _element;

        public Config()
        {
            XmlDocument doc = new XmlDocument();

            using (StreamReader reader = new StreamReader(FILENAME_FULLPATH, Encoding.GetEncoding("UTF-8")))
                doc.Load(reader);

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

        internal List<string> RecognizersEnabled
        {
            get
            {
                 string value = GetRecognizerAttribute("enabled");

                if (value == null)
                    return DEFAULT_RECOGNIZERS_ENABLED;

                string[] elem = value.Split(';', ',');
                return new List<string>(elem);
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
                object value = GetTunningAttribute("enabled");

                if (value == null)
                    return DEFAULT_TUNNING_ENABLED;

                return Convert.ToBoolean(value);
            }
        }

        internal string TunningPath
        {
            get
            {
                string value = GetTunningAttribute("path");

                if (value == null)
                    return DEFAULT_TUNNING_PATH;

                return value;
            }
        }

        internal string GetRecognizerAttribute(params string[] identifiers)
        {
            return GetAttribute("recognizers", identifiers);
        }

        internal string GetTunningAttribute(params string[] identifiers)
        {
            return GetAttribute("tunning", identifiers);
        }

        private string GetAttribute(string parentIdentifier, params string[] identifiers)
        {
            try
            {
                XmlNode node = _element.SelectSingleNode(parentIdentifier);

                foreach (string identifier in identifiers)
                    node = node.SelectSingleNode(identifier);

                return node.InnerText;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
