using System;

namespace OneNorth.FieldMigrator.Configuration
{
    public class RootConfiguration : IRootConfiguration
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}