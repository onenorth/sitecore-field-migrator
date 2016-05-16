using System;
using System.Collections.Generic;
using System.Linq;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class SkipFields : IMigrateFieldPipelineProcessor
    {
        private readonly List<string> _fields = new List<string>();
        public virtual List<string> Fields { get { return _fields; } }
        protected virtual void AddField(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                return;

            Guid guid;
            if (Guid.TryParse(field, out guid))
                _fields.Add(guid.ToString());
            else
                _fields.Add(field);
        }

        public virtual void Process(MigrateFieldPipelineArgs args)
        {
            if (args.Source == null)
                return;

            var source = args.Source;

            if (Fields.Contains(source.Name) || Fields.Contains(source.Id.ToString()))
                args.AbortPipeline();
        }
    }
}