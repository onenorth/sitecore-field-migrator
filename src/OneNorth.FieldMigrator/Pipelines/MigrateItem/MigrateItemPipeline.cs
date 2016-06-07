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
            if (args.Aborted || args.Item == null)
                Sitecore.Diagnostics.Log.Warn(string.Format("[FieldMigrator] Unable to migrate: {0}", source.Name), this);
            else if (args.Item != null)
                Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] Migrated: {0}", args.Item.Paths.FullPath), this);

            return args;
        }
    }
}