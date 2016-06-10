using System;
using System.Diagnostics;
using OneNorth.FieldMigrator.Models;
using Sitecore.Data.Items;
using Sitecore.Pipelines;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class MigrateVersionPipeline : IMigrateVersionPipeline
    {
        private static readonly IMigrateVersionPipeline _instance = new MigrateVersionPipeline();
        public static IMigrateVersionPipeline Instance { get { return _instance; } }

        private MigrateVersionPipeline()
        {

        }

        public MigrateVersionPipelineArgs Run(VersionModel source, Item item)
        {
            var args = new MigrateVersionPipelineArgs { Source = source, Item = item };
            try
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                CorePipeline.Run("migrateVersion", args, "OneNorth.Migrator");

                stopWatch.Stop();

                Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] (MigrateVersionPipeline) Migrated: {0} in {1}", args.Item.Uri, stopWatch.Elapsed), this);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] (MigrateVersionPipeline) {0}", args.Item.Uri), ex, this);
            }
            
            return args;
        }
    }
}