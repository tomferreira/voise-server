using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Voise.Classification.Exception;
using Voise.Classification.Interface;
using Voise.General.Interface;
using weka.core;

namespace Voise.Classification
{
    public class ClassifierManager : IClassifierManager
    {
        private ILog _logger;
        private Dictionary<string, Classifier.Base> _classifiers;

        // Delimiter used in the relation attribute
        // to separate model name and language code.
        private const char LANGUAGE_CODE_DELIMITER = '@';

        public ClassifierManager(IConfig config, ILog logger)
        {
            _logger = logger;

            _classifiers = new Dictionary<string, Classifier.Base>();

            LoadClassifiers(config.ClassifiersPath);
        }

        public Dictionary<string, List<string>> GetTrainingList(string modelName, string languageCode)
        {
            Classifier.Base classifier = null;

            string relationName = GenerateRelationName(modelName, languageCode);

            lock (_classifiers)
            {
                if (modelName == null || languageCode == null || !_classifiers.ContainsKey(relationName))
                    return null;

                classifier = _classifiers[relationName];
            }

            return classifier.GetTrainingList();
        }

        public Task<Classifier.Base.Result> ClassifyAsync(string modelName, string languageCode, string message)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                throw new BadModelException("Model name is empty.");

            if (string.IsNullOrWhiteSpace(languageCode))
                throw new BadModelException("Language code is empty.");

            string relationName = GenerateRelationName(modelName, languageCode);

            Classifier.Base classifier = null;

            lock (_classifiers)
            {
                if (!_classifiers.ContainsKey(relationName))
                    throw new BadModelException($"Model '{relationName}' not found.");

                classifier = _classifiers[relationName];
            }

            return Task.Run(() => classifier.Classify(message));
        }

        private void LoadClassifiers(string classifiersPath)
        {
            if (!Directory.Exists(classifiersPath))
                throw new ClassifiersPathNotFoundException($"Classifiers path not found: {classifiersPath}");

            _logger.Info($"Loading classifiers from {classifiersPath}");

            ConcurrentQueue<System.Exception> exceptions = new ConcurrentQueue<System.Exception>();

            IEnumerable<string> files = Directory.EnumerateFiles(
                classifiersPath, "*.arff", SearchOption.AllDirectories);

            Parallel.ForEach(files, (file) =>
            {
                try
                {
                    AddClassifier(file, new Classifier.LogisticTextClassifier(_logger));
                }
                catch (System.Exception e)
                {
                    exceptions.Enqueue(e);
                }
            });

            if (!exceptions.IsEmpty)
                throw new AggregateException(exceptions);
        }

        private void AddClassifier(string path, Classifier.Base classifier)
        {
            Instances trainingData;

            using (java.io.BufferedReader reader = new java.io.BufferedReader(new java.io.FileReader(path)))
            {
                trainingData = new Instances(reader);

                // The last attribute will be the class.
                trainingData.setClassIndex(trainingData.numAttributes() - 1);
            }

            string relationName = trainingData.relationName();

            Regex rgx = new Regex($"^([^{LANGUAGE_CODE_DELIMITER}]+){LANGUAGE_CODE_DELIMITER}(.+)$");
            Match match = rgx.Match(relationName);

            if (!match.Success)
                throw new System.Exception($"Relation attribute is invalid: '{relationName}'");

            classifier.ModelName = match.Groups[1].Value;
            classifier.LanguageCode = match.Groups[2].Value;

            lock (_classifiers)
            {
                if (_classifiers.ContainsKey(relationName))
                    throw new System.Exception($"Model is duplicated: '{relationName}'");
            }

            classifier.Train(trainingData);

            lock (_classifiers)
                _classifiers.Add(relationName, classifier);

            _logger.Info($"Classifier '{relationName}' loaded.");
        }

        private static string GenerateRelationName(string modelName, string languageCode)
        {
            return $"{modelName}{LANGUAGE_CODE_DELIMITER}{languageCode}";
        }
    }
}
