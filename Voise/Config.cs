using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Voise
{
    public class Config
    {
        private const string FILENAME_FULLPATH = "./config.xml";

        // General
        private const int DEFAULT_PORT = 8102;

        // Recognizer
        private static readonly List<string> DEFAULT_RECOGNIZERS_ENABLED =
            new List<string>() { Recognizer.Provider.Microsoft.MicrosoftRecognizer.ENGINE_IDENTIFIER };

        // Synthesizer
        private static readonly List<string> DEFAULT_SYNTHESIZERS_ENABLED =
            new List<string>() { Synthesizer.Provider.Microsoft.MicrosoftSynthesizer.ENGINE_IDENTIFIER };

        // Classifier
        private const string DEFAULT_CLASSIFIERS_PATH = "./classifiers/";

        // Tunning
        private const bool DEFAULT_TUNNING_ENABLED = false;
        private const string DEFAULT_TUNNING_PATH = "./tunning/";

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

        internal List<string> SynthesizersEnabled
        {
            get
            {
                string value = GetSynthesizerAttribute("enabled");

                if (value == null)
                    return DEFAULT_SYNTHESIZERS_ENABLED;

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
                catch (Exception)
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

        internal string GetSynthesizerAttribute(params string[] identifiers)
        {
            return GetAttribute("synthesizers", identifiers);
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
