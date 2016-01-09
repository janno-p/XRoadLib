using System;

namespace XRoadLib.Tests.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    internal class OrderAttribute : Attribute
    {
        public int Value { get; }

        public OrderAttribute(int value)
        {
            Value = value;
        }
    }
}
