using Autofac;
using log4net;
using log4net.Config;
using System;
using Voise.Classification;
using Voise.Classification.Interface;
using Voise.General;
using Voise.General.Interface;
using Voise.Recognizer;
using Voise.Recognizer.Interface;
using Voise.Synthesizer;
using Voise.Synthesizer.Interface;
using Voise.Tuning;
using Voise.Tuning.Interface;

namespace Voise
{
    public class IocModule
    {
        public static void BuildContainer(ContainerBuilder containerBuilder, IConfig config, ILog logger)
        {            
            containerBuilder.RegisterInstance(logger).As<ILog>().ExternallyOwned();
            containerBuilder.RegisterInstance(config).As<IConfig>().ExternallyOwned();

            containerBuilder.RegisterType<RecognizerManager>().As<IRecognizerManager>();
            containerBuilder.RegisterType<ClassifierManager>().As<IClassifierManager>();
            containerBuilder.RegisterType<SynthesizerManager>().As<ISynthesizerManager>();
            containerBuilder.RegisterType<TuningManager>().As<ITuningManager>();
        }
    }
}

