using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class CheckItemTemplate : IMigrateItemPipelineProcessor
    {
        private readonly Guid _mediaLibraryId = new Guid("{3D6658D8-A0BF-4E75-B3E2-D050FABCF4E1}");

        private readonly List<CheckItemTemplateInclude> _includes = new List<CheckItemTemplateInclude>();
        public List<CheckItemTemplateInclude> Includes { get { return _includes; } }
        protected void AddInclude(XmlNode node)
        {
            _includes.Add(new CheckItemTemplateInclude
            {
                Name = XmlUtil.GetAttribute("name", node),
                SourceTemplateId = Guid.Parse(XmlUtil.GetAttribute("sourceTemplateId", node, "{00000000-0000-0000-0000-000000000000}")),
                IncludeAllDescendants = bool.Parse(XmlUtil.GetAttribute("includeAllDescendants", node, "false")),
            });
        }

        private readonly List<CheckItemTemplateExclude> _excludes = new List<CheckItemTemplateExclude>();
        public List<CheckItemTemplateExclude> Excludes { get { return _excludes; } }
        protected void AddExclude(XmlNode node)
        {
            _excludes.Add(new CheckItemTemplateExclude
            {
                Name = XmlUtil.GetAttribute("name", node),
                TemplateId = Guid.Parse(XmlUtil.GetAttribute("sourceTemplateId", node, "{00000000-0000-0000-0000-000000000000}"))
            });
        }

        public bool IncludeMedia { get; set; }

        public virtual void Process(MigrateItemPipelineArgs args)
        {
            if (args.Source == null)
                return;

            var source = args.Source;

            if (Excludes.Any(x => x.TemplateId == source.TemplateId))
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
            var templateConfiguration = Includes.FirstOrDefault(x => x.SourceTemplateId == source.TemplateId);
            if (templateConfiguration != null)
                return;

            // Check to see if a parent template is set to include all children.
            if (source.RelativePath != null)
            {
                var parentTemplateConfigurations = source.RelativePath
                 .Select(parent => Includes.FirstOrDefault(x => x.SourceTemplateId == parent.TemplateId))
                 .Where(x => x != null);

                if (parentTemplateConfigurations.Any(x => x.IncludeAllDescendants))
                    return;
            }
            
            // Skip
            args.AbortPipeline();
        }
    }
}