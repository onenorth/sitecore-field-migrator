using System;

namespace OneNorth.FieldMigrator.Configuration
{
    public class RootConfiguration : IRootConfiguration
    {
        public Guid SourceItemId { get; set; }
    }
}