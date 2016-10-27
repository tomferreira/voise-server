﻿using weka.classifiers.functions;
using weka.core;
using weka.filters;
using weka.filters.unsupervised.attribute;

namespace Voise.Classification.Exception
{
    internal class LogisticTextClassifier : Base
    {
        internal LogisticTextClassifier(string modelName)
            : base(modelName)
        {
            _classifier = new Logistic();
            _filter = new StringToWordVector();

            _filter.setLowerCaseTokens(true);
        }

        internal override void Train(Instances data)
        {
            _trainingData = data;

            // Initialize filter and tell it about the input format.
            _filter.setInputFormat(_trainingData);

            // Generate word counts from the training data.
            Instances filteredData = Filter.useFilter(_trainingData, _filter);

            base.Train(filteredData);
        }
    }
}
