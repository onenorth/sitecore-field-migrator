using System;
using System.Collections.Generic;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class ResolveFieldOverride
    {
        public Guid SourceFieldId { get; set; }
        public Guid TargetFieldId { get; set; }
        public List<Guid> SourceTemplateIds { get; set; } 
    }
}