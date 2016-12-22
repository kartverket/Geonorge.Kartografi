using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class Symbol
    {
        public int Id { get; set; }

        /// <summary> Beskrivende navn på symbol.</summary>
        public string Name { get; set; }

        /// <summary> Beskrivelse av hva bildet viser eller brukes til</summary>
        public string Description { get; set; }

        /// <summary> Ekstern ID, eks surfisk3</summary>
        public string EksternalSymbolID { get; set; }

        /// <summary> Eier av symbolfil,Dropdown (fra organisasjonsregisteret, preutfylt med organisasjon til pålogget bruker. Kan endres.) </summary>
        public string OwnerOrganization { get; set; }

        /// <summary> Navn på hvem som har sendt inn symbolfilen. Personnavn</summary>
        public string OwnerPerson { get; set; }

        /// <summary> Editor. Hentes fra pålogget bruker</summary>
        public string LastEditedBy { get; set; }

        /// <summary> Punkt, skravur, dropdown</summary>
        public string Type { get; set; }

        /// <summary>Dato for når filen/informasjonen i registeret sist ble endret.</summary>
        public DateTime DateChanged { get; set; }

        /// <summary>Viser på samme måte som andre registre i geonorge om filen er godkjent eller ikke. Settes av administrator. (Ikke synlig før den er godkjent)</summary>
        public string Status { get; set; }

        /// <summary>Dato for når filen ble godkjent. Settes av administrator.</summary>
        public DateTime DateAccepted { get; set; }

        /// <summary>Angi om kartografi er levert som offisielt tilbud eller som et alternativ til offisiell kartografi, radioknapp</summary>
        public string OfficialStatus { get; set; }

        /// <summary>Hovedtema (hentes fra liste) + verdi «generell».</summary>
        public string Theme { get; set; }

        /// <summary>Navn på symbolsamling symbolet hentes fra. Tekstfelt</summary>
        public string Source { get; set; } 

        /// <summary>URL til symbolsamling symbolet hentes fra.</summary>
        public string SourceUrl { get; set; }

        /// <summary>ulike grafiske forekomster av symbolet (ulike formater eller farger, .. )</summary>
        public List<SymbolFile> SymbolFiles { get; set; }
    }
}