using System;
using System.Threading;

namespace ArosimServer
{
    class Program
    {

        public static bool isRunnig = false;


        static void Main(string[] args)
        {
            Console.Title = "RobotControl Server";
            isRunnig = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(20, 585);
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Runnig at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime nextLoop = DateTime.Now;

            while(isRunnig)
            {
                while(nextLoop < DateTime.Now)
                {
                    SimulationLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if(nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
