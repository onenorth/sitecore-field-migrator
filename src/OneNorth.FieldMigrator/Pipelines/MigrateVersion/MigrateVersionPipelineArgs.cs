using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class MigrateVersionPipelineArgs : PipelineArgs
    {
        public Item Item { get; set; }

        public VersionModel Source { get; set; }
    }
}