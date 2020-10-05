using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.IO;
using Geonorge.Kartografi.Helpers;
using System.Security.Claims;
using Geonorge.AuthLib.Common;

namespace Geonorge.Kartografi.Services
{
    public class CartographyService : ICartographyService
    {
        private readonly CartographyDbContext _dbContext;
        private IVersioningService _versioningService;
        private readonly IAuthorizationService _authorizationService;

        public CartographyService(CartographyDbContext dbContext, IVersioningService versioningService, IAuthorizationService authorizationService)
        {
            _dbContext = dbContext;
            _versioningService = versioningService;
            _authorizationService = authorizationService;
        }

        public List<Dataset> GetDatasets(string text = null, bool limitofficial = false, string owner = null, string datasetowner= null)
        {
            var query = _dbContext.CartographyFiles.AsQueryable();
            query = query.Where(c => c.SystemId == c.versioning.CurrentVersion);
            List<Dataset> datasets;
            var culture = CultureHelper.GetCurrentCulture();

            if (!string.IsNullOrEmpty(text))
            {
                query = query.Where(s => s.DatasetName.Contains(text) || s.DatasetUuid.Contains(text) || s.Description.Contains(text) || s.FileName.Contains(text) || s.Format.Contains(text)
                || s.Name.Contains(text) || s.Properties.Contains(text) || s.Theme.Contains(text) || s.Use.Contains(text)
                || s.OwnerDataset.Contains(text) || s.Owner.Contains(text) 
                || s.Translations.Any(d => d.DatasetName.Contains(text)) || s.Translations.Any(d => d.Description.Contains(text))
                || s.Translations.Any(d => d.Name.Contains(text)) || s.Translations.Any(d => d.Theme.Contains(text))
                || s.Translations.Any(d => d.Owner.Contains(text)) || s.Translations.Any(d => d.OwnerDataset.Contains(text))
                || s.Translations.Any(d => d.Use.Contains(text)) || s.Translations.Any(d => d.Properties.Contains(text))
                );


                if (limitofficial)
                    query = query.Where(l => l.OfficialStatus == true);

                if(!string.IsNullOrEmpty(owner))
                    query = query.Where(o => o.Owner == owner);

                if (!string.IsNullOrEmpty(datasetowner))
                    query = query.Where(o => o.OwnerDataset == datasetowner);

                datasets = query.ToList().Select(d => new { DatasetUuid = d.DatasetUuid, DatasetName = d.DatasetNameTranslated(), Theme = d.ThemeTranslated(), OwnerDataset = d.OwnerDatasetTranslated() })
                    .Distinct().Select(x => new Dataset { DatasetUuid = x.DatasetUuid, DatasetName = x.DatasetName, Theme = x.Theme, OwnerDataset = x.OwnerDataset })
                    .ToList();

            }
            else
            {
                if (limitofficial)
                    query = query.Where(l => l.OfficialStatus == true);

                if (!string.IsNullOrEmpty(owner))
                    query = query.Where(o => o.Owner == owner);

                if (!string.IsNullOrEmpty(datasetowner))
                    query = query.Where(o => o.OwnerDataset == datasetowner);

                datasets = query.ToList().Select(d =>  new { DatasetUuid = d.DatasetUuid, DatasetName = d.DatasetNameTranslated(), Theme = d.ThemeTranslated(), OwnerDataset = d.OwnerDatasetTranslated()  })
                    .Distinct().Select(x => new Dataset { DatasetUuid = x.DatasetUuid, DatasetName = x.DatasetName, Theme = x.Theme, OwnerDataset = x.OwnerDataset })
                    .ToList();
                }
            for (int d = 0; d < datasets.Count; d++)
            {
                if (!string.IsNullOrEmpty(datasets[d].DatasetUuid))
                {
                    var files = GetCartography(datasets[d].DatasetUuid).ToList();
                    foreach(var file in files)
                    {
                        if (limitofficial) { 
                            if(file.OfficialStatus)
                                datasets[d].Files.Add(file);
                        }
                        else
                            datasets[d].Files.Add(file);
                    }
                }
            }

            return datasets;
        }

        public List<CartographyFile> GetCartography(string uuid = null)
        {
            if(string.IsNullOrEmpty(uuid))
                return _dbContext.CartographyFiles.Where(c => c.SystemId == c.versioning.CurrentVersion).ToList();
            else
                return _dbContext.CartographyFiles.Where(c => c.SystemId == c.versioning.CurrentVersion && c.DatasetUuid == uuid).ToList();
        }

        public CartographyFile GetCartography(Guid? SystemId)
        {
            var cartography = _dbContext.CartographyFiles.Find(SystemId);
            if (cartography != null)
                cartography.AddMissingTranslations();

            return cartography;
        }

