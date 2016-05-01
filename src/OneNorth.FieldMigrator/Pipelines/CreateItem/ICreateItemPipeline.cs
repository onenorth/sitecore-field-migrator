using OneNorth.FieldMigrator.Models;
using Sitecore.Data;

namespace OneNorth.FieldMigrator.Pipelines.CreateItem
{
    public interface ICreateItemPipeline
    {
        CreateItemPipelineArgs Run(ItemModel source, ID itemId);
    }
}
