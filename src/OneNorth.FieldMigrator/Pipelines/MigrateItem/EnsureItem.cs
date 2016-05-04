using System;
using System.Collections.Generic;
using System.Xml;
using OneNorth.FieldMigrator.Pipelines.CreateItem;
using OneNorth.FieldMigrator.Pipelines.MigrateVersion;
using Sitecore.Data;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class EnsureItem : IMigrateItemPipelineProcessor
    {
        private readonly ICreateItemPipeline _createItemPipeline;

        public EnsureItem() : this(CreateItemPipeline.Instance)
        {

        }

        internal protected EnsureItem(ICreateItemPipeline createItemPipeline)
        {
            _createItemPipeline = createItemPipeline;
        }

        private readonly Dictionary<Guid, Guid> _overrides = new Dictionary<Guid, Guid>();
        public virtual Dictionary<Guid, Guid> Overrides { get { return _overrides; } }
        protected virtual void AddOverride(XmlNode node)
        {
            var key = Guid.Parse(XmlUtil.GetAttribute("sourceItemId", node, "{00000000-0000-0000-0000-000000000000}"));
            var value = Guid.Parse(XmlUtil.GetAttribute("targetItemId", node, "{00000000-0000-0000-0000-000000000000}"));

            if (!_overrides.ContainsKey(key))
                _overrides.Add(key, value);
        }

        public virtual void Process(MigrateItemPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item != null)
                return;

            var source = args.Source;

            var id = (Overrides.ContainsKey(source.Id)) ? Overrides[source.Id] : source.Id;
            var itemId = new ID(id);

            args.Item = Sitecore.Context.Database.GetItem(itemId);

            if (args.Item != null)
                return;

            var results = _createItemPipeline.Run(source, itemId);
            args.Item = results.Item;
        }
    }
}