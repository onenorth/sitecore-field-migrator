using System;
using System.Collections.Generic;
using System.Xml;
using Sitecore.Data;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.CreateItem
{
    public class ResolveTemplate : ICreateItemPipelineProcessor
    {
        private readonly Dictionary<Guid, Guid> _overrides = new Dictionary<Guid, Guid>();
        public virtual Dictionary<Guid, Guid> Overrides { get { return _overrides; } }
        protected virtual void AddOverride(XmlNode node)
        {
            var key = Guid.Parse(XmlUtil.GetAttribute("sourceTemplateId", node, "{00000000-0000-0000-0000-000000000000}"));
            var value = Guid.Parse(XmlUtil.GetAttribute("targetTemplateId", node, "{00000000-0000-0000-0000-000000000000}"));

            if (!_overrides.ContainsKey(key))
                _overrides.Add(key, value);
        }

        public virtual void Process(CreateItemPipelineArgs args)
        {
            if (args.Source == null ||
                args.Template != null)
                return;

            var source = args.Source;

            // Determing the template id to use in the target
            var templateId = (Overrides.ContainsKey(source.TemplateId))
                ? Overrides[source.TemplateId]
                : source.TemplateId;

            var template = Sitecore.Context.Database.GetTemplate(new ID(templateId));
            if (template == null)
            {
                Sitecore.Diagnostics.Log.Error(
                    string.Format("[FieldMigrator] Could not find the template with id {0} for {1}",
                        templateId.ToString().ToUpper(),
                        source.FullPath), this);
                args.AbortPipeline();
                return;
            }

            args.Template = template;
        }
    }
}