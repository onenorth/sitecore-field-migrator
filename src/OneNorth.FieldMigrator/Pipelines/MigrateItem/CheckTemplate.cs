using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class CheckTemplate : IMigrateItemPipelineProcessor
    {
        private readonly List<CheckTemplateInclude> _includes = new List<CheckTemplateInclude>();
        public List<CheckTemplateInclude> Includes { get { return _includes; } }
        protected void AddInclude(XmlNode node)
        {
            _includes.Add(new CheckTemplateInclude
            {
                Name = XmlUtil.GetAttribute("name", node),
                TemplateId = Guid.Parse(XmlUtil.GetAttribute("templateId", node, "{00000000-0000-0000-0000-000000000000}")),
                IncludeAllChildren = bool.Parse(XmlUtil.GetAttribute("includeAllChildren", node, "false")),
            });
        }

        private readonly List<CheckTemplateExclude> _excludes = new List<CheckTemplateExclude>();
        public List<CheckTemplateExclude> Excludes { get { return _excludes; } }
        protected void AddExclude(XmlNode node)
        {
            _excludes.Add(new CheckTemplateExclude
            {
                Name = XmlUtil.GetAttribute("name", node),
                TemplateId = Guid.Parse(XmlUtil.GetAttribute("templateId", node, "{00000000-0000-0000-0000-000000000000}"))
            });
        }

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

            var templateConfiguration = Includes.FirstOrDefault(x => x.TemplateId == source.TemplateId);
            var parentTemplateConfigurations = source.Parents(x => x.Parent)
                .Select(parent => Includes.FirstOrDefault(x => x.TemplateId == parent.TemplateId))
                .Where(x => x != null);

            // If template not registered in config, skip; unless parent template indicates to include all children.
            if (templateConfiguration == null && !parentTemplateConfigurations.Any(x => x.IncludeAllChildren))
                args.AbortPipeline();
        }
    }
}