using System.Reflection;
using XRoadLib;
using XRoadLib.Schema;

namespace Calculator
{
    public class CalculatorServiceManager : ServiceManager
    {
        public CalculatorServiceManager()
            : base("4.0", new DefaultSchemaExporter("http://calculator.x-road.eu/", typeof(CalculatorServiceManager).GetTypeInfo().Assembly))
        { }
    }
}