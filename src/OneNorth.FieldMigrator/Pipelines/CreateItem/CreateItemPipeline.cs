using System;
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
            try
            {
                CorePipeline.Run("createItem", args, "OneNorth.Migrator");

                if (args.Item == null)
                    Sitecore.Diagnostics.Log.Warn(string.Format("[FieldMigrator] (CreateItemPipeline) Did not create: {0}", source.FullPath(x => x.Parent, x => x.Name)), this);
                else
                    Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] (CreateItemPipeline) Created: {0}", args.Item.Paths.FullPath), this);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] (CreateItemPipeline) {0}", source.FullPath(x => x.Parent, x => x.Name)), ex, this);
            }
            
            return args;
        }
    }
}