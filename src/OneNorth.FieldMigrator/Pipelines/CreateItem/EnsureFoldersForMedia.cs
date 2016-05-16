using System;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.SecurityModel;

namespace OneNorth.FieldMigrator.Pipelines.CreateItem
{
    public class EnsureFoldersForMedia : ICreateItemPipelineProcessor
    {
        private readonly ID _mediaFolderTemplateId = new ID("{FE5DD826-48C6-436D-B87A-7C4210C7413B}");
        private readonly Guid _mediaLibraryId = new Guid("{3D6658D8-A0BF-4E75-B3E2-D050FABCF4E1}");

        public virtual void Process(CreateItemPipelineArgs args)
        {
            if (args.Source == null ||
                args.Parent != null)
                return;

            var source = args.Source;

            // Only process media items.  These are children of the Media Library
            if (!source.Versions.Any(v => v.Path.Any(f => f.Id == _mediaLibraryId)))
                return;

            Item parent = null;

            // Work backwards ensuring folders exist, skipping the current item
            var firstVersion = source.Versions.First();
            var count = firstVersion.Path.Count;
            for (var i = count - 1; i > 0; i--)
            {
                // If the folder already exists, skip it, moving onto the next.
                var folderModel = firstVersion.Path[i];
                var folder = Sitecore.Context.Database.GetItem(new ID(folderModel.Id));
                if (folder != null)
                {
                    parent = folder;
                    continue;
                }

                // Create the folder.  This becomes the parent for the next item
               parent = ItemManager.CreateItem(folderModel.Name, parent, _mediaFolderTemplateId, new ID(folderModel.Id), SecurityCheck.Disable);
            }

            args.Parent = parent;
        }
    }
}