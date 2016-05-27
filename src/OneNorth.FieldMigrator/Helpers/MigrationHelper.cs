using System;
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
            Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] GetItem from source started: {0}", itemId), this);
            var sourceItem = _hardRockWebServiceProxy.GetItem(itemId, true);
            Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] GetItem from source finished: {0}", itemId), this);
            MigrateRoot(sourceItem);
        }

        private void MigrateRoot(ItemModel sourceItem)
        {
            //MigrateItem(sourceItem);
            _migrateItemPipeline.Run(sourceItem);
            foreach (var child in sourceItem.Children)
            {
                MigrateRoot(child);
            }
        }
    }
}