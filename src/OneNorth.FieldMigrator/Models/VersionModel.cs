﻿using System.Collections.Generic;

namespace OneNorth.FieldMigrator.Models
{
    public class VersionModel
    {
        public List<FieldModel> Fields { get; set; }
        public bool HasWorkflow { get; set; }
        public bool InFinalWorkflowState { get; set; }
        public ItemModel Item { get; set; }
        public string Language { get; set; }
        public List<FolderModel> Path { get; set; } 
        public int Version { get; set; }
        
    }
}