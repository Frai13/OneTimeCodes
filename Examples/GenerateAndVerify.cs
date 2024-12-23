using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneTimeCodes;

namespace Examples
{
    internal class GenerateAndVerify
    {
        static void Main(string[] args)
        {
            uint start = args.Length >= 1 ? UInt32.Parse(args[0]) : 0;
            uint number = args.Length >= 2 ? UInt32.Parse(args[1]) : 3;

            TokenGenerator generator = new TokenGenerator(0, 3, 5);
            
            List<string> code_list = generator.GetCodes(start, number);
            generator.GenerateCodes(start, number);
            Console.WriteLine("Codes list:");
            foreach (var c in code_list) Console.WriteLine($"\t[{c}]");

            Console.WriteLine("Introduce codes to verify.");
            Console.WriteLine("Press CTRL+C to exit.");
            while (true)
            {
                string code = Console.ReadLine();
                if (generator.CheckCode(code))
                {
                    Console.WriteLine("Code verified successfully!");
                }
                else
                {
                    Console.WriteLine("Not allowed.");
                }
            }
        }
    }
}
