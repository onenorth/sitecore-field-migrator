using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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
        private readonly ConcurrentDictionary<Guid, WorkflowModel> _workFlows;

        private HardRockWebServiceProxy() : this(FieldMigratorConfiguration.Instance)
        {

        }

        internal HardRockWebServiceProxy(IFieldMigratorConfiguration configuration)
        {
            _configuration = configuration;
            _workFlows = new ConcurrentDictionary<Guid, WorkflowModel>();

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None) {MaxReceivedMessageSize = 20000000};
            var remoteAddress = new EndpointAddress(_configuration.SourceEndpointAddress);
            _service = new SitecoreWebService2SoapClient(binding, remoteAddress);
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

            var workflow = GetWorkflow(new Guid("{048E8BB8-1066-484D-B345-BA18264531A2}"));
            //var workflowResults = _service.GetXML("{048E8BB8-1066-484D-B345-BA18264531A2}", deep, _configuration.SourceDatabase, credentials);

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

            var itemModel = new ItemModel
            {
                Id = Guid.Parse(itemElement.Attribute("id").Value),
                Name = itemElement.Attribute("name").Value,
                Parent = parent,
                ParentId = Guid.Parse(itemElement.Attribute("parentid").Value),
                SortOrder = int.Parse(itemElement.Attribute("sortorder").Value),
                TemplateId = Guid.Parse(itemElement.Attribute("tid").Value),
                TemplateName = itemElement.Attribute("template").Value
            };

            // Gather the latest versions
            var childVersionElements = itemElement.Elements("version");
            itemModel.Versions = childVersionElements.Select(GetVersion)
                .OrderBy(x => x.Language)
                .ThenByDescending(x => x.Version)
                .GroupBy(x => x.Language)
                .Select(x => x.First())
                .ToList();

            // Populate the fields
            foreach(var version in itemModel.Versions)
                PopulateFields(itemModel.Id, version);

            // Gather the children
            var childItemElements = itemElement.Elements("item");
            itemModel.Children = childItemElements.Select(x => GetItem(x, itemModel)).ToList();

            return itemModel;
        }

        private ItemVersionModel GetVersion(XElement versionElement)
        {
            if (versionElement == null || versionElement.Name != "version")
                return null;

            var itemVersionModel = new ItemVersionModel
            {
                Language = versionElement.Attribute("language").Value,
                Version = int.Parse(versionElement.Attribute("version").Value)
            };

            return itemVersionModel;
        }

        private void PopulateFields(Guid itemId, ItemVersionModel version)
        {
            if (version == null)
                return;

            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            var results = _service.GetItemFields(itemId.ToString().ToUpper(), version.Language, version.Version.ToString(), true, _configuration.SourceDatabase, credentials);
            version.Fields = results.Descendants("field")
                .Select(GetField)
                .Where(x => x != null)
                .ToList();

            // Determine workflow
            var workflowField = version.Fields.FirstOrDefault(x => string.Equals(x.Name, "__Workflow", StringComparison.OrdinalIgnoreCase));
            var workflow = (workflowField != null && !string.IsNullOrEmpty(workflowField.Value))
                ? GetWorkflow(new Guid(workflowField.Value))
                : null;

            if (workflow != null)
            {
                version.HasWorkflow = true;

                var workflowStateField = version.Fields.FirstOrDefault(x => string.Equals(x.Name, "__Workflow state", StringComparison.OrdinalIgnoreCase));
                var workflowState = (workflowStateField != null && !string.IsNullOrEmpty(workflowStateField.Value))
                    ? new Guid(workflowStateField.Value)
                    : Guid.Empty;
                version.InFinalWorkflowState = workflow.FinalState == workflowState;
            }
        }

        private ItemFieldModel GetField(XElement fieldElement)
        {
            if (fieldElement == null || fieldElement.Name != "field")
                return null;

            var valueElement = fieldElement.Element("value");
            var value = (valueElement != null) ? valueElement.Value : "";
 
            var itemFieldModel = new ItemFieldModel
            {
                Id = Guid.Parse(fieldElement.Attribute("fieldid").Value),
                Name = fieldElement.Attribute("name").Value,
                StandardValue = int.Parse(fieldElement.Attribute("standardvalue").Value) == 1,
                Type = fieldElement.Attribute("type").Value,
                Value = value
            };

            return itemFieldModel;
        }

        private WorkflowModel GetWorkflow(Guid id)
        {
            return _workFlows.GetOrAdd(id, GetWorkflowFactory);
        }

        private WorkflowModel GetWorkflowFactory(Guid id)
        {
            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            var results = _service.GetXML(id.ToString().ToUpper(), true, _configuration.SourceDatabase, credentials);

            var stateItemElement = results.Descendants("item")
                .FirstOrDefault(x => x.Attribute("template").Value == "state" && x.Descendants("field").Where(f => f.Attribute("key").Value == "final").Descendants("content").Any(c => c.Value == "1"));
            if (stateItemElement == null)
                return null;

            var workflowModel = new WorkflowModel
            {
                FinalState = Guid.Parse(stateItemElement.Attribute("id").Value),
                Id = id
            };

            return workflowModel;
        }
    }
}