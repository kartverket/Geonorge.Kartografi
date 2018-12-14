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
using System.Net.Http.Headers;
using System.Globalization;
using System.Threading;
using Geonorge.Kartografi.Models.Api;

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
        public List<Models.Api.Cartography> GetCartography([FromUri] string text = null, bool limitofficial = false, string owner = null)
        {
            SetLanguage(Request);

            var cartographyFiles = ConvertRegister(_cartographyService.GetDatasets(text, limitofficial, owner), limitofficial);
                       
            return cartographyFiles.OrderBy(o => o.DatasetName).ThenBy(oo => oo.Name).ToList();
        }

        /// <summary>
        /// List items with limit and offset
        /// </summary>
        [Route("api/cartography")]
        [HttpGet]
        public CartographyResult GetCartographyResult([FromUri] string text = null, bool limitofficial = false, string owner = null, int limit = 1000, int offset = 0)
        {
            SetLanguage(Request);

            var cartographyFiles = ConvertRegister(_cartographyService.GetDatasets(text, limitofficial, owner), limitofficial);
            CartographyResult result = new CartographyResult();

            var files = cartographyFiles.OrderBy(o => o.DatasetName).ThenBy(oo => oo.Name).Skip(offset).Take(limit).ToList();
            result.Files = files;
            result.Count = files.Count;
            result.Limit = limit;
            result.Offset = offset;
            result.Total = cartographyFiles.Count;

            return result;
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
                    file.Status = cartography.StatusTranslated();
                    file.Theme = cartography.ThemeTranslated();
                    file.Use = cartography.UseTranslated();
                    file.Uuid = cartography.SystemId;
                    file.VersionId = cartography.VersionId;
                    file.DetailsUrl = cartography.DetailsUrl();

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

        private void SetLanguage(HttpRequestMessage request)
        {
            string language = Culture.NorwegianCode;

            IEnumerable<string> headerValues;
            if (request.Headers.TryGetValues("Accept-Language", out headerValues))
            {
                language = headerValues.FirstOrDefault();
                if (CultureHelper.IsNorwegian(language))
                    language = Culture.NorwegianCode;
                else
                    language = Culture.EnglishCode;
            }
            else
            {
                CookieHeaderValue cookie = request.Headers.GetCookies("_culture").FirstOrDefault();
                if (cookie != null && !string.IsNullOrEmpty(cookie["_culture"].Value))
                {
                    language = cookie["_culture"].Value;
                }
            }

            var culture = new CultureInfo(language);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

        }
    }
}
