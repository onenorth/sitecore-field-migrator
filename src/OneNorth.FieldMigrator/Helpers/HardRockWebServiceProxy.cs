using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void TraverseTree(Guid rootId, Action<ItemModel> itemAction)
        {
            // Get Root Item Model
            var itemModel = GetItem(rootId);
            if (itemModel == null)
            {
                Sitecore.Diagnostics.Log.Warn(string.Format("[FieldMigrator] (TraverseTree) could not find item with id:{0}", rootId), this);
                return;
            }
            var templateInclude = _configuration.TemplateIncludes.FirstOrDefault(x => x.SourceTemplateId == itemModel.TemplateId);
            var deep = (templateInclude != null && templateInclude.IncludeAllDescendants);
            if (deep)
                itemModel = GetItem(rootId, deep:true);

            if (itemModel != null)
            {
                // Process Root
                RunItemActions(itemModel, itemAction);

                // Process Children if needed
                if (!deep)
                    TraverseChildren(itemModel, itemAction);
            }
        }

        private void RunItemActions(ItemModel itemModel, Action<ItemModel> itemAction)
        {
            try
            {
                itemAction(itemModel);

                if (itemModel.Children != null)
                {
                    foreach (var child in itemModel.Children)
                    {
                        RunItemActions(child, itemAction);
                    }
                }
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] (HardRockWebServiceProxy) RunItemActions ItemId: {0}", itemModel.Id), ex, this);
            }
        }

        private void TraverseChildren(ItemModel parent, Action<ItemModel> itemAction)
        {
            var children = GetChildren(parent.Id);
            foreach (var child in children)
            {
                var templateInclude = _configuration.TemplateIncludes.FirstOrDefault(x => x.SourceTemplateId == child.TemplateId);
                var deep = (templateInclude != null && templateInclude.IncludeAllDescendants);
                var itemModel = GetItem(child.Id, parent, deep);
                if (itemModel != null)
                {
                    RunItemActions(itemModel, itemAction);
                    if (!deep && child.HasChildren)
                        TraverseChildren(itemModel, itemAction);
                }
            }
        }

        private IEnumerable<ChildModel> GetChildren(Guid parentId)
        {
            var children = new List<ChildModel>();
            try
            {
                var credentials = new Credentials
                {
                    UserName = _configuration.SourceUserName,
                    Password = _configuration.SourcePassword
                };

                var results = _service.GetChildren(parentId.ToString().ToUpper(), _configuration.SourceDatabase, credentials);
                var childElements = results.XPathSelectElements("//item");

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
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] (HardRockWebServiceProxy) GetChildren ParentId: {0}", parentId), ex, this);
            }
            return children;
        }

        private ItemModel GetItem(Guid id, ItemModel parent = null, bool deep = false)
        {
            try
            {
                var credentials = new Credentials
                {
                    UserName = _configuration.SourceUserName,
                    Password = _configuration.SourcePassword
                };

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                var results = _service.GetXML(id.ToString().ToUpper(), deep, _configuration.SourceDatabase, credentials);

                stopWatch.Stop();

                Sitecore.Diagnostics.Log.Info(string.Format("[FieldMigrator] (HardRockWebServiceProxy) GetXML id:{0} deep:{1} finished in {2}", id, deep, stopWatch.Elapsed), this);

                var itemElement = results.XPathSelectElement("//item");
                if (itemElement == null)
                    return null;

                return GetItem(itemElement, parent, deep);
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] (HardRockWebServiceProxy) GetItem ItemId: {0} deep:{1}", id, deep), ex, this);
            }
            return null;
        }

        private ItemModel GetItem(XElement itemElement, ItemModel parent, bool deep)
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

            // Determine if is MediaItem based on configured template ids.
            if (_configuration.MediaItemTemplateIds.Contains(itemModel.TemplateId))
                itemModel.IsMediaItem = true;

            // Gather the versions
            var versionElements = itemElement.Elements("version");
            itemModel.Versions = versionElements
                .Select(x => GetVersion(x, itemModel))
                .Where(x => x != null)
                .ToList();

            // Gather the children
            if (deep)
            {
                var childItemElements = itemElement.Elements("item");
                itemModel.Children = childItemElements.Select(x => GetItem(x, itemModel, deep)).ToList();
            }

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
            if (PopulateFieldValues(versionModel, versionElement))
                return versionModel;
            return null;
        }

        private bool PopulateFieldValues(VersionModel version, XElement versionElement)
        {
            if (version == null || version.Fields == null || versionElement == null || versionElement.Name != "version")
                return false;

            var fields = version.Fields;

            // Set the Version on the fields
            foreach (var field in fields)
            {
                field.Version = version;
            }

            // This version may not exist if it is version #1.  This is due to behavior of the Hard Rocks web service.
            var versionExists = version.Version != 1;

            // Populate known field values
            var fieldElements = versionElement.Descendants("field");
            foreach (var fieldElement in fieldElements)
            {
                var content = fieldElement.Element("content");
                var value = (content != null) ? content.Value : "";

                var fieldId = Guid.Parse(fieldElement.Attribute("tfid").Value);
                var field = fields.FirstOrDefault(x => x.Id == fieldId);
                if (field != null)
                {
                    if (!field.Shared)
                        versionExists = true;
                    field.Value = value;
                }
            }

            // Determine workflow
            var workflowField = fields.FirstOrDefault(x => string.Equals(x.Name, "__Workflow", StringComparison.OrdinalIgnoreCase));
            var workflow = (workflowField != null && !string.IsNullOrEmpty(workflowField.Value))
                ? GetWorkflow(new Guid(workflowField.Value))
                : null;

            version.HasWorkflow = false;
            version.WorkflowState = WorkflowState.Empty;

            if (workflow != null)
            {
                version.HasWorkflow = true;

                var workflowStateField = fields.FirstOrDefault(x => string.Equals(x.Name, "__Workflow state", StringComparison.OrdinalIgnoreCase));
                var workflowState = (workflowStateField != null && !string.IsNullOrEmpty(workflowStateField.Value))
                    ? new Guid(workflowStateField.Value)
                    : Guid.Empty;
                if (workflowState != Guid.Empty)
                    version.WorkflowState = (workflow.FinalState == workflowState) ? WorkflowState.Final : WorkflowState.NonFinal;
            }

            return versionExists;
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

        public byte[] MediaDownloadAttachment(Guid mediaItemId)
        {
            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            var parameters = new ArrayOfAnyType {_configuration.SourceDatabase, mediaItemId.ToString("B").ToUpper()};

            var result = _service.Execute("Sitecore.Rocks.Server.Requests.Media.DownloadAttachment", parameters, credentials);
            return Convert.FromBase64String(result);
        }

        public void SetContextLanguage(string languageName)
        {
            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            var parameters = new ArrayOfAnyType { languageName };

            _service.Execute("Sitecore.Rocks.Server.Requests.Languages.SetContextLanguage", parameters, credentials);
        }

        public void GetItemHeader(Guid id)
        {
            var credentials = new Credentials
            {
                UserName = _configuration.SourceUserName,
                Password = _configuration.SourcePassword
            };

            var parameters = new ArrayOfAnyType { _configuration.SourceDatabase };

            _service.Execute("Sitecore.Rocks.Server.Requests.Languages.SetContextLanguage", parameters, credentials);
        }
    }
}