using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class MigrateFieldPipelineArgs : PipelineArgs
    {
        public Field Field { get; set; }
        public Item Item { get; set; }
        public FieldModel Source { get; set; }
    }
}