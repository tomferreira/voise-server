using weka.classifiers.bayes;
using weka.classifiers.meta;
using weka.core;

namespace Voise.Classification.Classifier
{
    internal class NaiveBayesTextClassifier : Base
    {
        internal NaiveBayesTextClassifier() 
            : base()
        {
            _wekaClassifier = new NaiveBayesMultinomialText();

            (_wekaClassifier as NaiveBayesMultinomialText).setLowercaseTokens(true);
        }

        internal override void Train(Instances data)
        {
            _trainingData = data;

            CVParameterSelection ps = new CVParameterSelection();
            ps.setClassifier(new NaiveBayesMultinomialText());

            ps.setNumFolds(5);  // using 5-fold CV

            ps.addCVParameter("C 0.1 0.5 1 5");
            ps.addCVParameter("N 0 1 2");
            //ps.addCVParameter("P 0");
            ps.addCVParameter("M 1.0 3.0 5.0");
            ps.addCVParameter("norm 0.5 1.0 2.0");
            ps.addCVParameter("lnorm 1.0 2.0 3.0");

            ps.buildClassifier(_trainingData);

            string summary = ps.toSummaryString();

            string[] options = ps.getBestClassifierOptions();

            (_wekaClassifier as NaiveBayesMultinomialText).setOptions(options);

            base.Train(_trainingData);
        }
    }
}
