using Geonorge.Kartografi.Helpers;
using Geonorge.Kartografi.Models.Translations;
using Geonorge.Kartografi.Resources;
using Geonorge.Kartografi.Services;
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
        public CartographyFile()
        {
            this.Translations = new TranslationCollection<CartographyFileTranslation>();
        }

        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid SystemId { get; set; }
        /// <summary> Navn* på kartografiregel/stil. For eksempel «N50 stier og løyper».</summary>
        [Display(Name = "Name", ResourceType = typeof(UI))]
        [Required]
        public string Name { get; set; }

        /// <summary> Beskrivelse av både hva og hvordan filen uthever informasjon i datasettet.</summary>
        [Display(Name = "Description", ResourceType = typeof(UI))]
        public string Description { get; set; }

        /// <summary> Organisasjon som har sendt inn filen.</summary>
        [Display(Name = "Owner", ResourceType = typeof(UI))]
        public string Owner { get; set; }

        /// <summary> Eier av datasettet</summary>
        [Display(Name = "OwnerDataset", ResourceType = typeof(UI))]
        public string OwnerDataset { get; set; }

        /// <summary> Editor. Hentes fra pålogget bruker</summary>
        [Display(Name = "LastEditedBy", ResourceType = typeof(UI))]
        public string LastEditedBy { get; set; }

        /// <summary>Opplasting av filen i form av sld/lyr/lyrx/pdf/zip</summary>
        [Display(Name = "FileName", ResourceType = typeof(UI))]
        public string FileName { get; set; }

        /// <summary>Dropdown (sld/qml/lyr/zip, leses fra kodeliste)</summary>
        [Display(Name = "Format")]
        public string Format { get; set; }

        /// <summary>Beskrivelse over hva filen skal brukes til. F.eks. style WMS eller GML.</summary>
        [Display(Name = "Compatibility", ResourceType = typeof(UI))]
        public virtual ICollection<Compatibility> Compatibility { get; set; }

        /// <summary>Dropdown,beskrivelse over hva filen skal brukes til, f.eks. style WMS eller GML.</summary>
        [Display(Name = "Use", ResourceType = typeof(UI))]
        public string Use { get; set; }

        /// <summary>Kobling til datasett</summary>
        [Display(Name = "Dataset")]
        public string DatasetUuid { get; set; }

        /// <summary></summary>
        [Display(Name = "DatasetName", ResourceType = typeof(UI))]
        public string DatasetName { get; set; }

        /// <summary>Kobling til tjeneste</summary>
        [Display(Name = "Service", ResourceType = typeof(UI))]
        public string ServiceUuid { get; set; }

        /// <summary></summary>
        [Display(Name = "ServiceName", ResourceType = typeof(UI))]
        public string ServiceName { get; set; }

        /// <summary>Et lite bilde som viser hvilke symboler som benyttes med beskrivende tekst.</summary>
        [Display(Name = "PreviewImage", ResourceType = typeof(UI))]
        public string PreviewImage { get; set; }

        /// <summary>Tallverdi for hvilken versjon av filen det er snakk om. Autogenereres.</summary>
        [Display(Name = "Version", ResourceType = typeof(UI))]
        public int VersionId { get; set; }

        [ForeignKey("versioning")]
        public Guid versioningId { get; set; }
        public virtual Version versioning { get; set; }

        /// <summary>Dato for når filen/informasjonen i registeret sist ble endret.</summary>
        [Display(Name = "DateChanged", ResourceType = typeof(UI))]
        [DisplayFormat(NullDisplayText = "", ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime DateChanged { get; set; } = DateTime.Now;

        /// <summary>(gjelder kun for «offisiell» digital kartografi) *Viser på samme måte som andre registre i geonorge om filen er godkjent eller ikke. Settes av administrator.</summary>
        [Display(Name = "Status")]
        public string Status { get; set; }

        /// <summary>Dato for når filen ble godkjent. Settes av administrator.</summary>
        [Display(Name = "DateAccepted", ResourceType = typeof(UI))]
        [DisplayFormat(NullDisplayText = "", ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        public DateTime? DateAccepted { get; set; }

        /// <summary>Tekst dersom det er noe som er viktig å påpeke ved godkjenningen. F.eks. dersom noe blir godkjent, men ikke er optimalt. Settes av administrator</summary>
        [Display(Name = "AcceptedComment", ResourceType = typeof(UI))]
        public string AcceptedComment { get; set; }

        /// <summary>Angi om kartografi er levert som offisielt tilbud eller som et alternativ til offisiell kartografi, radioknapp</summary>
        [Display(Name = "Official", ResourceType = typeof(UI))]
        public bool OfficialStatus { get; set; }

        /// <summary>Hvilke attributter er sentrale for bruk av kartografien, tekstfelt</summary>
        [Display(Name = "Properties", ResourceType = typeof(UI))]
        public string Properties { get; set; }

        /// <summary>Hentes automatisk fra metadata. Inn i db eller koples dette i visningsapplikasjon? Tematisk hovedkategori i metadataene</summary>
        [Display(Name = "Theme", ResourceType = typeof(UI))]
        public string Theme { get; set; }

        public virtual TranslationCollection<CartographyFileTranslation> Translations { get; set; }

        public void AddMissingTranslations()
        {
            Translations.AddMissingTranslations();
        }

        public string NameTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var nameTranslated = Translations[cultureName]?.Name;
            if (string.IsNullOrEmpty(nameTranslated))
                nameTranslated = Name;
            return nameTranslated;
        }

        public string DescriptionTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var descriptionTranslated = Translations[cultureName]?.Description;
            if (string.IsNullOrEmpty(descriptionTranslated))
                descriptionTranslated = Description;
            return descriptionTranslated;
        }

        public string DatasetNameTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var datasetNameTranslated = Translations[cultureName]?.DatasetName;
            if (string.IsNullOrEmpty(datasetNameTranslated))
                datasetNameTranslated = DatasetName;
            return datasetNameTranslated;
        }

        public string OwnerDatasetTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var ownerDatasetTranslated = Translations[cultureName]?.OwnerDataset;
            if (string.IsNullOrEmpty(ownerDatasetTranslated))
                ownerDatasetTranslated = OwnerDataset;
            return ownerDatasetTranslated;
        }

        public string ThemeTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var themeTranslated = Translations[cultureName]?.Theme;
            if (string.IsNullOrEmpty(themeTranslated))
                themeTranslated = Theme;
            return themeTranslated;
        }

        public string OwnerTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var ownerTranslated = Translations[cultureName]?.Owner;
            if (string.IsNullOrEmpty(ownerTranslated))
                ownerTranslated = Owner;
            return ownerTranslated;
        }

        public string UseTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var useTranslated = Translations[cultureName]?.Use;
            if (string.IsNullOrEmpty(useTranslated))
                useTranslated = Use;
            return useTranslated;
        }

        public string PropertiesTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var propertiesTranslated = Translations[cultureName]?.Properties;
            if (string.IsNullOrEmpty(propertiesTranslated))
                propertiesTranslated = Properties;
            return propertiesTranslated;
        }

        public string ServiceNameTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            var serviceNameTranslated = Translations[cultureName]?.ServiceName;
            if (string.IsNullOrEmpty(serviceNameTranslated))
                serviceNameTranslated = ServiceName;
            return serviceNameTranslated;
        }

        public string StatusTranslated()
        {
            var cultureName = CultureHelper.GetCurrentCulture();
            if (CultureHelper.IsNorwegian(cultureName))
                return CodeList.Status[Status];
            else
                return Status;
        }

        public string FileUrl()
        {
            return CurrentDomain() + "/files/" + FileName;
        }

        public string PreviewImageUrl()
        {
            return CurrentDomain() + "/files/" + PreviewImage;
        }

        public string DetailsUrl()
        {
            return CurrentDomain() + "/files/Details?SystemId=" + SystemId;
        }

        string CurrentDomain()
        {
            var urlScheme = HttpContext.Current.Request.Url.Scheme;

            if(!urlScheme.EndsWith("s"))
                urlScheme += "s";

            return urlScheme + System.Uri.SchemeDelimiter
                 + HttpContext.Current.Request.Url.Host +
                 (HttpContext.Current.Request.Url.IsDefaultPort ? "" : ":" + HttpContext.Current.Request.Url.Port)
                 +(!HttpContext.Current.Request.Url.Host.Contains("localhost") ? "/register/kartografi" : "");
        }
    }
}