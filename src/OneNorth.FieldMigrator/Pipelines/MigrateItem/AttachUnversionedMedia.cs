using System;
using System.IO;
using System.Linq;
using OneNorth.FieldMigrator.Helpers;
using Sitecore;
using Sitecore.Resources.Media;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class AttachUnversionedMedia : IMigrateItemPipelineProcessor
    {
        private readonly IHardRockWebServiceProxy _hardRockWebServiceProxy;

        public AttachUnversionedMedia() : this(HardRockWebServiceProxy.Instance)
        {

        }

        internal AttachUnversionedMedia(IHardRockWebServiceProxy hardRockWebServiceProxy)
        {
            _hardRockWebServiceProxy = hardRockWebServiceProxy;
        }

        public virtual void Process(MigrateItemPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            var source = args.Source;
            var item = args.Item;

            // Confirm we are processing a Media Item
            if (!item.Paths.IsMediaItem || item.TemplateID == TemplateIDs.MediaFolder)
                return;

            // Confirm the target is an unversioned media item
            if (!item.Template.FullName.Contains("Unversioned"))
                return;

            var media = MediaManager.GetMedia(item);

            var blobField = source.Versions.SelectMany(x => x.Fields).First(x => string.Equals(x.Name, "Blob", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(blobField.Value))
                media.ReleaseStream();
            else
            {
                // Grab extension
                var extensionField = source.Versions.SelectMany(x => x.Fields).FirstOrDefault(x => string.Equals(x.Name, "Extension", StringComparison.OrdinalIgnoreCase));
                if (extensionField == null)
                    return;

                using (var stream = new MemoryStream(_hardRockWebServiceProxy.MediaDownloadAttachment(source.Id)))
                {
                    media.SetStream(stream, extensionField.Value);
                }
            }
        }
    }
}