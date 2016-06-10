using System;
using System.Diagnostics;
using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class MigrateFieldPipeline : IMigrateFieldPipeline
    {
        private static readonly IMigrateFieldPipeline _instance = new MigrateFieldPipeline();
        public static IMigrateFieldPipeline Instance { get { return _instance; } }

        private MigrateFieldPipeline()
        {

        }

        public MigrateFieldPipelineArgs Run(FieldModel source, Item item)
        {
            var args = new MigrateFieldPipelineArgs { Source = source, Item = item };
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                CorePipeline.Run("migrateField", args, "OneNorth.Migrator");

                stopWatch.Stop();

                if (args.Field == null)
                    Sitecore.Diagnostics.Log.Debug(string.Format("[FieldMigrator] (MigrateFieldPipeline) Did not migrate: {0} {1} in {2}", args.Item.Uri, source.Name, stopWatch.Elapsed), this);
                else
                    Sitecore.Diagnostics.Log.Debug(string.Format("[FieldMigrator] (MigrateFieldPipeline) Migrated: {0} {1} in {2}", args.Item.Uri, args.Field.Name, stopWatch.Elapsed), this);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] (MigrateFieldPipeline) {0} {1}", args.Item.Uri, source.Name), ex, this);
            }
           
            return args;
        }
    }
}