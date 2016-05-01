using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class MigrateVersionPipelineArgs : PipelineArgs
    {
        public EditContext EditContext { get; set; }

        public Item Item { get; set; }

        public ItemVersionModel Source { get; set; }
    }
}