using Optional;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using XRoadLib.Serialization;

namespace MyNamespace
{
    public class ToiminguOsaline : Osaline
    {
        public Option<DateTime?> AllkirjastamiseKP { get; set; }
        public Option<string> AllkirjastKeeldumisePohjus { get; set; }
        public Option<long?> AllkirjastKeeldumisePohjusKL { get; set; }
        public Option<string> IsikusamasuseTuvastamine { get; set; }
        public Option<string> KattesaajaCSV { get; set; }
        public Option<DateTime?> KattesaamiseKP { get; set; }
        public Option<Aadress> KinnipidamiseAadress { get; set; }
        public Option<string> KinnipidamiseKirjeldus { get; set; }
        public Option<string> KinnipidamisePohjendus { get; set; }
        public Option<IList<TegevuseKoht>> KokkusaamiseKohad { get; set; }
        public Option<bool?> LopetaMenetlusTaielikult { get; set; }
        public Option<long?> OsalemiseMeetodKL { get; set; }
        public Option<string> OsaliseAvaldusedVastuvaited { get; set; }
        public Option<string> OsaliseFaabula { get; set; }
        public Option<string> OsaliseSisu { get; set; }
        public Option<string> OsaliseYtlused { get; set; }
        public Option<string> OsalistKirjeldavadAndmed { get; set; }
        public Option<IList<OsaMakse>> OsaMaksed { get; set; }
        public Option<long?> SuhtlusKeelKL { get; set; }
        public Option<Isik> Taitja { get; set; }
        public Option<string> TeavitamiseKirjeldus { get; set; }
        public Option<long?> ToiminguosaliseLiikKL { get; set; }
        public Option<int?> ToiminguosaliseVanus { get; set; }
        public Option<Ametikoht> TookohtvOppeautus { get; set; }
        public Option<bool?> VottisOsa { get; set; }

        void IXRoadXmlSerializable.ReadXml(XmlReader reader, XRoadMessage message)
        {
        }

        void IXRoadXmlSerializable.WriteXml(XmlWriter writer, XRoadMessage message)
        {
        }
    }
}