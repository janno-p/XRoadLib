﻿using System.Diagnostics.CodeAnalysis;
using XRoadLib.Serialization;

namespace XRoadLib.Tests.Contract
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ParamType2 : XRoadSerializable
    {
        public long Value1 { get; set; }
    }
}