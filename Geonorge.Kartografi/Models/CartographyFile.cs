using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class CartographyFile
    {
        public int Id { get; set; }
        /// <summary> Navn* på kartografiregel/stil. For eksempel «N50 stier og løyper».</summary>
        public string Name { get; set; }

        /// <summary> Beskrivelse av både hva og hvordan filen uthever informasjon i datasettet.</summary>
        public string Description { get; set; }

        /// <summary> Eier av datasettet, eventuelt eier av fil (ved foreslått digital kartografi), dropdown (fra organisasjonsregisteret)</summary>
        public string OwnerOrganization { get; set; }

        /// <summary> Navn på hvem som har sendt inn filen. Personnavn</summary>
        public string OwnerPerson { get; set; }

        /// <summary> Editor. Hentes fra pålogget bruker</summary>
        public string LastEditedBy { get; set; }

        /// <summary>Opplasting av filen i form av sld/lyr/pdf</summary>
        public string FileName { get; set; }

        /// <summary>Dropdown (sld/lyr, leses fra kodeliste)</summary>
        public string Format { get; set; }

        /// <summary>Dropdown,beskrivelse over hva filen skal brukes til, f.eks. style WMS eller GML.</summary>
        public string Use { get; set; }

        /// <summary>Kobling til datasett</summary>
        public string DatasetUuid { get; set; }

        /// <summary></summary>
        public string DatasetName { get; set; }

        /// <summary>Kobling til tjeneste</summary>
        public string ServiceUuid { get; set; }

        /// <summary></summary>
        public string ServiceName { get; set; }

        /// <summary>Et lite bilde som viser hvilke symboler som benyttes med beskrivende tekst.</summary>
        public string PreviewImage { get; set; }

        /// <summary>Dato for når filen/informasjonen i registeret sist ble endret.</summary>
        public DateTime DateChanged { get; set; }

        /// <summary>(gjelder kun for «offisiell» digital kartografi) *Viser på samme måte som andre registre i geonorge om filen er godkjent eller ikke. Settes av administrator.</summary>
        public string Status { get; set; } 

        /// <summary>Dato for når filen ble godkjent. Settes av administrator.</summary>
        public DateTime DateAccepted { get; set; }

        /// <summary>Tekst dersom det er noe som er viktig å påpeke ved godkjenningen. F.eks. dersom noe blir godkjent, men ikke er optimalt. Settes av administrator</summary>
        public string AcceptedComment { get; set; } 

        /// <summary>Angi om kartografi er levert som offisielt tilbud eller som et alternativ til offisiell kartografi, radioknapp</summary>
        public string OfficialStatus { get; set; } 

        /// <summary>Hvilke attributter er sentrale for bruk av kartografien, tekstfelt</summary>
        public string Properties { get; set; } 

        /// <summary>Hentes automatisk fra metadata. Inn i db eller koples dette i visningsapplikasjon? Tematisk hovedkategori i metadataene</summary>
        public string Theme { get; set; }

    }
}