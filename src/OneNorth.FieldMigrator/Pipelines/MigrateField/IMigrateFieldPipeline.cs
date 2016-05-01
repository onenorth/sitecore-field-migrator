using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public interface IMigrateFieldPipeline
    {
        MigrateFieldPipelineArgs Run(ItemFieldModel source, Item item);
    }
}
