using System; 
using TerminatorCoreCSharp;

namespace TerminatorCoreCS
{ 
    public class TerminatorCoreTest
    {
        
        static void Main()
        {
            start:
            Console.WriteLine("How to use method? 1 - Process ID | 2 - Process Name(example.exe)");
            string response = Console.ReadLine();
            if (response == "1")
            {
                Console.WriteLine("Process ID:");
                int responseid = Convert.ToInt32(Console.ReadLine());
                var core = new TerminatorCore(responseid);
                core.SuspendProcess();
                Console.WriteLine("You are Suspend process ID: " + responseid);
                Console.WriteLine("Click the button to continue!");
                Console.ReadKey();
                core.ResumeProcess();
                Console.WriteLine("You are Resume process ID: " + responseid);
                Console.WriteLine("Click the button to continue!");
                Console.ReadKey();
                core.FreeTerminator();
                Console.WriteLine("The program is completed!");
                Console.ReadKey();
            }
            else if(response == "2")
            {
                Console.WriteLine("Process Name(case sensitive):");
                response = Console.ReadLine();
                var core = new TerminatorCore(response);
                core.SuspendProcess();
                Console.WriteLine("You are Suspend process: "+ response);
                Console.WriteLine("Click the button to continue!");
                Console.ReadKey();
                core.ResumeProcess();
                Console.WriteLine("You are Resume process: " + response);
                Console.WriteLine("Click the button to continue!");
                Console.ReadKey();
                core.FreeTerminator();
                Console.WriteLine("The program is completed!");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("You need to choose 1 - Process ID or 2 - Process Name(example.exe)!");
                Console.WriteLine("_________________________________");
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("");
                goto start;
            }
            
        } 
    }
}
