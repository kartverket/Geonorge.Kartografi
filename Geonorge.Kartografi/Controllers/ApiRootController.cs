using Geonorge.Kartografi.Services;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Geonorge.Kartografi.Models;
using System.Web.Http.Description;
using Geonorge.Kartografi.Helpers;
using Geonorge.Kartografi.Models.Translations;

namespace Geonorge.Kartografi.Controllers
{
    public class ApiRootController : ApiController
    {
        ICartographyService _cartographyService;
        private readonly IAuthorizationService _authorizationService;
        private readonly CartographyDbContext _dbContext;

        private static readonly ILog Log = LogManager.GetLogger(typeof(MvcApplication));

        public ApiRootController(ICartographyService cartographyService, IAuthorizationService authorizationService, CartographyDbContext dbContext)
        {
            _cartographyService = cartographyService;
            _authorizationService = authorizationService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// List items, optional limit by text
        /// </summary>
        [Route("api/kartografi")]
        [HttpGet]
        public List<Models.Api.Cartography> GetCartography([FromUri] string text = null, bool limitofficial = false)
        {
            var cartographyFiles = ConvertRegister(_cartographyService.GetDatasets(text, limitofficial), limitofficial);
                       
            return cartographyFiles.OrderBy(o => o.DatasetName).ThenBy(oo => oo.Name).ToList();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("api/syncdata")]
        [HttpGet]
        public IHttpActionResult SyncData()
        {
            new DataSync(_dbContext).UpdateAllExternal();

            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK));
        }

        private List<Models.Api.Cartography> ConvertRegister(List<Dataset> cartographyFiles, bool limitofficial = false)
        {
            var culture = CultureHelper.GetCurrentCulture();
            var cartograhyList = new List<Models.Api.Cartography>();
            foreach (var dataset in cartographyFiles)
            {
                foreach (var cartography in dataset.Files)
                {

                    var file = new Models.Api.Cartography();
                    file.Compatibility = FormatCompability(cartography.Compatibility);
                    file.DatasetName = cartography.DatasetNameTranslated();
                    file.DatasetUuid = cartography.DatasetUuid;
                    file.DateAccepted = cartography.DateAccepted;
                    file.DateChanged = cartography.DateChanged;
                    file.Description = cartography.DescriptionTranslated();
                    file.FileName = cartography.FileName;
                    file.FileUrl = cartography.FileUrl();
                    file.Format = cartography.Format;
                    file.Name = cartography.NameTranslated();
                    file.OfficialStatus = cartography.OfficialStatus;
                    file.Owner = cartography.OwnerTranslated();
                    file.OwnerDataset = cartography.OwnerDatasetTranslated();
                    file.PreviewImage = cartography.PreviewImage;
                    file.PreviewImageUrl = cartography.PreviewImageUrl();
                    file.Properties = cartography.PropertiesTranslated();
                    file.ServiceName = cartography.ServiceNameTranslated();
                    file.ServiceUuid = cartography.ServiceUuid;
                    file.Status = !CultureHelper.IsNorwegian(culture) ? cartography.Status : CodeList.Status[cartography.Status];
                    file.Theme = cartography.ThemeTranslated();
                    file.Use = cartography.UseTranslated();
                    file.Uuid = cartography.SystemId;
                    file.VersionId = cartography.VersionId;

                    if (limitofficial)
                    {
                        if (file.OfficialStatus)
                            cartograhyList.Add(file);
                    }
                    else
                        cartograhyList.Add(file);

                }
            }

            return cartograhyList;
        }

        private string FormatCompability(ICollection<Compatibility> compatibilityList)
        {
            string output = "";

            foreach (var compatibility in compatibilityList)
            { 

            output = output + compatibility;

                if(!compatibilityList.LastOrDefault().Equals(compatibility))
                {
                    output = output + ", ";
                }
            }

            return output;
        }
    }
}
