﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OneNorth.FieldMigrator.Configuration;
using OneNorth.FieldMigrator.Models;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.SecurityModel;

namespace OneNorth.FieldMigrator.Helpers
{
    public class MigrationHelper : IMigrationHelper
    {
        private static readonly IMigrationHelper _instance = new MigrationHelper();

        public static IMigrationHelper Instance
        {
            get { return _instance; }
        }

        private readonly IFieldMigratorConfiguration _configuration;
        private readonly IHardRockWebServiceProxy _hardRockWebServiceProxy;

        private MigrationHelper() : this(
            FieldMigratorConfiguration.Instance,
            HardRockWebServiceProxy.Instance)
        {
            
        }

        internal MigrationHelper(
            IFieldMigratorConfiguration configuration,
            IHardRockWebServiceProxy hardRockWebServiceProxy)
        {
            _configuration = configuration;
            _hardRockWebServiceProxy = hardRockWebServiceProxy;
        }

        public void MigrateRoot(Guid itemId)
        {
            var sourceItem = _hardRockWebServiceProxy.GetItem(itemId, true);
            MigrateRoot(sourceItem);
        }

        private void MigrateRoot(ItemModel sourceItem)
        {
            MigrateItem(sourceItem);
            foreach (var child in sourceItem.Children)
            {
                MigrateRoot(child);
            }
        }

        private void MigrateItem(ItemModel sourceItem)
        {
            var template = _configuration.Templates.FirstOrDefault(x => x.TemplateId == sourceItem.TemplateId);
            var parentTemplates = sourceItem.Parents(x => x.Parent)
                .Select(parent => _configuration.Templates.FirstOrDefault(x => x.TemplateId == parent.TemplateId))
                .Where(x => x != null);
            if (template == null && !parentTemplates.Any(x => x.IncludeAllChildren))
                return;

            var templateId = (template != null && template.TargetTemplateId != Guid.Empty)
                ? template.TargetTemplateId
                : sourceItem.TemplateId;

            var item = EnsureItemNode(sourceItem, templateId);
            if (item == null)
                return;

            // Get the most recent version by language
            var webServiceVersions = sourceItem.Versions
                .OrderBy(x => x.Language)
                .ThenByDescending(x => x.Version)
                .GroupBy(x => x.Language)
                .Select(x => x.First());

            // Update each version
            foreach (var webServiceVersion in webServiceVersions)
            {
                MigrateVersion(webServiceVersion, item);
            }
        }

        private Item EnsureItemNode(ItemModel sourceItem, Guid templateId)
        {
            // See if the item already exists.
            var item = Sitecore.Context.Database.GetItem(new ID(sourceItem.Id));
            if (item != null)
                return item;

            // The item was not found.  Determine the parent so the item can be created.
            var parent = EnsureParentNode(sourceItem);
            if (parent == null)
                return null;

            var template = EnsureTemplate(sourceItem, templateId);
            if (template == null)
                return null;

            item = ItemManager.CreateItem(sourceItem.Name, parent, template.ID, new ID(sourceItem.Id), SecurityCheck.Disable);

            return item;
        }

        private Item EnsureParentNode(ItemModel sourceItem)
        {
            // The item was not found.  Now we need to determine the parent so we can create the item.
            var parent = Sitecore.Context.Database.GetItem(new ID(sourceItem.ParentId));
            //if (parent == null)
            //    parent = GetParent(entity, context);
            if (parent == null)
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] Could not find the parent with id {0} for {1}",
                    sourceItem.ParentId.ToString().ToUpper(),
                    sourceItem.Id.ToString().ToUpper()), this);

            return parent;
        }

        private TemplateItem EnsureTemplate(ItemModel sourceItem, Guid templateId)
        {
            // Find the template
            var template = Sitecore.Context.Database.GetTemplate(new ID(templateId));
            if (template == null)
                Sitecore.Diagnostics.Log.Error(string.Format("[FieldMigrator] Could not find the template with id {0} for {1}", 
                    templateId.ToString().ToUpper(),
                    sourceItem.Id.ToString().ToUpper()), this);

            return template;
        }

        private void MigrateVersion(ItemVersionModel itemVersionModel, Item item)
        {
            // Ensure we are working with the correct language
            var language = LanguageManager.GetLanguage(itemVersionModel.Language);
            item = (item.Language == language) ? item : item.Database.GetItem(item.ID, language);

            // Create version if it does not exist
            if (item.Versions.Count == 0)
            {
                var disableFiltering = Sitecore.Context.Site.DisableFiltering;
                try
                {
                    Sitecore.Context.Site.DisableFiltering = true;
                    item = item.Versions.AddVersion();
                }
                finally
                {
                    Sitecore.Context.Site.DisableFiltering = disableFiltering;
                }
            }

            using (new EditContext(item, SecurityCheck.Disable))
            {
                foreach (var webServiceField in itemVersionModel.Fields)
                {
                    if (string.IsNullOrEmpty(webServiceField.Key) ||
                        (webServiceField.Key.StartsWith("__") &&
                        !webServiceField.Key.Equals("__display name") &&
                        !webServiceField.Key.Equals("__hidden") &&
                        !webServiceField.Key.Equals("__hide version") &&
                        !webServiceField.Key.Equals("__never publish") &&
                        !webServiceField.Key.Equals("__sortorder")))
                        continue;

                    var field = item.Fields[webServiceField.Key];
                    if (field == null)
                        continue;

                    if (webServiceField.Value == null)
                        field.Reset();
                    else 
                        field.Value = webServiceField.Value;
                }

                var workflow = item.Database.WorkflowProvider.GetWorkflow(item);
                if (workflow != null)
                {
                    var states = workflow.GetStates();
                    var finalState = states.First(x => x.FinalState);
                    var currentState = workflow.GetState(item);
                    if (currentState != finalState)
                        item.Fields["__Workflow state"].Value = finalState.StateID;
                }
            }
        }
    }
}