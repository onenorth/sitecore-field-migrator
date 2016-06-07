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
            _hardRockWebServiceProxy.TraverseTree(itemId, TraverseTreeItemAction);
        }

        private void TraverseTreeItemAction(ItemModel itemModel)
        {
            _migrateItemPipeline.Run(itemModel);
        }
    }
}