using System;
using GodotXUnitApi;
using Xunit;
using Xunit.Abstractions;

namespace GodotCSUnitTest.Tests
{
    public class SomePrintStatementsTest
    {
        private readonly ITestOutputHelper output;

        public SomePrintStatementsTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void SomeStuffPrinted()
        {
            output.WriteLine("hello hello helllloooooo!!!");
        }

        [Fact]
        public async void SomeReallyLongTest()
        {
            for (int i = 0; i < 100; i++)
            {
                await GDU.OnProcessAwaiter;
                output.WriteLine($"stuffs {i}");
            }
        }

        [Fact]
        public void NaughtyConsolePrint()
        {
            Console.WriteLine("idk what i'm doing... this will not show up and will be lost");
            output.WriteLine(output.GetType().ToString());
        }
    }
}
