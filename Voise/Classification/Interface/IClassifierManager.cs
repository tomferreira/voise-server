using System.Collections.Generic;
using System.Threading.Tasks;

namespace Voise.Classification.Interface
{
    public interface IClassifierManager
    {
        Dictionary<string, List<string>> GetTrainingList(string modelName);
        Task<Classifier.Base.Result> ClassifyAsync(string modelName, string message);
    }
}
