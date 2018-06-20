using System;

namespace BlackHole.Core
{
    public interface ILogger : IInterface
    {
        void Log(string format, params object[] args);
    }
    public class ConsoleLogger : ILogger
    {
        public void Log(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }

    public static class DefaultLogger
    {
        public static void Log(string format, params object[] args)
        {
            if (null == Logger)
                Logger = ImplementContainer.Construct<ILogger>();

            if (null != Logger)
                Logger.Log(format, args);
        }

        static ILogger Logger;
    }
}
