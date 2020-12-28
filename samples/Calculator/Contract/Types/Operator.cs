using XRoadLib.Attributes;

namespace Calculator.Contract.Types
{
    [XRoadSerializable]
    [XRoadTitle("en", "Operator type")]
    [XRoadNotes("en", "Defines operations to perform on given arguments")]
    public enum Operator
    {
        [XRoadTitle("en", "Add")]
        Add,

        [XRoadTitle("en", "Subtract")]
        Subtract,

        [XRoadTitle("en", "Multiply")]
        Multiply,

        [XRoadTitle("en", "Divide")]
        Divide
    }
}