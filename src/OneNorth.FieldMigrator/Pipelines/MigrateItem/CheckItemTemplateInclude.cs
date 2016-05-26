using System;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class CheckItemTemplateInclude
    {
        public bool IncludeAllDescendants { get; set; }
        public string Name { get; set; }
        public Guid SourceTemplateId { get; set; }
    }
}