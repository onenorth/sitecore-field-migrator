﻿using System;
using System.Collections.Generic;

namespace OneNorth.FieldMigrator.Models
{
    public class ItemModel
    {
        public Guid Id { get; set; }
        public bool IsMediaItem { get; set; }
        public string Name { get; set; }
        public ItemModel Parent { get; set; }
        public Guid ParentId { get; set; }
        public int SortOrder { get; set; }
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; }

        public List<ItemModel> Children { get; set; }

        public List<VersionModel> Versions { get; set; }
    }
}