        public void AddCartography(CartographyFile cartographyFile, HttpPostedFileBase uploadFile = null, HttpPostedFileBase uploadPreviewImage = null)
        {
            string owner = ClaimsPrincipal.Current.GetOrganizationName();
            if (_authorizationService.IsAdmin() && !string.IsNullOrEmpty(cartographyFile.Owner))
                owner = cartographyFile.Owner;

            cartographyFile.SystemId = Guid.NewGuid();
            cartographyFile.VersionId = 1;
            cartographyFile.versioningId = _versioningService.GetVersioningId(cartographyFile, null);
            cartographyFile.PreviewImage = CreateThumbnailFileName(cartographyFile, uploadPreviewImage);
            cartographyFile.Format = "sld";
            if (uploadFile != null)
            {
                string extension = Path.GetExtension(uploadFile.FileName);
                extension = extension.Replace(".", "");
                if ((extension == "sld" || extension == "lyr" || extension == "qml"))
                    cartographyFile.Format = extension;
            }
            cartographyFile.FileName = CreateFileName(cartographyFile);
            cartographyFile.Owner = owner;
            cartographyFile.LastEditedBy = ClaimsPrincipal.Current.GetUsername();
            _dbContext.CartographyFiles.Add(cartographyFile);
            _dbContext.SaveChanges();
            SaveFile(uploadFile, cartographyFile.FileName);
            SaveFile(uploadPreviewImage, cartographyFile.PreviewImage);
        }

        public void UpdateCartography(CartographyFile originalFile, CartographyFile file, HttpPostedFileBase uploadPreviewImage = null)
        {
            if (uploadPreviewImage != null)
            {
                originalFile.PreviewImage = CreateThumbnailFileName(file, uploadPreviewImage);
            }

            if(file != null)
            {
                originalFile.Name = file.Name;
                originalFile.Description = file.Description;
                originalFile.Use = file.Use;
                originalFile.Properties = file.Properties;
                originalFile.DatasetUuid = file.DatasetUuid;
                originalFile.DatasetName = file.DatasetName;
                originalFile.ServiceUuid = file.ServiceUuid;
                originalFile.ServiceName = file.ServiceName;
                originalFile.Status = file.Status;
                originalFile.AcceptedComment = file.AcceptedComment;
                originalFile.DateAccepted = file.DateAccepted;
                originalFile.DateChanged = DateTime.Now;
                string owner = ClaimsPrincipal.Current.GetOrganizationName();
                if (_authorizationService.IsAdmin() && !string.IsNullOrEmpty(file.Owner))
                    owner = file.Owner;
                originalFile.Owner = owner;
                originalFile.OwnerDataset = file.OwnerDataset;
                originalFile.Theme = file.Theme;

                var deleteSql = "DELETE from Compatibilities where CartographyFile_SystemId = {0}";
                _dbContext.Database.ExecuteSqlCommand(deleteSql, originalFile.SystemId);
                _dbContext.SaveChanges();

                foreach (var item in file.Compatibility.ToList()) {
                    var insertSql = "INSERT INTO Compatibilities(Id, [Key], CartographyFile_SystemId) Values ({0}, {1}, {2} )";
                    _dbContext.Database.ExecuteSqlCommand(insertSql, Guid.NewGuid().ToString(), item.Key, originalFile.SystemId);
                    _dbContext.SaveChanges();
                }

                _dbContext.Database.ExecuteSqlCommand("DELETE FROM CartographyFileTranslations WHERE CartographyFileId = {0}", originalFile.SystemId);
                foreach (var translation in file.Translations.ToList())
                {
                    var translated = new DataSync(_dbContext).GetTranslations(translation.CultureName, file);
                    _dbContext.Database.ExecuteSqlCommand("INSERT INTO CartographyFileTranslations" +
                        "(CartographyFileId,Name,Description,CultureName, Id, [Use], Properties, Owner, OwnerDataset, DatasetName, Theme, ServiceName)" +
                        " VALUES({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})",
                    translation.CartographyFileId, translation.Name, translation.Description, translation.CultureName, Guid.NewGuid(),
                    translation.Use, translation.Properties, translated.Owner, translated.OwnerDataset, translated.DatasetName, translated.Theme, translated.ServiceName);
                }
            }

            originalFile.LastEditedBy = ClaimsPrincipal.Current.GetUsername();
            _dbContext.Entry(originalFile).State = EntityState.Modified;
            _dbContext.SaveChanges();
            if (uploadPreviewImage != null)
                SaveFile(uploadPreviewImage, originalFile.PreviewImage);

        }

        public void AddCartographyVersion(CartographyFile cartographyFile, HttpPostedFileBase uploadFile = null, HttpPostedFileBase uploadPreviewImage = null)
        {
            CartographyFile originalCartographyFile = GetCartography(cartographyFile.SystemId);
            cartographyFile.SystemId = Guid.NewGuid();
            cartographyFile.VersionId = _versioningService.GetNewVersionNumber(originalCartographyFile);
            cartographyFile.versioningId = _versioningService.GetVersioningId(cartographyFile, originalCartographyFile);
            cartographyFile.PreviewImage = CreateThumbnailFileName(cartographyFile, uploadPreviewImage);
            cartographyFile.Format = "sld";
            if (uploadFile != null)
            {
                string extension = Path.GetExtension(uploadFile.FileName);
                extension = extension.Replace(".", "");
                if ((extension == "sld" || extension == "lyr" || extension == "qml"))
                    cartographyFile.Format = extension;
            }
            cartographyFile.FileName = CreateFileName(cartographyFile);
            cartographyFile.Owner = ClaimsPrincipal.Current.GetOrganizationName();
            cartographyFile.LastEditedBy = ClaimsPrincipal.Current.GetUsername();
            if (string.IsNullOrEmpty(cartographyFile.Status))
                cartographyFile.Status = "Submitted";
            _dbContext.CartographyFiles.Add(cartographyFile);
            _dbContext.SaveChanges();
            SaveFile(uploadFile, cartographyFile.FileName);
            SaveFile(uploadPreviewImage, cartographyFile.PreviewImage);

        }

