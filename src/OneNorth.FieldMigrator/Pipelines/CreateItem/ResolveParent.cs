using System;
using System.Collections.Generic;
using System.Xml;
using Sitecore.Data;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.CreateItem
{
    public class ResolveParent : ICreateItemPipelineProcessor
    {
        private readonly Dictionary<Guid, Guid> _overrides = new Dictionary<Guid, Guid>();
        public virtual Dictionary<Guid, Guid> Overrides { get { return _overrides; } }
        protected virtual void AddOverride(XmlNode node)
        {
            var key = Guid.Parse(XmlUtil.GetAttribute("sourceParentId", node, "{00000000-0000-0000-0000-000000000000}"));
            var value = Guid.Parse(XmlUtil.GetAttribute("targetParentId", node, "{00000000-0000-0000-0000-000000000000}"));

            if (!_overrides.ContainsKey(key))
                _overrides.Add(key, value);
        }

        public virtual void Process(CreateItemPipelineArgs args)
        {
            if (args.Source == null ||
                args.Parent != null)
                return;

            var source = args.Source;

            // Determing the template id to use in the target
            var parentId = (Overrides.ContainsKey(source.ParentId))
                ? Overrides[source.ParentId]
                : source.ParentId;

            var parent = Sitecore.Context.Database.GetItem(new ID(parentId));
            if (parent == null)
            {
                Sitecore.Diagnostics.Log.Error(
                    string.Format("[FieldMigrator] Could not find the parent with id {0} for {1} ({2})",
                        source.ParentId.ToString().ToUpper(),
                        source.Name,
                        source.Id), this);
                return;
            }

            args.Parent = parent;
        }
    }
}