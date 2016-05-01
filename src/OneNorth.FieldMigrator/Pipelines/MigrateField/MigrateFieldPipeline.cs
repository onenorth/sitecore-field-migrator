using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class MigrateFieldPipeline : IMigrateFieldPipeline
    {
        private static readonly IMigrateFieldPipeline _instance = new MigrateFieldPipeline();
        public static IMigrateFieldPipeline Instance { get { return _instance; } }

        private MigrateFieldPipeline()
        {

        }

        public MigrateFieldPipelineArgs Run(ItemFieldModel source, Item item)
        {
            var args = new MigrateFieldPipelineArgs { Source = source, Item = item };
            CorePipeline.Run("migrateField", args, "OneNorth.Migrator");
            return args;
        }
    }
}