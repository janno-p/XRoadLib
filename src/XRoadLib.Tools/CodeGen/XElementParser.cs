using System;
using System.Collections.Generic;
using System.Xml.Linq;
using XRoadLib.Tools.CodeGen.Extensions;

namespace XRoadLib.Tools.CodeGen
{
    public class XElementParser
    {
        private readonly XElement element;
        private readonly IEnumerator<XElement> enumerator;

        public bool IsDone { get; private set; } = true;

        public XElementParser(XElement element)
        {
            this.element = element;
            this.enumerator = element.Elements().GetEnumerator();
        }

        public void AttributeNotImplemented(string attributeName)
        {
            if (element.HasAttribute(attributeName))
                throw new NotImplementedException(attributeName);
        }

        public bool IgnoreElement(string name)
        {
            return ParseElement(name, e => { });
        }

        public bool ParseElement(string name, Action<XElement> action = null)
        {
            if (IsDone && !LoadNext())
                return false;

            if (enumerator.Current != null && enumerator.Current.Name == XName.Get(name, NamespaceConstants.XSD))
            {
                if (action == null)
                    throw new NotImplementedException(enumerator.Current.Name.ToString());

                action(enumerator.Current);
                IsDone = true;

                return true;
            }

            return false;
        }

        public void ThrowIfNotDone()
        {
            if (!IsDone)
                throw new NotImplementedException(enumerator.Current.Name.ToString());
        }

        private bool LoadNext()
        {
            if (!enumerator.MoveNext())
                return false;

            IsDone = false;

            return true;
        }
    }
}