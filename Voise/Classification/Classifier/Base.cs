﻿using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using weka.classifiers;
using weka.core;
using weka.filters.unsupervised.attribute;

namespace Voise.Classification.Classifier
{
    internal abstract class Base
    {
        protected const double MINUMIN_APRC = 0.9;

        protected const int K_FOLDS = 11;

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

        protected weka.classifiers.Classifier _wekaClassifier;
        protected StringToWordVector _filter;

        protected Instances _trainingData;

        // List of all class => text used in train data.
        protected Dictionary<string, List<string>> _trainingList;

        protected ILog _log;

        private string _modelName;
        internal string ModelName {
            get
            {
                return _modelName;
            }

            set
            {
                if (_modelName != null)
                    throw new System.Exception("Model name alread defined.");

                _modelName = value;
            }
        }

        internal Base()
        {
            _log = LogManager.GetLogger(typeof(Base));

            _modelName = null;
            _trainingList = new Dictionary<string, List<string>>();
        }

        internal Dictionary<string, List<string>> GetTrainingList()
        {
            return _trainingList;
        }

        internal virtual void Train(Instances data)
        {
            _wekaClassifier.buildClassifier(data);

            SetTrainText();

            Evaluation evaluator = new Evaluation(data);

            evaluator.setDiscardPredictions(false);
            evaluator.crossValidateModel(_wekaClassifier, data, K_FOLDS, new java.util.Random(42));

            double waupr = evaluator.weightedAreaUnderPRC();

            if (waupr < MINUMIN_APRC)
            {
                _log.Warn($"{ModelName}: Area under PR Curve is {waupr}, that is below down {MINUMIN_APRC}");
            }
            else
            {
                _log.Info($"{ModelName}: Area under PR Curve is {waupr}");
            }
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

            return _wekaClassifier.distributionForInstance(filteredInstance);
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

        private void SetTrainText()
        {
            int numInstances = _trainingData.numInstances();
            for (int i = 0; i < numInstances; i++)
            {
                Instance inst = _trainingData.instance(i);

                int ci = Convert.ToInt32(inst.classValue());
                string className = _trainingData.classAttribute().value(ci);

                if (!_trainingList.ContainsKey(className))
                    _trainingList.Add(className, new List<string>());

                _trainingList[className].Add(inst.stringValue(0).ToLower());
            }
        }
    }
}