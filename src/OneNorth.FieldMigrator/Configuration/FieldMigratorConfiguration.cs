using System;
using System.Collections.Generic;
using System.Xml;
using OneNorth.FieldMigrator.Pipelines.MigrateItem;
using Sitecore.Configuration;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Configuration
{
    public class FieldMigratorConfiguration : IFieldMigratorConfiguration
    {
        private static readonly object SyncRoot = new object();

        private static volatile IFieldMigratorConfiguration _instance;
        public static IFieldMigratorConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                            _instance = Factory.CreateObject("fieldmigrator", true) as IFieldMigratorConfiguration;
                    }
                }
                return _instance;
            }
        }

        public string SourceEndpointAddress { get; set; }
        public string SourceDatabase { get; set; }
        public string SourceUserName { get; set; }
        public string SourcePassword { get; set; }
        public string TargetDatabase { get; set; }
        public string TargetUserName { get; set; }

        // MediaItemTemplateIds
        private readonly List<Guid> _mediaItemTemplateIds = new List<Guid>();
        public List<Guid> MediaItemTemplateIds { get { return _mediaItemTemplateIds; } }
        protected void AddMediaItemTemplateId(string value)
        {
            _mediaItemTemplateIds.Add(Guid.Parse(value));
        }

        // Roots
        private readonly List<IRootConfiguration> _roots = new List<IRootConfiguration>();
        public List<IRootConfiguration> Roots { get { return _roots; } }
        protected void AddRoot(XmlNode node)
        {
            _roots.Add(new RootConfiguration
            {
                SourceItemId = Guid.Parse(XmlUtil.GetAttribute("sourceItemId", node))
            });
        }

        // TemplateIncludes
        private readonly List<TemplateInclude> _templateIncludes = new List<TemplateInclude>();
        public List<TemplateInclude> TemplateIncludes { get { return _templateIncludes; } }
        protected void AddTemplateInclude(XmlNode node)
        {
            _templateIncludes.Add(new TemplateInclude
            {
                SourceTemplateId = Guid.Parse(XmlUtil.GetAttribute("sourceTemplateId", node, "{00000000-0000-0000-0000-000000000000}")),
                IncludeAllDescendants = bool.Parse(XmlUtil.GetAttribute("includeAllDescendants", node, "false")),
            });
        }

        // TemplateExcludes
        private readonly List<TemplateExclude> _templateExcludes = new List<TemplateExclude>();
        public List<TemplateExclude> TemplateExcludes { get { return _templateExcludes; } }
        protected void AddTemplateExclude(XmlNode node)
        {
            _templateExcludes.Add(new TemplateExclude
            {
                SourceTemplateId = Guid.Parse(XmlUtil.GetAttribute("sourceTemplateId", node, "{00000000-0000-0000-0000-000000000000}"))
            });
        }
    }
}