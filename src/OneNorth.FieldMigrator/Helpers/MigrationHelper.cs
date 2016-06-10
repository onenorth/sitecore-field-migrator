using System;
using System.Diagnostics;
using OneNorth.FieldMigrator.Models;
using OneNorth.FieldMigrator.Pipelines.MigrateItem;

namespace OneNorth.FieldMigrator.Helpers
{
    public class MigrationHelper : IMigrationHelper
    {
        private static readonly IMigrationHelper _instance = new MigrationHelper();

        public static IMigrationHelper Instance
        {
            get { return _instance; }
        }

        private readonly IHardRockWebServiceProxy _hardRockWebServiceProxy;
        private readonly IMigrateItemPipeline _migrateItemPipeline;

        private MigrationHelper() : this(
            HardRockWebServiceProxy.Instance,
            MigrateItemPipeline.Instance)
        {
            
        }

        internal MigrationHelper(
            IHardRockWebServiceProxy hardRockWebServiceProxy,
            IMigrateItemPipeline migrateItemPipeline)
        {
            _hardRockWebServiceProxy = hardRockWebServiceProxy;
            _migrateItemPipeline = migrateItemPipeline;
        }

        public void MigrateRoot(Guid itemId)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            _hardRockWebServiceProxy.TraverseTree(itemId, TraverseTreeItemAction);

            stopWatch.Stop();

            Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] Migrated Root {0} in {1}", itemId, stopWatch.Elapsed), this);
        }

        private void TraverseTreeItemAction(ItemModel itemModel)
        {
            _migrateItemPipeline.Run(itemModel);
        }
    }
}