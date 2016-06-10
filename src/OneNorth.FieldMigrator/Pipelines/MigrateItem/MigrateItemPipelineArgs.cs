using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class MigrateItemPipelineArgs : PipelineArgs
    {
        public Item Item { get; set; }
        public ItemModel Source { get; set; }
    }
}