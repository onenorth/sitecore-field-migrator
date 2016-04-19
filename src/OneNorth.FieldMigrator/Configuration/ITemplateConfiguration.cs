using System;

namespace OneNorth.FieldMigrator.Configuration
{
    public interface ITemplateConfiguration
    {
        bool IncludeAllChildren { get; set; }
        string Name { get; }
        Guid TargetTemplateId { get; }
        Guid TemplateId { get; }
    }
}
