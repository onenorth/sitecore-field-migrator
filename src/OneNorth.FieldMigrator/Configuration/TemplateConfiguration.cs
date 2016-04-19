using System;

namespace OneNorth.FieldMigrator.Configuration
{
    public class TemplateConfiguration : ITemplateConfiguration
    {
        public bool IncludeAllChildren { get; set; }
        public string Name { get; set; }
        public Guid TargetTemplateId { get; set; }
        public Guid TemplateId { get; set; }
    }
}