        public CartographyFile RemoveCartography(CartographyFile cartographyFile)
        {
            CartographyFile newVersion = null;
            CartographyFile originalCartographyFile = GetCartography(cartographyFile.SystemId);
            var versioning = originalCartographyFile.versioning;
            if (originalCartographyFile.SystemId == versioning.CurrentVersion)
            {
                var versions = _dbContext.CartographyFiles.Where(v => v.SystemId != cartographyFile.SystemId &&  v.versioning.SystemId == versioning.SystemId).ToList().OrderByDescending(s => s.versioning.LastVersionNumber);

                if(versions != null && versions.Count() > 0)
                { 
                    newVersion = versions.First();

                    foreach (var version in versions)
                    {
                        if (version.OfficialStatus)
                        {
                            newVersion = version;
                            break;
                        }
                    }

                    Models.Version versionGroup = _versioningService.GetVersionGroup(newVersion.versioningId);
                    versionGroup.CurrentVersion = newVersion.SystemId;
                    versionGroup.LastVersionNumber = newVersion.VersionId;
                    if (newVersion.OfficialStatus)
                        newVersion.Status = "Accepted";
                    else
                        newVersion.Status = "Submitted";

                    _dbContext.SaveChanges();

                }
            }
            _dbContext.CartographyFiles.Remove(cartographyFile);
            _dbContext.SaveChanges();
            DeleteFile(cartographyFile.FileName);
            DeleteFile(cartographyFile.PreviewImage);

            return newVersion;
        }

        public string CreateFileName(CartographyFile cartographyFile)
        {
            string name = cartographyFile.Name;
            string compability = "";

            var compatibilityList = cartographyFile.Compatibility.ToList();

            for (int c = 0; c < compatibilityList.Count; c++)
            {
                compability = compability + compatibilityList[c];
                if (c != compatibilityList.Count - 1)
                    compability = compability + "-";
            }
            compability = compability.ToLower();
            string version = cartographyFile.VersionId.ToString();
            string format = cartographyFile.Format;
            name = MakeSeoFriendlyString(name);

            string filename = name + "_" + compability + "_v" + version + "." + format;

            return filename;
        }

        public string CreateThumbnailFileName(CartographyFile cartographyFile, HttpPostedFileBase image)
        {
            string filename = null;

            if (image != null)
            {
                string name = cartographyFile.Name;
                string compability = "";

                var compatibilityList = cartographyFile.Compatibility.ToList();

                for (int c = 0; c < compatibilityList.Count; c++)
                {
                    compability = compability + compatibilityList[c];
                    if (c != compatibilityList.Count - 1)
                        compability = compability + "-";
                }
                compability = compability.ToLower();
                string version = cartographyFile.VersionId.ToString();
                name = MakeSeoFriendlyString(name);
                filename = name + "_" + compability + "_v" + version + Path.GetExtension(image.FileName);
            }

            return filename;
        }

        public static string MakeSeoFriendlyString(string input)
        {
            string encodedUrl = (input ?? "").ToLower();
            // replace & with and
            encodedUrl = Regex.Replace(encodedUrl, @"\&+", "and");

            // remove characters
            encodedUrl = encodedUrl.Replace("'", "");

            // replace norwegian characters
            encodedUrl = encodedUrl.Replace("å", "a").Replace("æ", "ae").Replace("ø", "o");

            // remove invalid characters
            encodedUrl = Regex.Replace(encodedUrl, @"[^a-z0-9]", "");

            // trim leading & trailing characters
            encodedUrl = encodedUrl.Trim(' ');

            return encodedUrl;
        }

        private string SaveFile(HttpPostedFileBase file, string fileName)
        {
            if (file != null)
            {
                string targetFolder = System.Web.HttpContext.Current.Server.MapPath("~/files");
                string targetPath = Path.Combine(targetFolder, fileName);
                file.SaveAs(targetPath);
            }

            return fileName;
        }

        private void DeleteFile(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string targetFolder = System.Web.HttpContext.Current.Server.MapPath("~/files");
                string targetPath = Path.Combine(targetFolder, fileName);
                if(File.Exists(targetPath))
                    File.Delete(targetPath);
            }
        }

        public VersionsItem Versions(Guid? SystemId)
        {
            return _versioningService.Versions(SystemId);
        }
    }
}