using Geonorge.Kartografi.Models;
using Geonorge.Kartografi.Models.Translations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Geonorge.Kartografi.Services
{
    public class DataSync
    {
        private static readonly WebClient _webClient = new WebClient();
        private readonly CartographyDbContext _dbContext;
        public DataSync(CartographyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public CartographyFile GetTranslations(string cultureName, CartographyFile originalFile)
        {
            CartographyFile fileTranslated = new CartographyFile();

            fileTranslated.Owner = originalFile.Owner;
            fileTranslated.OwnerDataset = originalFile.OwnerDataset;
            fileTranslated.DatasetName = originalFile.DatasetName;
            fileTranslated.Theme = originalFile.Theme;
            fileTranslated.ServiceName = originalFile.ServiceName;

            try {

                string url = System.Web.Configuration.WebConfigurationManager.AppSettings["RegistryUrl"] + "api/organisasjon/navn/" + originalFile.Owner + "/" + cultureName;
                _webClient.Headers.Remove("Accept-Language");
                _webClient.Headers.Add("Accept-Language", cultureName);
                _webClient.Encoding = System.Text.Encoding.UTF8;

                try { 
                var data = _webClient.DownloadString(url);
                var response = Newtonsoft.Json.Linq.JObject.Parse(data);
                JToken name = response["Name"];
                if (name != null && !string.IsNullOrEmpty(name.ToString()))
                    fileTranslated.Owner = name.ToString();
                }
                catch (Exception ex) { }

                try
                {
                    url = System.Web.Configuration.WebConfigurationManager.AppSettings["KartkatalogenUrl"] + "api/getdata/" + originalFile.DatasetUuid;
                    var data = _webClient.DownloadString(url);
                    var response = Newtonsoft.Json.Linq.JObject.Parse(data);
                    var metadataOwner = response["ContactOwner"];
                    if (metadataOwner != null)
                    {
                        var organizationEnglish = metadataOwner.SelectToken("OrganizationEnglish");
                        if (organizationEnglish != null && !string.IsNullOrEmpty(organizationEnglish.ToString()))
                            fileTranslated.OwnerDataset = organizationEnglish.ToString();
                    }

                    var datasetName = response["EnglishTitle"];
                    if (datasetName != null && !string.IsNullOrEmpty(datasetName.ToString()))
                        fileTranslated.DatasetName = datasetName.ToString();

                    var theme = response["KeywordsNationalTheme"];
                    if (theme != null)
                    {
                        var keywordValue = theme[0].SelectToken("EnglishKeyword");
                        if (keywordValue != null && !string.IsNullOrEmpty(keywordValue.ToString()))
                            fileTranslated.Theme = keywordValue.ToString();
                    }
                }
                catch (Exception ex) { }

                if (!string.IsNullOrEmpty(originalFile.ServiceUuid))
                {
                    try
                    {
                        url = System.Web.Configuration.WebConfigurationManager.AppSettings["KartkatalogenUrl"] + "api/getdata/" + originalFile.ServiceUuid;
                        var data = _webClient.DownloadString(url);
                        var response = Newtonsoft.Json.Linq.JObject.Parse(data);

                        var serviceName = response["EnglishTitle"];

                        if (serviceName != null && !string.IsNullOrEmpty(serviceName.ToString()))
                            fileTranslated.ServiceName = serviceName.ToString();
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex) { }


            return fileTranslated;
        }

        public void UpdateAllExternal()
        {
            var files = _dbContext.CartographyFiles.ToList();
            foreach (var file in files)
            {
                file.AddMissingTranslations();
                foreach (var translation in file.Translations)
                {
                    var fileTranslated = GetTranslations(translation.CultureName, file);
                    translation.DatasetName = fileTranslated.DatasetName;
                    translation.Owner = fileTranslated.Owner;
                    translation.OwnerDataset = fileTranslated.OwnerDataset;
                    translation.ServiceName = fileTranslated.ServiceName;
                    translation.Theme = fileTranslated.Theme;

                    _dbContext.SaveChanges();
                }
            }
        }
    }
}