using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class MigrateVersionPipeline : IMigrateVersionPipeline
    {
        private static readonly IMigrateVersionPipeline _instance = new MigrateVersionPipeline();
        public static IMigrateVersionPipeline Instance { get { return _instance; } }

        private MigrateVersionPipeline()
        {

        }

        public MigrateVersionPipelineArgs Run(VersionModel source, Item item)
        {
            var args = new MigrateVersionPipelineArgs { Source = source, Item = item };
            CorePipeline.Run("migrateVersion", args, "OneNorth.Migrator");
            return args;
        }
    }
}