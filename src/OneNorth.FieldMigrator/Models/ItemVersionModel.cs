using System.Collections.Generic;

namespace OneNorth.FieldMigrator.Models
{
    public class ItemVersionModel
    {
        public string Language { get; set; }
        public int Version { get; set; }
        public List<ItemFieldModel> Fields { get; set; }
    }
}