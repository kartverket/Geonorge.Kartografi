using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Geonorge.Kartografi.Models;
using System.Data.Entity;

namespace Geonorge.Kartografi.Services
{
    public class VersioningService : IVersioningService
    {
        private readonly CartographyDbContext _dbContext;

        public VersioningService(CartographyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Guid GetVersioningId(CartographyFile cartographyFile, CartographyFile originalCartographyFile)
        {
            if (originalCartographyFile == null || cartographyFile.versioningId == null || cartographyFile.versioningId == Guid.Empty)
            {
                return NewVersioningGroup(cartographyFile);
            }
            else if (originalCartographyFile.versioningId != Guid.Empty && originalCartographyFile!= null && originalCartographyFile.versioningId != null)
            {
                UpdateLatestVersioningGroup(originalCartographyFile.versioningId, cartographyFile);
                return originalCartographyFile.versioningId;
            }
            else
            {
                return cartographyFile.versioningId;
            }

        }

        private void UpdateLatestVersioningGroup(Guid versioningId, CartographyFile cartographyFile)
        {
            Models.Version versjonsgruppe = GetVersionGroup(versioningId);
            if (cartographyFile.OfficialStatus)
                versjonsgruppe.CurrentVersion = cartographyFile.SystemId;

            versjonsgruppe.LastVersionNumber = cartographyFile.VersionId;
            _dbContext.SaveChanges();
        }


        public Models.Version GetVersionGroup(Guid? versioningId)
        {
            var queryResultVersions = from v in _dbContext.Versions
                                      where v.SystemId == versioningId
                                      select v;

            Models.Version versjonsgruppe = queryResultVersions.FirstOrDefault();
            return versjonsgruppe;
        }

        private Guid NewVersioningGroup(CartographyFile cartographyFile)
        {
            Models.Version versjoneringsGruppe = new Models.Version();
            versjoneringsGruppe.SystemId = Guid.NewGuid();
            versjoneringsGruppe.CurrentVersion = cartographyFile.SystemId;
            versjoneringsGruppe.LastVersionNumber = cartographyFile.VersionId;

            _dbContext.Entry(versjoneringsGruppe).State = EntityState.Modified;
            _dbContext.Versions.Add(versjoneringsGruppe);
            _dbContext.SaveChanges();
            return versjoneringsGruppe.SystemId;
        }

        public VersionsItem Versions(Guid? SystemId)
        {
            // Find versionGroup
            Models.Version versjonsGruppe = new Models.Version();
            
            var queryResultsRegisteritem = from ca in _dbContext.CartographyFiles
                                            where ca.SystemId == SystemId
                                           select ca.versioning;

                versjonsGruppe = queryResultsRegisteritem.FirstOrDefault();
            
            
            Guid? versjonsGruppeId = versjonsGruppe.SystemId;

            Guid currentVersionId = versjonsGruppe.CurrentVersion;
            List<CartographyFile> SuggestionsItems = new List<CartographyFile>();
            List<CartographyFile> HistoricalItems = new List<CartographyFile>();

            // find current version in versionGroup
            var queryResults = from ca in _dbContext.CartographyFiles
                               where ca.SystemId == currentVersionId
                               select ca;

            CartographyFile CurrentVersion = queryResults.FirstOrDefault();

            // Finne all suggested
            queryResults = from ca in _dbContext.CartographyFiles
                           where
                           ca.versioningId == versjonsGruppeId
                           && (ca.Status == "Submitted" && ca.SystemId!= currentVersionId)                  
                           select ca;

            foreach (CartographyFile item in queryResults.ToList())
            {
                    SuggestionsItems.Add(item);
            }


            //Find historical versions
            var queryResultsHistorical = from ca in _dbContext.CartographyFiles
                                         where ca.versioningId == CurrentVersion.versioningId
                                         && ca.SystemId != currentVersionId
                                          && (ca.Status == "Superseded"
                                          || ca.Status == "Retired")                                        
                                         select ca;

            foreach (CartographyFile item in queryResultsHistorical.ToList())
            {
                HistoricalItems.Add(item);
            }

            return new VersionsItem
            {
                CurrentVersion = CurrentVersion,
                Historical = HistoricalItems,
                Suggestions = SuggestionsItems
            };


        }
    }
}