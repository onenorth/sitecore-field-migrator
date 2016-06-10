using System;
using System.Diagnostics;
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
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                CorePipeline.Run("createItem", args, "OneNorth.Migrator");

                stopWatch.Stop();

                if (args.Item == null)
                    Sitecore.Diagnostics.Log.Warn(string.Format("[FieldMigrator] (CreateItemPipeline) Did not create: {0} {1} in {2}", source.Id, source.FullPath(x => x.Parent, x => x.Name), stopWatch.Elapsed), this);
                else
                    Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] (CreateItemPipeline) Created: {0} {1} in {2}", args.Item.ID, args.Item.Paths.FullPath, stopWatch.Elapsed), this);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] (CreateItemPipeline) {0} {1}", source.Id, source.FullPath(x => x.Parent, x => x.Name)), ex, this);
            }
            
            return args;
        }
    }
}