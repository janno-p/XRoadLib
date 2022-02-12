namespace XRoadLib.Schema;

public enum XRoadFaultPresentation
{
    /// <summary>
    /// X-Road fault is not included in service description.
    /// </summary>
    Implicit,

    /// <summary>
    /// X-Road fault is described as sequence of optional elements which include
    /// operation response value, faultCode and faultString.
    /// </summary>
    Explicit,

    /// <summary>
    /// X-Road fault is described as choice where options are: 1) operation response value;
    /// 2) sequence containing faultCode and faultString elements
    /// </summary>
    Choice
}