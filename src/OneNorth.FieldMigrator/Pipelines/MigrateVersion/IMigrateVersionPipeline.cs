using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public interface IMigrateVersionPipeline
    {
        MigrateVersionPipelineArgs Run(ItemVersionModel source, Item item);
    }
}