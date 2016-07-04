using System;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    class SendNotificationRequest : IXRoadXmlSerializable
    {
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string IdentificationNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Recidence { get; set; }
        public string Region { get; set; }
        public long ProbationType { get; set; }
        public long ProbationID { get; set; }
        public DateTime ProbationStartDate { get; set; }
        public DateTime ProbationEndDate { get; set; }
        public string CaseNumber { get; set; }
        public string VerdictNumber { get; set; }
        public DateTime VerdictDate { get; set; }
        public string VerdictIssuer { get; set; }

        public class QualificationsType : IXRoadXmlSerializable
        {
            public string item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public QualificationsType Qualifications { get; set; }

        public class ObligationsType : IXRoadXmlSerializable
        {
            public class itemType : IXRoadXmlSerializable
            {
                public string Type { get; set; }
                public string Details { get; set; }
                public DateTime DeadlineDate { get; set; }
                public string Status { get; set; }

                void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
                {
                }

                void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
                {
                }
            }

            public itemType item { get; set; }

            void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
            {
            }

            void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
            {
            }
        }

        public ObligationsType Obligations { get; set; }
        public string OfficialGivenName { get; set; }
        public string OfficialSurname { get; set; }
        public string OfficialInstitution { get; set; }
        public string OfficialEmail { get; set; }
        public string OfficialPhoneNumber { get; set; }
        public string OfficialWorkingHours { get; set; }
        public Stream Document { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}