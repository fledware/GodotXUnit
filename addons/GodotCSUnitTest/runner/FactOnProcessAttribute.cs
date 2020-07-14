using Xunit;
using Xunit.Sdk;

namespace GodotCSUnitTest
{
    [XunitTestCaseDiscoverer("GodotCSUnitTest.FactOnProcessDiscoverer", "GodotCSUnitTest")]
    public class FactOnProcessAttribute : FactAttribute
    {
        
    }
}