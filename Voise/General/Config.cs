using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Voise.General.Interface;

namespace Voise.General
{
    public class Config : IConfig
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

        // Tuning
        private const bool DEFAULT_TUNING_ENABLED = false;
        private const string DEFAULT_TUNING_PATH = "./tuning/";
        private const int DEFAULT_TUNING_RETENTON_DAY = 7;

        private XmlElement _element;

        public Config()
        {
            XmlDocument doc = new XmlDocument();

            using (StreamReader reader = new StreamReader(FILENAME_FULLPATH, Encoding.GetEncoding("UTF-8")))
                doc.Load(reader);

            _element = doc.DocumentElement;
        }

        public int Port
        {
            get
            {
                var node = _element.SelectSingleNode("port");

                if (node == null)
                    return DEFAULT_PORT;

                return Convert.ToInt32(node.InnerText, CultureInfo.InvariantCulture);
            }
        }

        public List<string> RecognizersEnabled
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

        public List<string> SynthesizersEnabled
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

        public string ClassifiersPath
        {
            get
            {
                var node = _element.SelectSingleNode("classifiers_path");

                if (node == null)
                    return DEFAULT_CLASSIFIERS_PATH;

                return node.InnerText;
            }
        }

        public bool TuningEnabled
        {
            get
            {
                object value = GetTuningAttribute("enabled");

                if (value == null)
                    return DEFAULT_TUNING_ENABLED;

                return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
            }
        }

        public string TuningPath
        {
            get
            {
                string value = GetTuningAttribute("path");

                if (value == null)
                    return DEFAULT_TUNING_PATH;

                return value;
            }
        }

        public int TuningRetentionDays
        {
            get
            {
                string value = GetTuningAttribute("retention_days");

                if (value == null)
                    return DEFAULT_TUNING_RETENTON_DAY;

                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
        }

        public string GetRecognizerAttribute(params string[] identifiers)
        {
            return GetAttribute("recognizers", identifiers);
        }

        public string GetSynthesizerAttribute(params string[] identifiers)
        {
            return GetAttribute("synthesizers", identifiers);
        }

        public string GetTuningAttribute(params string[] identifiers)
        {
            return GetAttribute("tuning", identifiers);
        }

        private string GetAttribute(string parentIdentifier, params string[] identifiers)
        {
            var node = _element.SelectSingleNode(parentIdentifier);

            foreach (string identifier in identifiers)
                node = node?.SelectSingleNode(identifier);

            return node?.InnerText;
        }
    }
}
