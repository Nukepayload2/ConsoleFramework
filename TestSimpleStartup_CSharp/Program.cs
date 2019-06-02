using System;

namespace TestSimpleStartup_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            Nukepayload2.ConsoleFramework.Application.Run(
            (Action<int>)(port =>
            {
                Console.WriteLine("Started at " + port);
            }));
        }
    }
}
