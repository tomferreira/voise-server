using log4net;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using weka.classifiers;
using weka.core;
using weka.filters.unsupervised.attribute;

namespace Voise.Classification
{
    internal abstract class Base
    {
        protected const double MINUMIN_APRC = 0.9;

        internal class Result
        {
            internal string ClassName { get; private set; }
            internal float Probability { get; private set; }

            internal Result(string className, float probability)
            {
                ClassName = className;
                Probability = probability;
            }
        };

        protected Classifier _classifier;
        protected StringToWordVector _filter;

        protected Instances _trainingData;
        protected ILog _log;

        internal string ModelName { get; private set; }

        internal Base(string modelName)
        {
            _log = LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            ModelName = modelName;
        }

        internal virtual void Train(Instances data)
        {
            _classifier.buildClassifier(data);

            // TODO: Resolver se irei usar isso para verificar a "qualidade" do modelo.
            Evaluation evaluator = new Evaluation(data);

            evaluator.setDiscardPredictions(false);
            evaluator.crossValidateModel(_classifier, data, 11, new java.util.Random(42));

            string summary = evaluator.toSummaryString();
            double waupr = evaluator.weightedAreaUnderPRC();

            double aupr0 = evaluator.areaUnderPRC(0);
            double aupr1 = evaluator.areaUnderPRC(1);

            string matrix = evaluator.toMatrixString();

            if (waupr < MINUMIN_APRC)
                _log.Warn($"{ModelName}: Area under PR Curve below down {MINUMIN_APRC}");
        }

        internal virtual Result Classify(string message)
        {
            if (message == null)
                throw new ArgumentException("Message is null.");

            message = message.ToLower();

            Result result = InnerClassify(message);

            if (result != null)
              return result;

            double[] probabilities = Distribution(message);

            double probability = probabilities.Max();

            int predictedClass = probabilities.ToList().IndexOf(probability);
            string className = _trainingData.classAttribute().value(predictedClass);

            return new Result(className, (float)probability);
        }

        internal double[] Distribution(string message)
        {
            if (message == null)
                throw new ArgumentException("Message is null.");

            Instances testset = _trainingData.stringFreeStructure();

            // Make message into test instance.
            Instance instance = MakeInstance(message, testset);

            Instance filteredInstance;

            if (_filter != null)
            {
                // Filter instance.
                _filter.input(instance);
                filteredInstance = _filter.output();
            }
            else
            {
                filteredInstance = instance;
            }

            return _classifier.distributionForInstance(filteredInstance);
        }

        protected Instance MakeInstance(string text, Instances data)
        {
            // Create instance of length two.
            Instance instance = new DenseInstance(2);

            // Set value for message attribute
            weka.core.Attribute messageAtt = data.attribute("text");
            instance.setValue(messageAtt, messageAtt.addStringValue(text));

            // Give instance access to attribute information from the dataset.
            instance.setDataset(data);

            return instance;
        }

        protected Result InnerClassify(string message)
        {
            message = message.ToLower();

            // Verify if the message match with some training data.
            int numInstances = _trainingData.numInstances();
            for (int i = 0; i < numInstances; i++)
            {
                Instance inst = _trainingData.instance(i);

                if (inst.stringValue(0).ToLower() == message)
                {
                    int ci = Convert.ToInt32(inst.classValue());
                    return new Result(_trainingData.classAttribute().value(ci), 1);
                }
            }

            return null;
        }
    }
}
