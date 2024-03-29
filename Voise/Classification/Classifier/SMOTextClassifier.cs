﻿using log4net;
using weka.classifiers.functions;
using weka.classifiers.meta;
using weka.core;
using weka.filters;
using weka.filters.unsupervised.attribute;

namespace Voise.Classification.Classifier
{
    internal class SMOTextClassifier : Base
    {
        internal SMOTextClassifier(ILog logger)
            : base(logger)
        {
            _wekaClassifier = new SMO();
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

            CVParameterSelection ps = new CVParameterSelection();
            ps.setClassifier(new SMO());

            ps.setNumFolds(5);  // using 5-fold CV

            ps.addCVParameter("C 0.1 0.5 1 5");
            ps.addCVParameter("N 0 1 2");

            ps.buildClassifier(filteredData);

            string[] bestOptions = ps.getBestClassifierOptions();

            string[] options = new string[4] { "C", Utils.getOption('C', bestOptions), "N", Utils.getOption('N', bestOptions) };
            (_wekaClassifier as SMO).setOptions(options);

            base.Train(filteredData);
        }
    }
}
