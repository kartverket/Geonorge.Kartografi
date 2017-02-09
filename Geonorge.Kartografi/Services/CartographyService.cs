using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.IO;

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

        public List<Dataset> GetDatasets()
        {
            return _dbContext.CartographyFiles
                .Select(d => new Dataset{ DatasetUuid = d.DatasetUuid, DatasetName = d.DatasetName, Theme = d.Theme, OwnerDataset = d.OwnerDataset })
                .Distinct()
                .ToList();
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
            return _dbContext.CartographyFiles.Find(SystemId);
        }

        public void AddCartography(CartographyFile cartographyFile, HttpPostedFileBase uploadFile = null, HttpPostedFileBase uploadPreviewImage = null)
        {
            string owner = _authorizationService.GetSecurityClaim("organization").FirstOrDefault();
            if (_authorizationService.IsAdmin() && !string.IsNullOrEmpty(cartographyFile.Owner))
                owner = cartographyFile.Owner;

            cartographyFile.SystemId = Guid.NewGuid();
            cartographyFile.VersionId = 1;
            cartographyFile.versioningId = _versioningService.GetVersioningId(cartographyFile, null);
            cartographyFile.PreviewImage = CreateThumbnailFileName(cartographyFile, uploadPreviewImage);
            cartographyFile.FileName = CreateFileName(cartographyFile);
            cartographyFile.Owner = owner;
            cartographyFile.LastEditedBy = _authorizationService.GetSecurityClaim("username").FirstOrDefault();
            _dbContext.CartographyFiles.Add(cartographyFile);
            _dbContext.SaveChanges();
            SaveFile(uploadFile, cartographyFile.FileName);
            SaveFile(uploadPreviewImage, cartographyFile.PreviewImage);
        }

        public void UpdateCartography(CartographyFile originalFile, CartographyFile file)
        {
            if(file != null)
            {
                originalFile.Name = file.Name;
                originalFile.Description = file.Description;
                originalFile.Format = file.Format;
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
                string owner = _authorizationService.GetSecurityClaim("organization").FirstOrDefault();
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
            }

            originalFile.LastEditedBy = _authorizationService.GetSecurityClaim("username").FirstOrDefault();
            _dbContext.Entry(originalFile).State = EntityState.Modified;
            _dbContext.SaveChanges();

        }

        public void AddCartographyVersion(CartographyFile cartographyFile, HttpPostedFileBase uploadFile = null, HttpPostedFileBase uploadPreviewImage = null)
        {
            CartographyFile originalCartographyFile = GetCartography(cartographyFile.SystemId);
            cartographyFile.SystemId = Guid.NewGuid();
            cartographyFile.VersionId = originalCartographyFile.versioning.LastVersionNumber + 1;
            cartographyFile.versioningId = _versioningService.GetVersioningId(cartographyFile, originalCartographyFile);
            cartographyFile.PreviewImage = CreateThumbnailFileName(cartographyFile, uploadPreviewImage);
            cartographyFile.FileName = CreateFileName(cartographyFile);
            cartographyFile.Owner = _authorizationService.GetSecurityClaim("organization").FirstOrDefault();
            cartographyFile.LastEditedBy = _authorizationService.GetSecurityClaim("username").FirstOrDefault();
            _dbContext.CartographyFiles.Add(cartographyFile);
            _dbContext.SaveChanges();
            SaveFile(uploadFile, cartographyFile.FileName);
            SaveFile(uploadPreviewImage, cartographyFile.PreviewImage);

        }

        public void RemoveCartography(CartographyFile cartographyFile)
        {
            CartographyFile originalCartographyFile = GetCartography(cartographyFile.SystemId);
            var versioning = originalCartographyFile.versioning;
            if (originalCartographyFile.SystemId == versioning.CurrentVersion)
            {
                var versions = _dbContext.CartographyFiles.Where(v => v.SystemId != cartographyFile.SystemId &&  v.versioning.SystemId == versioning.SystemId).ToList().OrderByDescending(s => s.versioning.LastVersionNumber);

                if(versions != null && versions.Count() > 0)
                { 
                    var newVersion = versions.First();

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
                    _dbContext.SaveChanges();

                }
            }
            _dbContext.CartographyFiles.Remove(cartographyFile);
            _dbContext.SaveChanges();
            DeleteFile(cartographyFile.FileName);
            DeleteFile(cartographyFile.PreviewImage);
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