using System;

namespace OneNorth.FieldMigrator.Configuration
{
    public interface IRootConfiguration
    {
        Guid Id { get; }
        string Name { get; }
    }
}
