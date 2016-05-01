using OneNorth.FieldMigrator.Models;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.CreateItem
{
    public class CreateItemPipelineArgs : PipelineArgs
    {
        public Item Item { get; set; }
        public ID ItemId { get; set; }
        public Item Parent { get; set; }
        public ItemModel Source { get; set; }
        public TemplateItem Template { get; set; }
    }
}