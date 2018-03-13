using System;
using System.Collections.Generic;
using System.Linq;

namespace BatchProcess
{
    internal class Program
    {
        private static ReadmeExample readme = new ReadmeExample();
        private static MessageHookExample messageHook = new MessageHookExample();

        private static void Main(string[] args)
        {
            RunSamplesByIndex();
        }

        private static void RunSamplesByIndex()
        {
            var actions = Activities();
            var key = "";
            while (key.ToLower() != "n")
            {
                Console.WriteLine(string.Join("", Enumerable.Repeat("#", 80)));
                Console.WriteLine("Samples Index :");
                Console.WriteLine(string.Join(Environment.NewLine, Index()));
                Console.WriteLine(Environment.NewLine);

                Console.WriteLine("Press 'n' for exit.");
                Console.Write("Select an index [1 to {0}] : ", actions.Count);
                key = Console.ReadLine();
                if (key.ToLower() == "n")
                    break;

                if (int.TryParse(key, out int index))
                {
                    if (index > actions.Count)
                    {
                        Console.WriteLine("Select an index [1 to {0}] : ", actions.Count);
                        continue;
                    }
                    Console.WriteLine(Environment.NewLine);
                    actions[index]();
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine(string.Join("", Enumerable.Repeat("#", 80)));
                }
                else
                    continue;
            }
        }

        private static string[] Index()
        {
            return new string[4] { "1 - WorkflowJob - Simple integration job.",
            "2 - RetryJob - Job will be retried max of 3 times, when any failures.",
            "3 - Message hook, with simple Job uses Jobcontext's message push.",
            "4 - Messahe hook, out of the box hooks avaialble in Workflow job execution."};
        }

        private static Dictionary<int, Action> Activities()
        {
            return new Dictionary<int, Action>() {
                { 1, () => readme.WorkflowJob()  },
                { 2, () => readme.RetryJob()},
                { 3, () => messageHook.With_Standalone_Job()},
                { 4, () => messageHook.With_Workflow_Job() }
            };
        }
    }
}