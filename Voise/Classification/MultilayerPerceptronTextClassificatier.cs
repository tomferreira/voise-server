using weka.classifiers.functions;
using weka.core;
using weka.filters;
using weka.filters.unsupervised.attribute;

namespace Voise.Classification
{
    internal class MultilayerPerceptronTextClassificatier : Base
    {
        internal MultilayerPerceptronTextClassificatier(string modelName)
            : base(modelName)
        {
            _classifier = new MultilayerPerceptron();
            _filter = new StringToWordVector();
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
