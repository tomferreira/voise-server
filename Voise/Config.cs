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
            new List<string>() { Recognizer.Provider.Microsoft.MicrosoftRecognizer.ENGINE_IDENTIFIER };

        // Classifier
        private static string DEFAULT_CLASSIFIERS_PATH = "./classifiers/";

        // Tuning
        private static bool DEFAULT_TUNING_ENABLED = false;
        private static string DEFAULT_TUNING_PATH = "./tuning/";

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

        internal bool TuningEnabled
        {
            get
            {
                object value = GetTuningAttribute("enabled");

                if (value == null)
                    return DEFAULT_TUNING_ENABLED;

                return Convert.ToBoolean(value);
            }
        }

        internal string TuningPath
        {
            get
            {
                string value = GetTuningAttribute("path");

                if (value == null)
                    return DEFAULT_TUNING_PATH;

                return value;
            }
        }

        internal string GetRecognizerAttribute(params string[] identifiers)
        {
            return GetAttribute("recognizers", identifiers);
        }

        internal string GetTuningAttribute(params string[] identifiers)
        {
            return GetAttribute("tuning", identifiers);
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
