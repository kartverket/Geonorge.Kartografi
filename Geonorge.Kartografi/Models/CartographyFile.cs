using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models
{
    public class CartographyFile
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid SystemId { get; set; }
        /// <summary> Navn* på kartografiregel/stil. For eksempel «N50 stier og løyper».</summary>
        [Display(Name = "Tegneregelnavn")]
        [Required]
        public string Name { get; set; }

        /// <summary> Beskrivelse av både hva og hvordan filen uthever informasjon i datasettet.</summary>
        [Display(Name = "Beskrivelse")]
        public string Description { get; set; }

        /// <summary> Eier av datasettet, eventuelt eier av fil (ved foreslått digital kartografi), dropdown (fra organisasjonsregisteret)</summary>
        [Display(Name = "Organisasjon")]
        public string OwnerOrganization { get; set; }

        /// <summary> Navn på hvem som har sendt inn filen. Personnavn</summary>
        [Display(Name = "Innsender")]
        public string OwnerPerson { get; set; }

        /// <summary> Editor. Hentes fra pålogget bruker</summary>
        [Display(Name = "Redigert av")]
        public string LastEditedBy { get; set; }

        /// <summary>Opplasting av filen i form av sld/lyr/pdf</summary>
        [Display(Name = "Kartografi-fil")]
        public string FileName { get; set; }

        /// <summary>Dropdown (sld/lyr, leses fra kodeliste)</summary>
        [Display(Name = "Format")]
        public string Format { get; set; }

        /// <summary>Beskrivelse over hva filen skal brukes til. F.eks. style WMS eller GML.</summary>
        [Display(Name = "Kompatibel med")]
        public virtual ICollection<Compatibility> Compatibility { get; set; }

        /// <summary>Dropdown,beskrivelse over hva filen skal brukes til, f.eks. style WMS eller GML.</summary>
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

        /// <summary>Tallverdi for hvilken versjon av filen det er snakk om. Autogenereres.</summary>
        [Display(Name = "Versjon")]
        public int VersionId { get; set; }

        [ForeignKey("versioning")]
        public Guid versioningId { get; set; }
        public virtual Version versioning { get; set; }

        /// <summary>Dato for når filen/informasjonen i registeret sist ble endret.</summary>
        [Display(Name = "Dato endret")]
        public DateTime DateChanged { get; set; }

        /// <summary>(gjelder kun for «offisiell» digital kartografi) *Viser på samme måte som andre registre i geonorge om filen er godkjent eller ikke. Settes av administrator.</summary>
        [Display(Name = "Status")]
        public string Status { get; set; }

        /// <summary>Dato for når filen ble godkjent. Settes av administrator.</summary>
        [Display(Name = "Dato godkjent")]
        public DateTime DateAccepted { get; set; }

        /// <summary>Tekst dersom det er noe som er viktig å påpeke ved godkjenningen. F.eks. dersom noe blir godkjent, men ikke er optimalt. Settes av administrator</summary>
        [Display(Name = "Godkjent kommentar")]
        public string AcceptedComment { get; set; }

        /// <summary>Angi om kartografi er levert som offisielt tilbud eller som et alternativ til offisiell kartografi, radioknapp</summary>
        [Display(Name = "Offisiell")]
        public bool OfficialStatus { get; set; }

        /// <summary>Hvilke attributter er sentrale for bruk av kartografien, tekstfelt</summary>
        [Display(Name = "Viktig egenskap for kartografien")]
        public string Properties { get; set; }

        /// <summary>Hentes automatisk fra metadata. Inn i db eller koples dette i visningsapplikasjon? Tematisk hovedkategori i metadataene</summary>
        [Display(Name = "Tema")]
        public string Theme { get; set; }

    }
}