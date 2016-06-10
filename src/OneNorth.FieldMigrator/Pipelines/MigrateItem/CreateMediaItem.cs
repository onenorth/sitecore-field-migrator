using System;
using System.IO;
using System.Linq;
using OneNorth.FieldMigrator.Helpers;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class CreateMediaItem : IMigrateItemPipelineProcessor
    {
        private readonly IHardRockWebServiceProxy _hardRockWebServiceProxy;

        public CreateMediaItem() : this(HardRockWebServiceProxy.Instance)
        {

        }

        internal CreateMediaItem(IHardRockWebServiceProxy hardRockWebServiceProxy)
        {
            _hardRockWebServiceProxy = hardRockWebServiceProxy;
        }

        public bool KeepExisting { get; set; }

        public virtual void Process(MigrateItemPipelineArgs args)
        {
            if (args.Source == null)
                return;

            var source = args.Source;

            // Confirm we are processing a Media Item
            if (!source.IsMediaItem)
                return;

            // Check if we should process due to KeepExisting
            var item = Sitecore.Context.Database.GetItem(new ID(source.Id));
            if (KeepExisting && item != null)
            {
                args.AbortPipeline();
                return;
            }

            // Grab extension
            var extensionField = source.Versions.SelectMany(x => x.Fields).FirstOrDefault(x => string.Equals(x.Name, "Extension", StringComparison.OrdinalIgnoreCase));
            if (extensionField == null)
                return;

            var extension = extensionField.Value;
            var versioned = Sitecore.Configuration.Settings.Media.UploadAsVersionableByDefault;

            // Create dummy item that is a place holder
            if (item == null)
            {
                var parent = EnsureMediaFolders(args);
                var templateId = MediaManager.Config.GetTemplate(extension, versioned);
                var template = parent.Database.Templates[templateId];
                item = CreateItem(source.Name, parent, template.ID, new ID(source.Id));
            }
            
            // Get Alternate Text
            string alternateText = null;
            var altField = source.Versions.SelectMany(x => x.Fields).FirstOrDefault(x => string.Equals(x.Name, "Alt", StringComparison.OrdinalIgnoreCase));
            if (altField != null)
                alternateText = altField.Value;

            var fileName = item.Name + "." + extensionField.Value;

            var mediaCreatorOptions = new MediaCreatorOptions
            {
                AlternateText = alternateText,
                Database = item.Database,
                Destination = item.Paths.FullPath,
                FileBased = Sitecore.Configuration.Settings.Media.UploadAsFiles,
                IncludeExtensionInItemName = false,
                KeepExisting = KeepExisting,
                Language = LanguageManager.DefaultLanguage,
                Versioned = versioned
            };

            using (var stream = new MemoryStream(_hardRockWebServiceProxy.MediaDownloadAttachment(source.Id)))
            {
                args.Item = MediaManager.Creator.CreateFromStream(stream, fileName, mediaCreatorOptions);
            }
        }

        private Item EnsureMediaFolders(MigrateItemPipelineArgs args)
        {
            var source = args.Source;

            // Determine if the parent already exists.  If it does, there is nothing to do.
            var parent = Sitecore.Context.Database.GetItem(new ID(source.ParentId));
            if (parent != null)
                return parent;

            // Work backwards ensuring folders exist, skipping the current item
            var firstVersion = source.Versions.First();
            var path = _hardRockWebServiceProxy.GetFullPath(source.Id, firstVersion.Language, firstVersion.Version);

            // Find first ancestor that exists
            var count = path.Count;
            var index = 2; //Skip the first 2 (self and parent)
            while (index < count)
            {
                parent = Sitecore.Context.Database.GetItem(new ID(path[index].Id));
                if (parent != null)
                    break;

                index++;
            }

            // Move to the first missing parent
            index--;

            // Create the parents, furthest to closest
            while (index > 0)
            {
                var folder = path[index];

                // Create the folder.  This becomes the parent for the next item
                parent = CreateItem(folder.Name, parent, TemplateIDs.MediaFolder, new ID(folder.Id));

                Sitecore.Diagnostics.Log.Debug(string.Format("[FieldMigrator] (CreateMediaItem): {0}", parent.Paths.FullPath), this);

                index--;
            }

            return parent;
        }

        private Item CreateItem(string name, Item parent, ID templateId, ID id)
        {
            var item = ItemManager.CreateItem(name, parent, templateId, id, SecurityCheck.Disable);

            item = (item.Language == LanguageManager.DefaultLanguage) ? item : item.Database.GetItem(item.ID, LanguageManager.DefaultLanguage);

            // Create version if it does not exist
            if (item.Versions.Count == 0)
            {
                var disableFiltering = Context.Site.DisableFiltering;
                try
                {
                    Context.Site.DisableFiltering = true;
                    item = item.Versions.AddVersion();
                }
                finally
                {
                    Context.Site.DisableFiltering = disableFiltering;
                }
            }

            return item;
        }
    }
}