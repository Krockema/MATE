using System;
using NLog;

namespace Zpp
{
    class Program
    {
        bool IS_DB_INITIALIZED = false;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Hello World");
                for (int i = 0; i < 10 ; i++)
                {
                    Console.WriteLine(i);
                }

                ;

                testMethod();
        }

        private static void testMethod()
        {
            Console.WriteLine("my testMethod()!");
        }
    }
}