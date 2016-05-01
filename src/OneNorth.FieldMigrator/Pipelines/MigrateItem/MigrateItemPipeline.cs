using OneNorth.FieldMigrator.Models;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class MigrateItemPipeline : IMigrateItemPipeline
    {
        private static readonly IMigrateItemPipeline _instance = new MigrateItemPipeline();
        public static IMigrateItemPipeline Instance { get { return _instance; } }

        private MigrateItemPipeline()
        {

        }

        public MigrateItemPipelineArgs Run(ItemModel source)
        {
            var args = new MigrateItemPipelineArgs { Source = source };
            CorePipeline.Run("migrateItem", args, "OneNorth.Migrator");
            return args;
        }
    }
}