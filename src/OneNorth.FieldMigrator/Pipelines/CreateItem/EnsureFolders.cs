using System;
using System.Linq;
using OneNorth.FieldMigrator.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.SecurityModel;

namespace OneNorth.FieldMigrator.Pipelines.CreateItem
{
    public class EnsureFolders : ICreateItemPipelineProcessor
    {
        private readonly ID _folderTemplateId = new ID("{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}");
        private readonly ID _mediaFolderTemplateId = new ID("{FE5DD826-48C6-436D-B87A-7C4210C7413B}");
        private readonly Guid _mediaLibraryId = new Guid("{3D6658D8-A0BF-4E75-B3E2-D050FABCF4E1}");

        private readonly IHardRockWebServiceProxy _hardRockWebServiceProxy;

        public EnsureFolders() : this(HardRockWebServiceProxy.Instance)
        {
            
        }

        internal EnsureFolders(IHardRockWebServiceProxy hardRockWebServiceProxy)
        {
            _hardRockWebServiceProxy = hardRockWebServiceProxy;
        }

        public string DefaultLanguage { get; set; }

        public virtual void Process(CreateItemPipelineArgs args)
        {
            if (args.Source == null ||
                args.Parent != null ||
                args.Template == null)
                return;

            var source = args.Source;

            // Determine if the parent already exists.  If it does, there is nothing to do.
            args.Parent = Sitecore.Context.Database.GetItem(new ID(source.ParentId));
            if (args.Parent != null)
                return;

            Item parent = null;

            // Work backwards ensuring folders exist, skipping the current item
            var firstVersion = source.Versions.First();
            var path = _hardRockWebServiceProxy.GetFullPath(source.Id, firstVersion.Language, firstVersion.Version);

            // Get folder type based on location
            var folderTemplateId = (path.Any(f => f.Id == _mediaLibraryId)) ? _mediaFolderTemplateId : _folderTemplateId;

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
                parent = ItemManager.CreateItem(folder.Name, parent, folderTemplateId, new ID(folder.Id), SecurityCheck.Disable);
                
                // Add at least 1 version
                if (!string.IsNullOrEmpty(DefaultLanguage))
                {
                    var language = LanguageManager.GetLanguage(DefaultLanguage);
                    parent = (parent.Language == language) ? parent : parent.Database.GetItem(parent.ID, language);

                    // Create version if it does not exist
                    if (parent.Versions.Count == 0)
                    {
                        var disableFiltering = Sitecore.Context.Site.DisableFiltering;
                        try
                        {
                            Sitecore.Context.Site.DisableFiltering = true;
                            parent = parent.Versions.AddVersion();
                        }
                        finally
                        {
                            Sitecore.Context.Site.DisableFiltering = disableFiltering;
                        }
                    }
                }

                Sitecore.Diagnostics.Log.Debug(string.Format("[FieldMigrator] Created: {0}", parent.Paths.FullPath), this);

                index--;
            }

            args.Parent = parent;
        }
    }
}