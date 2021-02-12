using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Voise.General
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

        internal int Port
        {
            get
            {
                var node = _element.SelectSingleNode("port");

                if (node == null)
                    return DEFAULT_PORT;

                return Convert.ToInt32(node.InnerText, CultureInfo.InvariantCulture);
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
                var node = _element.SelectSingleNode("classifiers_path");

                if (node == null)
                    return DEFAULT_CLASSIFIERS_PATH;

                return node.InnerText;
            }
        }

        internal bool TuningEnabled
        {
            get
            {
                object value = GetTuningAttribute("enabled");

                if (value == null)
                    return DEFAULT_TUNING_ENABLED;

                return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
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

        internal int TuningRetentionDays
        {
            get
            {
                string value = GetTuningAttribute("retention_days");

                if (value == null)
                    return DEFAULT_TUNING_RETENTON_DAY;

                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
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

        internal string GetTuningAttribute(params string[] identifiers)
        {
            return GetAttribute("tuning", identifiers);
        }

        private string GetAttribute(string parentIdentifier, params string[] identifiers)
        {
            var node = _element.SelectSingleNode(parentIdentifier);

            foreach (string identifier in identifiers)
                node = node.SelectSingleNode(identifier);

            return node?.InnerText;
        }
    }
}
