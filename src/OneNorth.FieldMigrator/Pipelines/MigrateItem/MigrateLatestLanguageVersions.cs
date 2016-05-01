using System.Linq;
using OneNorth.FieldMigrator.Pipelines.MigrateVersion;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class MigrateLatestLanguageVersions : IMigrateItemPipelineProcessor
    {
        private readonly IMigrateVersionPipeline _migrateVersionPipeline;

        public MigrateLatestLanguageVersions() : this(MigrateVersionPipeline.Instance)
        {
            
        }

        internal protected MigrateLatestLanguageVersions(IMigrateVersionPipeline migrateVersionPipeline)
        {
            _migrateVersionPipeline = migrateVersionPipeline;
        }

        public virtual void Process(MigrateItemPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            // Get the most recent version by language
            var webServiceVersions = args.Source.Versions
                .OrderBy(x => x.Language)
                .ThenByDescending(x => x.Version)
                .GroupBy(x => x.Language)
                .Select(x => x.First());

            // Update each version
            foreach (var itemVersionModel in webServiceVersions)
            {
                _migrateVersionPipeline.Run(itemVersionModel, args.Item);
            }
        }
    }
}