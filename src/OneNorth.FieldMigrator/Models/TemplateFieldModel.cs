using System;

namespace OneNorth.FieldMigrator.Models
{
    public class TemplateFieldModel
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}