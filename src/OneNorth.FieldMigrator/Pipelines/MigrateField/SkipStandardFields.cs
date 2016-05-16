using System;
using System.Collections.Generic;
using System.Linq;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class SkipStandardFields : IMigrateFieldPipelineProcessor
    {
        private readonly List<string> _exceptions = new List<string>();
        public virtual List<string> Exceptions { get { return _exceptions; } }
        protected virtual void AddException(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                return;

            _exceptions.Add(field);
        }

        public virtual void Process(MigrateFieldPipelineArgs args)
        {
            if (args.Source == null)
                return;

            var source = args.Source;

            if (string.IsNullOrEmpty(source.Name) ||
                (source.Name.StartsWith("__") &&
                 !Exceptions.Contains(source.Name, StringComparer.OrdinalIgnoreCase)))
            {
                args.AbortPipeline();
            }
        }
    }
}