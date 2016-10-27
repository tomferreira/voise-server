using weka.classifiers.trees;
using weka.core;
using weka.filters;
using weka.filters.unsupervised.attribute;

namespace Voise.Classification
{
    internal class J48TextClassifier : Base
    {
        internal J48TextClassifier(string modelName)
            : base(modelName)
        {
            _classifier = new J48();
            _filter = new StringToWordVector();

            //_filter.setTokenizer(new NGramTokenizer());
            //_filter.setStemmer(new PTStemmer());

            _filter.setLowerCaseTokens(true);
        }

        internal override void Train(Instances data)
        {
            _trainingData = data;

            Instances filteredData;

            if (_filter != null)
            {
                // Initialize filter and tell it about the input format.
                _filter.setInputFormat(_trainingData);

                // Generate word counts from the training data.
                filteredData = Filter.useFilter(_trainingData, _filter);
            }
            else
            {
                filteredData = _trainingData;
            }

            base.Train(filteredData);
        }
    }
}
