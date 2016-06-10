using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class SkipAlreadyProcessed : IMigrateItemPipelineProcessor
    {
        private readonly HashSet<Guid> _processed = new HashSet<Guid>(); 

        public virtual void Process(MigrateItemPipelineArgs args)
        {
            if (args.Source == null)
                return;

            if (_processed.Contains(args.Source.Id))
            {
                args.AbortPipeline();
                return;
            }

            _processed.Add(args.Source.Id);
        }
    }
}