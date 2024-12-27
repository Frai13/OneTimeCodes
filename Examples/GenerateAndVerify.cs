using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OneTimeCodes;

namespace Examples
{
    /// <summary>
    /// In this example, TokenGenerator is used to generate codes of 3 chars using seed "0".
    /// By default, it generates 3 codes starting at index 0. Once "codes" file is created, it waits for user
    /// to write a code to be checked (until user presses CTRL+C).
    /// </summary>
    internal class GenerateAndVerify
    {
        static void Main(string[] args)
        {
            uint start = args.Length >= 1 ? UInt32.Parse(args[0]) : 0;
            uint number = args.Length >= 2 ? UInt32.Parse(args[1]) : 3;

            string password = "";
            byte[] salt = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };
            int iterations = 10000;
            TokenGenerator generator = new TokenGenerator(password, salt, iterations, 3);
            
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
