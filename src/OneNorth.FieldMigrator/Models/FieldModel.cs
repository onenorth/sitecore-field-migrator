using System;

namespace OneNorth.FieldMigrator.Models
{
    public class FieldModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool Shared { get; set; }
        public bool StandardValue { get; set; }
        public string Type { get; set; }
        public bool Unversioned { get; set; }
        public string Value { get; set; }
        public VersionModel Version { get; set; }
    }
}