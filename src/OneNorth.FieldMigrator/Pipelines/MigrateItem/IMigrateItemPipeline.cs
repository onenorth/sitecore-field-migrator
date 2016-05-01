using OneNorth.FieldMigrator.Models;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public interface IMigrateItemPipeline
    {
        MigrateItemPipelineArgs Run(ItemModel source);
    }
}
