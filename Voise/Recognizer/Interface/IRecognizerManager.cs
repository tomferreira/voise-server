using Voise.Recognizer.Provider.Common;

namespace Voise.Recognizer.Interface
{
    public interface IRecognizerManager
    {
        ICommonRecognizer GetRecognizer(string engineID);
    }
}
