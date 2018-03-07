using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Voise.Classification.Exception;
using weka.core;

namespace Voise.Classification
{
    internal class ClassifierManager
    {
        private ILog _log;
        private Dictionary<string, Classifier.Base> _classifiers;

        internal ClassifierManager(Config config)
        {
            _log = LogManager.GetLogger(typeof(ClassifierManager));

            _classifiers = new Dictionary<string, Classifier.Base>();

            LoadClassifiers(config.ClassifiersPath);
        }

        internal Dictionary<string, List<string>> GetTrainingList(string modelName)
        {
            Classifier.Base classifier = null;

            lock (_classifiers)
            {
                if (modelName == null || !_classifiers.ContainsKey(modelName))
                    return null;

                classifier = _classifiers[modelName];
            }

            return classifier.GetTrainingList();
        }

        internal async Task<Classifier.Base.Result> ClassifyAsync(string modelName, string message)
        {
            if (modelName == null || modelName.Trim() == "")
                throw new BadModelException("Model name is empty.");

            Classifier.Base classifier = null;

            lock (_classifiers)
            {
                if (!_classifiers.ContainsKey(modelName))
                    throw new BadModelException($"Model '{modelName}' not found.");

                classifier = _classifiers[modelName];
            }

            return await Task.Run(() => classifier.Classify(message) );
        }

        private void LoadClassifiers(string classifiersPath)
        {
            if (!Directory.Exists(classifiersPath))
                throw new ClassifiersPathNotFound($"Classifiers path not found: {classifiersPath}");

            _log.Info($"Loading classifiers from {classifiersPath}");

            IEnumerable<string> directories = Directory.EnumerateDirectories(classifiersPath);

            ConcurrentQueue<System.Exception> exceptions = new ConcurrentQueue<System.Exception>();

            foreach (var directory in directories)
            {
                IEnumerable<string> files = Directory.EnumerateFiles(directory, "*.arff");

                Parallel.ForEach(files, (file) =>
                {
                    try
                    {
                        AddClassifier(file, new Classifier.LogisticTextClassifier());
                    }
                    catch(System.Exception e)
                    {
                        exceptions.Enqueue(e);
                    }
                });
            }

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

            classifier.ModelName = trainingData.relationName();

            lock (_classifiers)
            {
                if (_classifiers.ContainsKey(classifier.ModelName))
                    throw new System.Exception($"Model is duplicated: '{classifier.ModelName}'");
            }

            classifier.Train(trainingData);

            lock (_classifiers)
                _classifiers.Add(classifier.ModelName, classifier);

            _log.Info($"Classifier '{classifier.ModelName}' loaded.");
        }
    }
}
