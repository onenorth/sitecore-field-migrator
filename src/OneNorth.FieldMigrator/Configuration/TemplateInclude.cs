using System;

namespace OneNorth.FieldMigrator.Configuration
{
    public class TemplateInclude
    {
        public bool IncludeAllDescendants { get; set; }
        public Guid SourceTemplateId { get; set; }
    }
}