using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using OneNorth.FieldMigrator.Configuration;
using OneNorth.FieldMigrator.HardRockWebService2;
using OneNorth.FieldMigrator.Models;
using Sitecore.Data;

namespace OneNorth.FieldMigrator.Helpers
{
    public class HardRockWebServiceProxy : IHardRockWebServiceProxy
    {
        private static readonly IHardRockWebServiceProxy _instance = new HardRockWebServiceProxy();

        public static IHardRockWebServiceProxy Instance
        {
            get { return _instance; }
        }

        private readonly IFieldMigratorConfiguration _configuration;
        private readonly SitecoreWebService2SoapClient _service;
        private readonly ConcurrentDictionary<Guid, TemplateModel> _templates; 

        private HardRockWebServiceProxy() : this(FieldMigratorConfiguration.Instance)
        {

        }

        internal HardRockWebServiceProxy(IFieldMigratorConfiguration configuration)
        {
            _configuration = configuration;
            _service = new SitecoreWebService2SoapClient();
            _templates = new ConcurrentDictionary<Guid, TemplateModel>();
        }

        public IEnumerable<ChildModel> GetChildren(Guid parentId)
        {
            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            var results = _service.GetChildren(parentId.ToString().ToUpper(), _configuration.SourceDatabase, credentials);
            var childElements = results.XPathSelectElements("//item");

            var children = new List<ChildModel>();
            foreach (var childElement in childElements)
            {
                var child = new ChildModel
                {
                    HasChildren = int.Parse(childElement.Attribute("haschildren").Value) == 1,
                    Id = Guid.Parse(childElement.Attribute("id").Value),
                    Name = childElement.Value,
                    ParentId = parentId,
                    TemplateId = Guid.Parse(childElement.Attribute("templateid").Value)
                };

                children.Add(child);
            }

            return children;
        }

        public ItemModel GetItem(Guid id, bool deep = true)
        {
            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            var results = _service.GetXML(id.ToString().ToUpper(), deep, _configuration.SourceDatabase, credentials);
            var itemElement = results.XPathSelectElement("//item");
            if (itemElement == null)
                return null;

            return GetItem(itemElement);
        }

        private ItemModel GetItem(XElement itemElement, ItemModel parent = null)
        {
            if (itemElement == null || itemElement.Name != "item")
                return null;

            var webServiceItem = new ItemModel
            {
                Id = Guid.Parse(itemElement.Attribute("id").Value),
                Name = itemElement.Attribute("name").Value,
                Parent = parent,
                ParentId = Guid.Parse(itemElement.Attribute("parentid").Value),
                SortOrder = int.Parse(itemElement.Attribute("sortorder").Value),
                TemplateId = Guid.Parse(itemElement.Attribute("tid").Value),
                TemplateName = itemElement.Attribute("template").Value
            };

            var templateFields = GetAllTemplateFields(webServiceItem.TemplateId);

            // Gather the versions
            var childVersionElements = itemElement.Elements("version");
            webServiceItem.Versions = childVersionElements.Select(x => GetVersion(x, templateFields)).ToList();

            // Gather the children
            var childItemElements = itemElement.Elements("item");
            webServiceItem.Children = childItemElements.Select(x => GetItem(x, webServiceItem)).ToList();

            return webServiceItem;
        }

        private ItemVersionModel GetVersion(XElement versionElement, List<TemplateFieldModel> templateFields)
        {
            if (versionElement == null || versionElement.Name != "version")
                return null;

            var webServiceVersion = new ItemVersionModel
            {
                Fields = new List<ItemFieldModel>(),
                Language = versionElement.Attribute("language").Value,
                Version = int.Parse(versionElement.Attribute("version").Value)
            };

            // Grab fields set on the item
            var fieldsElement = versionElement.Element("fields");
            if (fieldsElement != null)
                webServiceVersion.Fields.AddRange(fieldsElement.Elements().Select(GetField));

            // Add the remaining fields from the template and enrich the fields from the item.
            foreach (var templateField in templateFields)
            {
                var itemField = webServiceVersion.Fields.FirstOrDefault(x => x.Id == templateField.Id);
                if (itemField != null)
                    itemField.Name = templateField.Name; // Enrich with the name.
                else
                {
                    webServiceVersion.Fields.Add(new ItemFieldModel
                    {
                        Id = templateField.Id,
                        Key = templateField.Key,
                        Name = templateField.Name,
                        Type = templateField.Type,
                        Value = null
                    });
                }
            }

            return webServiceVersion;
        }

