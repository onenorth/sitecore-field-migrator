using OneNorth.FieldMigrator.Models;
using Sitecore.Data;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.CreateItem
{
    public class CreateItemPipeline : ICreateItemPipeline
    {
        private static readonly ICreateItemPipeline _instance = new CreateItemPipeline();
        public static ICreateItemPipeline Instance { get { return _instance; } }

        private CreateItemPipeline()
        {

        }

        public CreateItemPipelineArgs Run(ItemModel source, ID itemId)
        {
            var args = new CreateItemPipelineArgs { Source = source, ItemId = itemId };
            CorePipeline.Run("createItem", args, "OneNorth.Migrator");
            return args;
        }
    }
}