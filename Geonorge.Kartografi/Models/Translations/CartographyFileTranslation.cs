using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Geonorge.Kartografi.Models.Translations
{
    public class CartographyFileTranslation : Translation<CartographyFileTranslation>
    {

        public Guid CartographyFileId { get; set; }

        public string Owner { get; set; }
        public string OwnerDataset { get; set; }
        public string DatasetName { get; set; }
        public string ServiceName { get; set; }
        public string Use { get; set; }
        public string Properties { get; set; }
        public string Theme { get; set; }

        public CartographyFileTranslation()
        {
            Id = Guid.NewGuid();
        }

    }
}