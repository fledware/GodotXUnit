using System;
using GodotXUnitApi;
using Xunit;
using Xunit.Abstractions;

namespace GodotXUnitTest.Tests
{
    public class SomePrintStatementsTest
    {
        private readonly ITestOutputHelper output;

        public SomePrintStatementsTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [GodotFact]
        public void SomeStuffPrinted()
        {
            output.WriteLine("hello hello helllloooooo!!!");
        }

        [GodotFact]
        public async void SomeReallyLongTest()
        {
            for (int i = 0; i < 100; i++)
            {
                await GDU.OnProcessAwaiter;
                output.WriteLine($"stuffs {i}");
            }
        }

        [GodotFact]
        public void NaughtyConsolePrint()
        {
            Console.WriteLine("this will be in the test output, but you will get a warning from IDEs");
            GDU.Print("this will be in test output too");
            output.WriteLine(output.GetType().ToString());
        }
    }
}