        private ItemFieldModel GetField(XElement fieldElement)
        {
            if (fieldElement == null || fieldElement.Name != "field")
                return null;

            var content = fieldElement.Element("content");
            var value = (content != null) ? content.Value : "";
            var webServiceField = new ItemFieldModel
            {
                Id = Guid.Parse(fieldElement.Attribute("tfid").Value),
                Key = fieldElement.Attribute("key").Value,
                Type = fieldElement.Attribute("type").Value,
                Value = value
            };

            return webServiceField;
        }

        private List<TemplateFieldModel> GetAllTemplateFields(Guid templateId)
        {
            var template = GetTemplate(templateId);
            var templates = template.Flatten(x => x.BaseTemplates);
            var templateFields = templates
                .SelectMany(x => x.Fields)
                .GroupBy(x => x.Id)
                .Select(x => x.FirstOrDefault())
                .ToList();
            return templateFields;
        }

        private TemplateModel GetTemplate(Guid templateId)
        {
            return _templates.GetOrAdd(templateId, GetTemplateFactory);
        }

        private TemplateModel GetTemplateFactory(Guid templateId)
        {
            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            var results = _service.GetXML(templateId.ToString().ToUpper(), true, _configuration.SourceDatabase, credentials);

            var templateItemElement = results.XPathSelectElement("//item");
            if (templateItemElement == null)
                return null;

            return GetTemplateItem(templateItemElement);
        }

        private TemplateModel GetTemplateItem(XElement templateItemElement)
        {
            if (templateItemElement == null || templateItemElement.Name != "item")
                return null;

            var templateModel = new TemplateModel
            {
                Id = Guid.Parse(templateItemElement.Attribute("id").Value),
                Name = templateItemElement.Attribute("name").Value,
            };

            // Skip the standard template
            if (templateModel.Name == "Standard template")
                return null;

            // Get Base Templates
            templateModel.BaseTemplates = GetBaseTemplates(templateItemElement);

            // Get Fields
            var fieldItemElements = templateItemElement.Descendants("item").Where(x => x.Attribute("template").Value == "template field");
            templateModel.Fields = fieldItemElements.Select(GetTemplateField).ToList();

            return templateModel;
        }

        private List<TemplateModel> GetBaseTemplates(XElement templateItemElement)
        {
            var baseTemplateFieldElement = templateItemElement.Descendants("field").FirstOrDefault(x => x.Attribute("key").Value == "__base template");
            if (baseTemplateFieldElement == null)
                return new List<TemplateModel>();

            var content = baseTemplateFieldElement.Element("content");
            var baseTemplateString = (content != null) ? content.Value : "";
            if (string.IsNullOrEmpty(baseTemplateString))
                return new List<TemplateModel>();

            var baseTemplateIds = ID.ParseArray(baseTemplateString).Select(x => x.Guid);
            return baseTemplateIds.Select(GetTemplate).Where(x => x != null).ToList();
        }

        private TemplateFieldModel GetTemplateField(XElement fieldItemElement)
        {
            if (fieldItemElement == null || fieldItemElement.Name != "item")
                return null;

            var templateFieldModel = new TemplateFieldModel
            {
                Id = Guid.Parse(fieldItemElement.Attribute("id").Value),
                Key = fieldItemElement.Attribute("key").Value,
                Name = fieldItemElement.Attribute("name").Value
            };

            var baseTemplateFieldElement = fieldItemElement.Descendants("field").FirstOrDefault(x => x.Attribute("key").Value == "type");
            if (baseTemplateFieldElement != null)
            {
                var content = baseTemplateFieldElement.Element("content");
                templateFieldModel.Type = (content != null) ? content.Value : "";
            }

            return templateFieldModel;
        }
    }
}