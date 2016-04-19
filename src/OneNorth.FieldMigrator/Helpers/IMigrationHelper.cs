

using System;

namespace OneNorth.FieldMigrator.Helpers
{
    public interface IMigrationHelper
    {
        void MigrateRoot(Guid itemId);
    }
}
