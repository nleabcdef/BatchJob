using BatchProcess.AutoJob;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BatchProcess.AutoJob.Extensions;

namespace BatchProcess
{
    internal class SimpleBatchProcessing
    {
        public void Run()
        {

            IAutomatedJob printUtcTime = InlineJob.GetDefault(() =>
            {
                Console.WriteLine(DateTime.UtcNow.ToString());
            });

            printUtcTime.Doable();
            printUtcTime.Repeat(5).Doable();

            Console.ReadLine();
        }
    }
    
}
