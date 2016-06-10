using System.Collections.Generic;
using System.Linq;
using OneNorth.FieldMigrator.Models;
using OneNorth.FieldMigrator.Pipelines.MigrateVersion;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class MigrateLatestApprovedVersionPerLanguage : IMigrateItemPipelineProcessor
    {
        private readonly IMigrateVersionPipeline _migrateVersionPipeline;

        public MigrateLatestApprovedVersionPerLanguage() : this(MigrateVersionPipeline.Instance)
        {
            
        }

        internal protected MigrateLatestApprovedVersionPerLanguage(IMigrateVersionPipeline migrateVersionPipeline)
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
                VersionModel latestVersion = null;
                VersionModel latestApproved = null;

                // Find latest version that is publishable and is approved
                foreach (var version in language)
                {
                    if (latestVersion == null)
                        latestVersion = version;

                    if (version.HasWorkflow && version.WorkflowState == WorkflowState.NonFinal)
                        continue;

                    latestApproved = version;
                    
                    break;
                }
                
                // Take the latest approved version.  If there is no approved version take the latest version.
                if (latestApproved != null)
                    versionModels.Add(latestApproved);
                else if (latestVersion != null)
                {
                    versionModels.Add(latestVersion);
                    Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] Approved version not found: {0}, {1}, {2}", args.Source.Id, latestVersion.Language, args.Source.FullPath(x => x.Parent, x => x.Name)), this);
                }
            }

            // Update each version
            foreach (var versionModel in versionModels)
            {
                _migrateVersionPipeline.Run(versionModel, args.Item);
            }
        }
    }
}