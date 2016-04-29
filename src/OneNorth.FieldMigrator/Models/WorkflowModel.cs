using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OneNorth.FieldMigrator.Models
{
    public class WorkflowModel
    {
        public Guid Id { get; set; }
        public Guid FinalState { get; set; }
    }
}