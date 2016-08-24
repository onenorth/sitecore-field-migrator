using System;

namespace OneNorth.FieldMigrator.Configuration
{
    public interface IRootConfiguration
    {
        Guid SourceItemId { get; }
    }
}
