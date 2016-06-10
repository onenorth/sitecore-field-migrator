using System.Linq;
using OneNorth.FieldMigrator.Configuration;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class CheckItemTemplate : IMigrateItemPipelineProcessor
    {
        private readonly IFieldMigratorConfiguration _configuration;

        public CheckItemTemplate() : this(FieldMigratorConfiguration.Instance)
        {
            
        }

        internal CheckItemTemplate(IFieldMigratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IncludeMedia { get; set; }

        public virtual void Process(MigrateItemPipelineArgs args)
        {
            if (args.Source == null)
                return;

            var source = args.Source;

            if (_configuration.TemplateExcludes.Any(x => x.SourceTemplateId == source.TemplateId))
            {
                args.AbortPipeline();
                return;
            }

            // If configured to include media, allow all media through
            if (IncludeMedia)
            {
                if (source.IsMediaItem)
                    return;
            }

            // If template is registered, we can process.
            var templateInclude = _configuration.TemplateIncludes.FirstOrDefault(x => x.SourceTemplateId == source.TemplateId);
            if (templateInclude != null)
                return;

            // Check to see if a parent template is set to include all children.
            var parentTemplateIncludes = source.Parents(x => x.Parent)
                .Select(parent => _configuration.TemplateIncludes.FirstOrDefault(x => x.SourceTemplateId == parent.TemplateId))
                .Where(x => x != null);
            if (parentTemplateIncludes.Any(x => x.IncludeAllDescendants))
                return;

            // Skip item
            args.AbortPipeline();
        }
    }
}