using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class MigrateFieldPipelineArgs : PipelineArgs
    {
        public Item Item { get; set; }

        public ItemFieldModel Source { get; set; }
    }
}