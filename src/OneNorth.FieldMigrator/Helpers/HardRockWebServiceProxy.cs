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
        private readonly ConcurrentDictionary<Guid, List<FieldModel>> _templateFields;
        private readonly ConcurrentDictionary<Guid, WorkflowModel> _workFlows;

        private HardRockWebServiceProxy() : this(FieldMigratorConfiguration.Instance)
        {

        }

        internal HardRockWebServiceProxy(IFieldMigratorConfiguration configuration)
        {
            _configuration = configuration;
            _templateFields = new ConcurrentDictionary<Guid, List<FieldModel>>();
            _workFlows = new ConcurrentDictionary<Guid, WorkflowModel>();

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
            {
                MaxReceivedMessageSize = 524288000, // 500 Megabytes
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                SendTimeout = TimeSpan.FromMinutes(10)
            };
            var remoteAddress = new EndpointAddress(_configuration.SourceEndpointAddress);
            _service = new SitecoreWebService2SoapClient(binding, remoteAddress);
        }

        public void TraverseTree(Guid rootId, Action<ItemModel> itemAction, FolderModel[] relativePath = null, bool? hasChildren = null)
        {
            var itemModel = GetItem(rootId, false);
            itemModel.RelativePath = relativePath;
            itemAction(itemModel);

            if (hasChildren.HasValue && !hasChildren.Value)
                return;

            var pathList = new List<FolderModel> {
                new FolderModel {
                    Id = itemModel.Id,
                    Name = itemModel.Name,
                    TemplateId = itemModel.TemplateId
                }
            };
            if (relativePath != null)
                pathList.AddRange(relativePath);
            var pathArray = pathList.ToArray();

            var children = GetChildren(rootId);
            foreach (var child in children)
            {
                TraverseTree(child.Id, itemAction, pathArray, child.HasChildren);
            }
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

            Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] GetXML id:{0} deep:{1}", id, deep), this);
            var results = _service.GetXML(id.ToString().ToUpper(), deep, _configuration.SourceDatabase, credentials);
            Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] GetXML id:{0} deep:{1} finished", id, deep), this);

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
                ParentId = Guid.Parse(itemElement.Attribute("parentid").Value),
                SortOrder = int.Parse(itemElement.Attribute("sortorder").Value),
                TemplateId = Guid.Parse(itemElement.Attribute("tid").Value),
                TemplateName = itemElement.Attribute("template").Value
            };

            // Determine if is MediaItem based on configured template ids.
            if (_configuration.MediaItemTemplateIds.Contains(itemModel.TemplateId))
                itemModel.IsMediaItem = true;

            // Gather the versions
            var versionElements = itemElement.Elements("version");
            itemModel.Versions = versionElements.Select(x => GetVersion(x, itemModel)).ToList();

            return itemModel;
        }

        private VersionModel GetVersion(XElement versionElement, ItemModel owner)
        {
            if (versionElement == null || versionElement.Name != "version")
                return null;

            var versionModel = new VersionModel
            {
                Item = owner,
                Language = versionElement.Attribute("language").Value,
                Version = int.Parse(versionElement.Attribute("version").Value)
            };

            // Get all fields
            versionModel.Fields = GetTemplateFields(owner.TemplateId, owner.Id, versionModel.Language, versionModel.Version);

            // Populate the field values
            PopulateFieldValues(versionModel, versionElement);

            return versionModel;
        }

        private void PopulateFieldValues(VersionModel version, XElement versionElement)
        {
            if (version == null || version.Fields == null || versionElement == null || versionElement.Name != "version")
                return;

            var fields = version.Fields;

            // Set the Version on the fields
            foreach (var field in fields)
            {
                field.Version = version;
            }

            // Populate known field values
            var fieldElements = versionElement.Descendants("field");
            foreach (var fieldElement in fieldElements)
            {
                var content = fieldElement.Element("content");
                var value = (content != null) ? content.Value : "";

                var fieldId = Guid.Parse(fieldElement.Attribute("tfid").Value);
                var field = fields.FirstOrDefault(x => x.Id == fieldId);
                if (field != null)
                    field.Value = value;
            }

            // Determine workflow
            var workflowField = fields.FirstOrDefault(x => string.Equals(x.Name, "__Workflow", StringComparison.OrdinalIgnoreCase));
            var workflow = (workflowField != null && !string.IsNullOrEmpty(workflowField.Value))
                ? GetWorkflow(new Guid(workflowField.Value))
                : null;

            if (workflow != null)
            {
                version.HasWorkflow = true;

                var workflowStateField = fields.FirstOrDefault(x => string.Equals(x.Name, "__Workflow state", StringComparison.OrdinalIgnoreCase));
                var workflowState = (workflowStateField != null && !string.IsNullOrEmpty(workflowStateField.Value))
                    ? new Guid(workflowStateField.Value)
                    : Guid.Empty;
                version.InFinalWorkflowState = workflow.FinalState == workflowState;
            }
        }

        private List<FieldModel> GetTemplateFields(Guid templateId, Guid itemId, string language, int version)
        {
            var fields = _templateFields.GetOrAdd(templateId, key => GetTemplateFieldsFactory(key, itemId, language, version));
            return fields.Select(x => x.Clone()).ToList();
        }

        private List<FieldModel> GetTemplateFieldsFactory(Guid templateId, Guid itemId, string language, int version)
        {
            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            Sitecore.Diagnostics.Log.Debug(string.Format("[FieldMigrator] GetTemplateFieldsFactory templateid:{0}", templateId), this);
            var results = _service.GetItemFields(itemId.ToString().ToUpper(), language, version.ToString(), true, _configuration.SourceDatabase, credentials);
            var fields = results.Descendants("field")
                .Select(GetTemplateField)
                .Where(x => x != null)
                .OrderBy(x => x.Name)
                .ToList();

            return fields;
        }

        private FieldModel GetTemplateField(XElement fieldElement)
        {
            if (fieldElement == null || fieldElement.Name != "field")
                return null;

            var fieldModel = new FieldModel
            {
                Id = Guid.Parse(fieldElement.Attribute("fieldid").Value),
                Name = fieldElement.Attribute("name").Value,
                Shared = int.Parse(fieldElement.Attribute("shared").Value) == 1,
                Type = fieldElement.Attribute("type").Value,
                Unversioned = int.Parse(fieldElement.Attribute("unversioned").Value) == 1,
                Value = null
            };

            return fieldModel;
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

            Sitecore.Diagnostics.Log.Debug(string.Format("[FieldMigrator] GetWorkflowFactory id:{0}", id), this);
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

        public List<FolderModel> GetFullPath(Guid itemId, string language, int version)
        {
            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            var results = _service.GetItemFields(itemId.ToString().ToUpper(), language, version.ToString(), true, _configuration.SourceDatabase, credentials);
            var path = results.Elements("path").Elements("item")
                .Select(GetFolder)
                .Where(x => x != null)
                .ToList();

            return path;
        }

        private FolderModel GetFolder(XElement folderElement)
        {
            if (folderElement == null || folderElement.Name != "item")
                return null;

            var folderModel = new FolderModel
            {
                DisplayName = folderElement.Attribute("displayname").Value,
                Id = Guid.Parse(folderElement.Attribute("id").Value),
                Name = folderElement.Attribute("name").Value
            };

            return folderModel;
        }
    }
}