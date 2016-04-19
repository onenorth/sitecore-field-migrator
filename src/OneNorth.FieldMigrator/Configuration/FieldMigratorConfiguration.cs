using System;
using System.Collections.Generic;
using System.Xml;
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

        public string SourceDatabase { get; set; }
        public string SourceUserName { get; set; }
        public string SourcePassword { get; set; }
        public string TargetDatabase { get; set; }
        public string TargetUserName { get; set; }


        private readonly List<IRootConfiguration> _roots = new List<IRootConfiguration>();
        public List<IRootConfiguration> Roots { get { return _roots; } }
        protected void AddRoot(XmlNode node)
        {
            _roots.Add(new RootConfiguration
            {
                Name = XmlUtil.GetAttribute("name", node),
                Id = Guid.Parse(XmlUtil.GetAttribute("id", node))
            });
        }

        private readonly List<ITemplateConfiguration> _templates = new List<ITemplateConfiguration>();
        public List<ITemplateConfiguration> Templates { get { return _templates; } }
        protected void AddTemplate(XmlNode node)
        {
            _templates.Add(new TemplateConfiguration
            {
                Name = XmlUtil.GetAttribute("name", node),
                TemplateId = Guid.Parse(XmlUtil.GetAttribute("templateId", node, "{00000000-0000-0000-0000-000000000000}")),
                IncludeAllChildren = bool.Parse(XmlUtil.GetAttribute("includeAllChildren", node, "false")),
                TargetTemplateId = Guid.Parse(XmlUtil.GetAttribute("targetTemplateId", node, "{00000000-0000-0000-0000-000000000000}"))
            });
        }
    }
}