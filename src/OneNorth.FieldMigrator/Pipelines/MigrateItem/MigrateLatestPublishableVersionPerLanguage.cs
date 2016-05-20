using System.Collections.Generic;
using System.Linq;
using OneNorth.FieldMigrator.Models;
using OneNorth.FieldMigrator.Pipelines.MigrateVersion;
using DateTime = System.DateTime;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class MigrateLatestPublishableVersionPerLanguage : IMigrateItemPipelineProcessor
    {
        private readonly IMigrateVersionPipeline _migrateVersionPipeline;

        public MigrateLatestPublishableVersionPerLanguage() : this(MigrateVersionPipeline.Instance)
        {
            
        }

        internal protected MigrateLatestPublishableVersionPerLanguage(IMigrateVersionPipeline migrateVersionPipeline)
        {
            _migrateVersionPipeline = migrateVersionPipeline;
        }

        public virtual void Process(MigrateItemPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            // Sort decending by version and Group by language
            var languages = args.Source.Versions
                .OrderBy(x => x.Language)
                .ThenByDescending(x => x.Version)
                .GroupBy(x => x.Language);

            var versionModels = new List<VersionModel>();
            foreach (var language in languages)
            {
                VersionModel firstApproved = null;
                VersionModel firstPublishable = null;

                // Find latest version that is publishable and is approved
                foreach (var version in language)
                {
                    if (version.HasWorkflow && !version.InFinalWorkflowState)
                        continue;

                    if (firstApproved == null)
                        firstApproved = version;

                    if (version.Fields.First(x => x.Name == "__Hide version").Value == "1")
                        continue;

                    var validFrom = version.Fields.First(x => x.Name == "__Valid from").Value;
                    if (!string.IsNullOrEmpty(validFrom) && Sitecore.DateUtil.ParseDateTime(validFrom, DateTime.MinValue) > DateTime.Now)
                        continue;

                    var validTo = version.Fields.First(x => x.Name == "__Valid to").Value;
                    if (!string.IsNullOrEmpty(validFrom) && Sitecore.DateUtil.ParseDateTime(validTo, DateTime.MaxValue) < DateTime.Now)
                        continue;

                    firstPublishable = version;
                    break;
                }

                // If there is no publishable items, that may be because they were switched to non publishable after the fact, in that case take the last approved version.
                if (firstPublishable != null)
                    versionModels.Add(firstPublishable);
                else if (firstApproved != null)
                    versionModels.Add(firstApproved);
            }

            // Update each version
            foreach (var versionModel in versionModels)
            {
                _migrateVersionPipeline.Run(versionModel, args.Item);
            }
        }
    }
}