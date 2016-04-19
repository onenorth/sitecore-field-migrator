using System;
using System.Collections.Generic;
using OneNorth.FieldMigrator.Helpers;

namespace OneNorth.FieldMigrator.Models
{
    public class ItemModel
    {
        public string Database { get; set; }
        public bool HasChildren { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ItemModel Parent { get; set; }
        public Guid ParentId { get; set; }
        public int SortOrder { get; set; }
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; }
        
        public List<ItemModel> Children { get; set; } 

        public List<ItemVersionModel> Versions { get; set; }
    }
}