using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace XRoadLib.Serialization
{
    public abstract class XRoadFilterMap<T> : IXRoadFilterMap
    {
        private readonly string groupName;
        private readonly ISet<string> enabledProperties = new SortedSet<string>();

        string IXRoadFilterMap.GroupName { get { return groupName; } }
        ISet<string> IXRoadFilterMap.EnabledProperties { get { return enabledProperties; } }

        protected XRoadFilterMap(string groupName)
        {
            this.groupName = groupName;
        }

        protected void Enable<TValue>(Expression<Func<T, TValue>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException(string.Format("MemberExpression expected, but was {0} ({1}).", expression.Body.GetType().Name, GetType().Name));

            if (memberExpression.Expression != expression.Parameters[0])
                throw new ArgumentException(string.Format("Only parameter members should be used in mapping definition ({0}).", GetType().Name));

            enabledProperties.Add(memberExpression.Member.Name);
        }
    }
}