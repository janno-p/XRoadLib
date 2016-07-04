using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace XRoadLib.Tools.CodeGen
{
    public class XElementParser
    {
        private readonly IEnumerator<XElement> enumerator;
        private bool isDone = true;

        public XElementParser(XElement element)
        {
            this.enumerator = element.Elements().GetEnumerator();
        }

        public bool IgnoreElement(string name)
        {
            return ParseElement(name, e => { });
        }

        public bool ParseElement(string name, Action<XElement> action = null)
        {
            if (isDone && !LoadNext())
                return false;

            if (enumerator.Current != null && enumerator.Current.Name == XName.Get(name, NamespaceConstants.XSD))
            {
                if (action == null)
                    throw new NotImplementedException(enumerator.Current.Name.ToString());

                action(enumerator.Current);
                isDone = true;

                return true;
            }

            return false;
        }

        public void ThrowIfNotDone()
        {
            if (!isDone)
                throw new NotImplementedException(enumerator.Current.Name.ToString());
        }

        private bool LoadNext()
        {
            if (!enumerator.MoveNext())
                return false;

            isDone = false;

            return true;
        }
    }
}