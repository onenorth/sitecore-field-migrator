using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.CreateItem
{
    public class ResolveTemplate : ICreateItemPipelineProcessor
    {
        public bool MediaManagerDecidesMediaItemTemplates { get; set; }

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
            
            TemplateItem template;
            if (MediaManagerDecidesMediaItemTemplates && source.IsMediaItem)
            {
                // Grab extension
                var extensionField =
                    source.Versions.SelectMany(x => x.Fields)
                        .FirstOrDefault(x => string.Equals(x.Name, "Extension", StringComparison.OrdinalIgnoreCase));
                if (extensionField == null)
                    return;

                var extension = extensionField.Value;
                var versioned = Sitecore.Configuration.Settings.Media.UploadAsVersionableByDefault;

                var templateFullName = MediaManager.Config.GetTemplate(extension, versioned);
                template = Context.Database.GetTemplate(templateFullName);
            }
            else
            {
                var templateId = Overrides.ContainsKey(source.TemplateId) ? Overrides[source.TemplateId] : source.TemplateId;
                template = Context.Database.GetTemplate(new ID(templateId));
            }
 
            if (template == null)
            {
                Sitecore.Diagnostics.Log.Warn(
                    string.Format("[FieldMigrator] Could not find the template for {0} ({1})",
                        source.FullPath(x => x.Parent, x => x.Name),
                        source.Id), this);
                return;
            }

            args.Template = template;
        }
    }
}