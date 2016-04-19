using System;
using System.Collections.Generic;

namespace OneNorth.FieldMigrator.Models
{
    public class TemplateModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<TemplateModel> BaseTemplates { get; set; } 
        public List<TemplateFieldModel> Fields { get; set; }
    }
}