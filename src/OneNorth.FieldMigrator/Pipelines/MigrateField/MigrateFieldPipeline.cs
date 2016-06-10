using System;
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
                CorePipeline.Run("migrateField", args, "OneNorth.Migrator");
                if (args.Field == null)
                    Sitecore.Diagnostics.Log.Warn(string.Format("[FieldMigrator] (MigrateFieldPipeline) Did not migrate: {0} {1}", item.Uri, source.Name), this);
                else
                    Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] (MigrateFieldPipeline) Migrated: {0} {1}", item.Uri, args.Field.Name), this);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] (MigrateFieldPipeline) {0} {1}", item.Uri, source.Name), ex, this);
            }
           
            return args;
        }
    }
}