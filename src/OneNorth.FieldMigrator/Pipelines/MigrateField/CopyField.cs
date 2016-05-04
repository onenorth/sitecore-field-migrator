
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class CopyField : IMigrateFieldPipelineProcessor
    {
        private readonly List<CopyFieldOverride> _overrides = new List<CopyFieldOverride>();
        public virtual List<CopyFieldOverride> Overrides { get { return _overrides; } }
        protected virtual void AddOverride(XmlNode node)
        {
            var copyFieldOverride = new CopyFieldOverride();
            copyFieldOverride.SourceFieldId = Guid.Parse(XmlUtil.GetAttribute("sourceFieldId", node, "{00000000-0000-0000-0000-000000000000}"));
            copyFieldOverride.TargetFieldId = Guid.Parse(XmlUtil.GetAttribute("targetFieldId", node, "{00000000-0000-0000-0000-000000000000}"));

            var sourceTemplateIds = XmlUtil.GetAttribute("sourceTemplateIds", node, "");
            copyFieldOverride.SourceTemplateIds = ID.ParseArray(sourceTemplateIds).Select(x => x.Guid).ToList();

            _overrides.Add(copyFieldOverride);
        }

        public void Process(MigrateFieldPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            var source = args.Source;
            var item = args.Item;

            var copyFieldOverride = Overrides.FirstOrDefault(x => x.SourceFieldId == source.Id && (!x.SourceTemplateIds.Any() || x.SourceTemplateIds.Contains(source.Version.Item.TemplateId)));

            Field field;
            if (copyFieldOverride != null)
                field = item.Fields[new ID(copyFieldOverride.TargetFieldId)];
            else
                field = item.Fields[source.Name];
            if (field == null)
                return;

            if (source.StandardValue)
                field.Reset();
            else
                field.Value = source.Value;
        }
    }
}