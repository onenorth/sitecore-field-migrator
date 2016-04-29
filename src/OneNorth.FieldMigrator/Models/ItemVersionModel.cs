﻿using System.Collections.Generic;

namespace OneNorth.FieldMigrator.Models
{
    public class ItemVersionModel
    {
        public List<ItemFieldModel> Fields { get; set; }
        public bool HasWorkflow { get; set; }
        public bool InFinalWorkflowState { get; set; }
        public string Language { get; set; }
        public int Version { get; set; }
        
    }
}