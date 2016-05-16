using System;

namespace OneNorth.FieldMigrator.Models
{
    public class ChildModel
    {
        public bool HasChildren { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ParentId { get; set; }
        public Guid TemplateId { get; set; }
    }
}