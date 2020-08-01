using Xunit;
using Xunit.Sdk;

namespace GodotXUnitApi
{
    /// <summary>
    /// will have the test run in the process notification
    /// </summary>
    /*
        [FactOnProcess]
        public void ILikeToRunOnProcess()
        {
            GD.Print("i'm in the process event!!");
        }
     */
    [XunitTestCaseDiscoverer("GodotXUnitApi.Internal.FactOnProcessDiscoverer", "GodotXUnitApi")]
    public class FactOnProcessAttribute : FactAttribute
    {
        
    }
}