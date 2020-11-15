using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using XRoadLib.Schema;

namespace XRoadLib.Serialization
{
    public abstract class XRoadFilterMap<T> : IXRoadFilterMap
    {
        private readonly string _groupName;
        private readonly ISet<string> _enabledProperties = new SortedSet<string>();

        string IXRoadFilterMap.GroupName => _groupName;
        ISet<string> IXRoadFilterMap.EnabledProperties => _enabledProperties;

        protected XRoadFilterMap(string groupName)
        {
            _groupName = groupName;
        }

        protected void Enable<TValue>(Expression<Func<T, TValue>> expression)
        {
            if (!(expression.Body is MemberExpression memberExpression))
                throw new SchemaDefinitionException($"MemberExpression expected, but was {expression.Body.GetType().Name} ({GetType().Name}).");

            if (memberExpression.Expression != expression.Parameters[0])
                throw new SchemaDefinitionException($"Only parameter members should be used in mapping definition ({GetType().Name}).");

            _enabledProperties.Add(memberExpression.Member.Name);
        }
    }
}