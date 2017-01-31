using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;
using System.Data.Entity;

namespace Geonorge.Kartografi.Services
{
    public class CartographyService : ICartographyService
    {
        private readonly CartographyDbContext _dbContext;
        private IVersioningService _versioningService;

        public CartographyService(CartographyDbContext dbContext, IVersioningService versioningService)
        {
            _dbContext = dbContext;
            _versioningService = versioningService;
        }

        public List<Dataset> GetDatasets()
        {
            return _dbContext.CartographyFiles
                .Select(d => new Dataset{ DatasetUuid = d.DatasetUuid, DatasetName = d.DatasetName, Theme = d.Theme, OwnerOrganization = d.OwnerOrganization })
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

        public void AddCartography(CartographyFile cartographyFile)
        {
            cartographyFile.SystemId = Guid.NewGuid();
            cartographyFile.versioningId = _versioningService.GetVersioningId(cartographyFile, null);
            _dbContext.CartographyFiles.Add(cartographyFile);
            _dbContext.SaveChanges();

        }

        public void UpdateCartography(CartographyFile cartographyFile)
        {

            var deleteSql = "DELETE from Compatibilities where CartographyFile_SystemId = {0}";
            _dbContext.Database.ExecuteSqlCommand(deleteSql, cartographyFile.SystemId);
            _dbContext.SaveChanges();

            foreach (var item in cartographyFile.Compatibility.ToList()) {
                var insertSql = "INSERT INTO Compatibilities(Id, [Key], CartographyFile_SystemId) Values ({0}, {1}, {2} )";
                _dbContext.Database.ExecuteSqlCommand(insertSql, Guid.NewGuid().ToString(), item.Key, cartographyFile.SystemId);
                _dbContext.SaveChanges();
            }

            _dbContext.Entry(cartographyFile).State = EntityState.Modified;
            _dbContext.SaveChanges();

        }

        public void AddCartographyVersion(CartographyFile cartographyFile)
        {
            CartographyFile originalCartographyFile = GetCartography(cartographyFile.SystemId);
            cartographyFile.SystemId = Guid.NewGuid();
            cartographyFile.versioningId = _versioningService.GetVersioningId(cartographyFile, originalCartographyFile);
            _dbContext.CartographyFiles.Add(cartographyFile);
            _dbContext.SaveChanges();

        }

        public void RemoveCartography(CartographyFile cartographyFile)
        {
            _dbContext.CartographyFiles.Remove(cartographyFile);
            _dbContext.SaveChanges();
        }

        public VersionsItem Versions(Guid? SystemId)
        {
            return _versioningService.Versions(SystemId);
        }
    }
}