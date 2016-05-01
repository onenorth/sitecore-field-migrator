using System;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class CheckTemplateInclude
    {
        public bool IncludeAllChildren { get; set; }
        public string Name { get; set; }
        public Guid TemplateId { get; set; }
    }
}