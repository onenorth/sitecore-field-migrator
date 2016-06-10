using System;
using System.Diagnostics;
using OneNorth.FieldMigrator.Models;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class MigrateItemPipeline : IMigrateItemPipeline
    {
        private static readonly IMigrateItemPipeline _instance = new MigrateItemPipeline();
        public static IMigrateItemPipeline Instance { get { return _instance; } }

        private MigrateItemPipeline()
        {

        }

        public MigrateItemPipelineArgs Run(ItemModel source)
        {
            var args = new MigrateItemPipelineArgs { Source = source };
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                CorePipeline.Run("migrateItem", args, "OneNorth.Migrator");

                stopWatch.Stop();

                if (args.Item == null)
                    Sitecore.Diagnostics.Log.Warn(string.Format("[FieldMigrator] (MigrateItemPipeline) Did not migrate: {0} {1} in {2}", source.Id, source.FullPath(x => x.Parent, x => x.Name), stopWatch.Elapsed), this);
                else
                    Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] (MigrateItemPipeline) Migrated: {0} {1} in {2}", args.Item.ID, args.Item.Paths.FullPath, stopWatch.Elapsed), this);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] (MigrateItemPipeline) {0} {1}", source.Id, source.FullPath(x => x.Parent, x => x.Name)), ex, this);
            }
            return args;
        }
    }
}