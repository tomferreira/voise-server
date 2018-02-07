using CPqDAsr.ASR;
using CPqDASR.Communication;
using CPqDASR.Config;
using System.Collections.Generic;
using Voise.Recognizer.Provider.Common;
using Voise.Recognizer.Provider.Common.Job;
using Voise.Recognizer.Provider.Cpqd.Job;

namespace Voise.Recognizer.Provider.Cpqd
{
    internal class CpqdRecognizer : CommonRecognizer
    {
        internal const string ENGINE_IDENTIFIER = "ce";


        private readonly ClientConfig _config;
        private readonly LanguageModelList _modelList;


        public CpqdRecognizer(string userName, string password, string host)
        {
            _config = new ClientConfig
            {
                ServerUrl = host,
                Credentials = new Credentials
                {
                    UserName = userName,
                    Password = password
                },
                RecogConfig = new RecognitionConfig
                {
                    MaxSentences = 1,
                    RecognitionTimeoutEnabled = false,
                    ConfidenceThreshold = 0,
                    NoInputTimeoutEnabled = false,
                    ContinuousMode = true
                },
                ConnectOnRecognize = false,
                AutoClose = false
            };

            _modelList = new LanguageModelList();
            _modelList.AddFromUri("builtin:slm/general");
            
        }

        protected override ISyncJob CreateSyncJob(string audio_base64, string encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new SyncJob(_config, _modelList, audio_base64);
        }

        protected override IStreamingJob CreateStreamingJob(AudioStream streamIn, string encoding, int sampleRate, string languageCode, Dictionary<string, List<string>> contexts)
        {
            return new StreamingJob(_config, streamIn, _modelList);
        }
    }
}