using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models.Api
{
    public class Cartography
    {

        public Guid Uuid { get; set; }
        /// <summary> Navn på kartografiregel/stil.</summary>
        [Display(Name = "Tegneregelnavn")]
        public string Name { get; set; }

        /// <summary> Beskrivelse av både hva og hvordan filen uthever informasjon i datasettet.</summary>
        [Display(Name = "Beskrivelse")]
        public string Description { get; set; }

        /// <summary> Organisasjon som har sendt inn filen.</summary>
        [Display(Name = "Organisasjon")]
        public string Owner { get; set; }

        /// <summary> Eier av datasettet</summary>
        [Display(Name = "Datasetteier")]
        public string OwnerDataset { get; set; }

        /// <summary>Filnavn sld/qml/lyr</summary>
        [Display(Name = "Kartografi-fil")]
        public string FileName { get; set; }

        /// <summary>Format:sld/qml/lyr</summary>
        [Display(Name = "Format")]
        public string Format { get; set; }

        /// <summary>Beskrivelse over hva filen skal brukes til. F.eks. style WMS eller GML.</summary>
        [Display(Name = "Kompatibel med")]
        public string Compatibility { get; set; }

        /// <summary>Beskrivelse over hva filen skal brukes til, f.eks. style WMS eller GML.</summary>
        [Display(Name = "Bruksområde")]
        public string Use { get; set; }

        /// <summary>Kobling til datasett</summary>
        [Display(Name = "Datasett")]
        public string DatasetUuid { get; set; }

        /// <summary></summary>
        [Display(Name = "Datasettnavn")]
        public string DatasetName { get; set; }

        /// <summary>Kobling til tjeneste</summary>
        [Display(Name = "Tjeneste")]
        public string ServiceUuid { get; set; }

        /// <summary></summary>
        [Display(Name = "Tjeneste-navn")]
        public string ServiceName { get; set; }

        /// <summary>Et lite bilde som viser hvilke symboler som benyttes med beskrivende tekst.</summary>
        [Display(Name = "Miniatyrbilde")]
        public string PreviewImage { get; set; }

        /// <summary>Tallverdi for hvilken versjon av filen det er snakk om.</summary>
        [Display(Name = "Versjon")]
        public int VersionId { get; set; }

        /// <summary>Dato for når filen/informasjonen i registeret sist ble endret.</summary>
        [Display(Name = "Dato endret")]
        [DisplayFormat(NullDisplayText = "", DataFormatString = "{0:d}")]
        public DateTime DateChanged { get; set; } = DateTime.Now;

        /// <summary>(gjelder kun for «offisiell» digital kartografi) Viser på samme måte som andre registre i geonorge om filen er godkjent eller ikke.</summary>
        [Display(Name = "Status")]
        public string Status { get; set; }

        /// <summary>Dato for når filen ble godkjent.</summary>
        [Display(Name = "Dato godkjent")]
        [DisplayFormat(NullDisplayText = "", DataFormatString = "{0:d}")]
        public DateTime? DateAccepted { get; set; }

        /// <summary>Angi om kartografi er levert som offisielt tilbud eller som et alternativ til offisiell kartografi</summary>
        [Display(Name = "Offisiell")]
        public bool OfficialStatus { get; set; }

        /// <summary>Hvilke attributter er sentrale for bruk av kartografien</summary>
        [Display(Name = "Viktig egenskap for kartografien")]
        public string Properties { get; set; }

        /// <summary>Tematisk hovedkategori i metadataene</summary>
        [Display(Name = "Tema")]
        public string Theme { get; set; }

        /// <summary>Url til fil</summary>
        [Display(Name = "FilUrl")]
        public string FileUrl { get; set; }

        /// <summary>Url til thumbnail</summary>
        public string PreviewImageUrl { get; set; }

        /// <summary>Url til detaljer</summary>
        [Display(Name = "DetailsUrl")]
        public string DetailsUrl { get; set; }
    }
}