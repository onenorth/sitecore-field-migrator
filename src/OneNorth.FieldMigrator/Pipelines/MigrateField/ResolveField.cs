using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Data;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class ResolveField : IMigrateFieldPipelineProcessor
    {
        private readonly List<ResolveFieldOverride> _overrides = new List<ResolveFieldOverride>();
        public virtual List<ResolveFieldOverride> Overrides { get { return _overrides; } }
        protected virtual void AddOverride(XmlNode node)
        {
            var resolveFieldOverride = new ResolveFieldOverride();
            resolveFieldOverride.SourceFieldId = Guid.Parse(XmlUtil.GetAttribute("sourceFieldId", node, "{00000000-0000-0000-0000-000000000000}"));
            resolveFieldOverride.TargetFieldId = Guid.Parse(XmlUtil.GetAttribute("targetFieldId", node, "{00000000-0000-0000-0000-000000000000}"));

            var sourceTemplateIds = XmlUtil.GetAttribute("sourceTemplateIds", node, "");
            resolveFieldOverride.SourceTemplateIds = ID.ParseArray(sourceTemplateIds).Select(x => x.Guid).ToList();

            _overrides.Add(resolveFieldOverride);
        }

        public void Process(MigrateFieldPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null ||
                args.Field != null)
                return;

            var source = args.Source;
            var item = args.Item;

            var fieldOverride = Overrides.FirstOrDefault(x => x.SourceFieldId == source.Id && (!x.SourceTemplateIds.Any() || x.SourceTemplateIds.Contains(source.Version.Item.TemplateId)));

            if (fieldOverride != null)
                args.Field = item.Fields[new ID(fieldOverride.TargetFieldId)];
            else
                args.Field = item.Fields[source.Name];
        }
    }
}