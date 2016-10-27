using log4net;
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
        private Dictionary<string, Base> _classifiers;

        internal ClassifierManager(string classifiersPath)
        {
            _log = LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            _classifiers = new Dictionary<string, Base>();

            LoadClassifiers(classifiersPath);
        }

        internal async Task<Base.Result> ClassifyAsync(string modelName, string message)
        {
            if (modelName == null || modelName.Trim() == "")
                throw new BadModelException("Model name is empty.");

            Base classifier = null;

            lock (_classifiers)
            {
                if (!_classifiers.ContainsKey(modelName))
                    throw new BadModelException($"Model '{modelName}' not found.");

                classifier = _classifiers[modelName];
            }

            return classifier.Classify(message);
        }

        private void LoadClassifiers(string classifiersPath)
        {
            if (!Directory.Exists(classifiersPath))
                throw new ClassifiersPathNotFound($"Classifiers path not found: {classifiersPath}");

            _log.Info($"Loading classifiers from {classifiersPath}");

            IEnumerable<string> directories = Directory.EnumerateDirectories(classifiersPath);

            foreach(var directory in directories)
            {
                IEnumerable<string> files = Directory.EnumerateFiles(directory, "*.arff");

                foreach(var file in files)
                    AddClassifier(file);
            }
        }

        private void AddClassifier(string path)
        {
            Instances trainingData;

            using (java.io.BufferedReader reader = new java.io.BufferedReader(new java.io.FileReader(path)))
            {
                trainingData = new Instances(reader);

                // The last attribute will be the class.
                trainingData.setClassIndex(trainingData.numAttributes() - 1);
            }

            lock (_classifiers)
            {
                if (_classifiers.ContainsKey(trainingData.relationName()))
                    throw new System.Exception($"Model is duplicated: '{trainingData.relationName()}'");
            }

            Base classifier = new LogisticTextClassifier(trainingData.relationName());
            classifier.Train(trainingData);

            Base.Result s0 = classifier.Classify("pare de atrapalhar");
            Base.Result s1 = classifier.Classify("tá ocupado");
            Base.Result s2 = classifier.Classify("está ocupado");
            Base.Result s3 = classifier.Classify("sim");
            Base.Result s4 = classifier.Classify("não");
            Base.Result s41 = classifier.Classify("não porque");
            Base.Result s5 = classifier.Classify("não entendi muito bem");
            Base.Result s6 = classifier.Classify("sim sim");
            Base.Result s7 = classifier.Classify("não não");
            Base.Result s8 = classifier.Classify("só momento");
            Base.Result s9 = classifier.Classify("está no céu");
            Base.Result s10 = classifier.Classify("para de ligar para mim");
            Base.Result s11 = classifier.Classify("para de ligar");
            Base.Result s12 = classifier.Classify("eu não comemoro o meu aniversário");
            Base.Result s13 = classifier.Classify("sim mas não comemoro meu aniversário");
            Base.Result s14 = classifier.Classify("está de cama");
            Base.Result s15 = classifier.Classify("está fora");

            lock (_classifiers)
                _classifiers.Add(classifier.ModelName, classifier);

            _log.Info($"Classifier '{classifier.ModelName}' loaded.");
        }
    }
}
