using Autofac;
using Autofac.Core;
using log4net;
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
    public static class IocModule
    {
        public static void BuildContainer(ContainerBuilder containerBuilder, ILog logger)
        {            
            containerBuilder.RegisterInstance(logger).As<ILog>().ExternallyOwned();

            containerBuilder.RegisterType<FileConfig>().As<IConfig>().SingleInstance();

            containerBuilder.RegisterType<RecognizerManager>().As<IRecognizerManager>();
            containerBuilder.RegisterType<ClassifierManager>().As<IClassifierManager>();
            containerBuilder.RegisterType<SynthesizerManager>().As<ISynthesizerManager>();
            containerBuilder.RegisterType<TuningManager>().As<ITuningManager>();
        }

        public static void LogDeepestExceptions(Exception e, ILog logger)
        {
            if (e is DependencyResolutionException)
            {
                LogDeepestExceptions(e.InnerException, logger);
            }
            else if (e is AggregateException)
            {
                var ae = e as AggregateException;
                foreach (var ie in ae.InnerExceptions)
                    LogDeepestExceptions(ie, logger);
            }
            else
            {
                logger.Fatal($"{e.Message}\nStacktrace: {e.StackTrace}");
            }
        }
    }
}

