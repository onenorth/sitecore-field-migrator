using System;
using System.IO;
using System.Linq;
using System.Net;
using OneNorth.FieldMigrator.Configuration;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class ForMediaItemsAttachUnversionedMedia : IMigrateItemPipelineProcessor
    {
        private readonly IFieldMigratorConfiguration _configuration;

        public ForMediaItemsAttachUnversionedMedia() : this(FieldMigratorConfiguration.Instance)
        {

        }

        internal ForMediaItemsAttachUnversionedMedia(IFieldMigratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual void Process(MigrateItemPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            var source = args.Source;
            var item = args.Item;

            // Confirm we are processing a Media Item
            if (!item.Paths.IsMediaItem)
                return;

            // Confirm this is an unversioned media item
            var blobField = source.Versions.SelectMany(x => x.Fields).FirstOrDefault(x => string.Equals(x.Name, "Blob", StringComparison.OrdinalIgnoreCase));
            if (blobField == null || !blobField.Shared)
                return;

            var mediaItem = new MediaItem(item);
            var media = MediaManager.GetMedia(mediaItem);

            if (blobField.StandardValue)
                media.ReleaseStream();
            else
            {
                // Grab extension
                var extensionField = source.Versions.SelectMany(x => x.Fields).FirstOrDefault(x => string.Equals(x.Name, "Extension", StringComparison.OrdinalIgnoreCase));
                if (extensionField == null)
                    return;

                // Determine the source URL for the media
                var sourceEndpoint = new Uri(_configuration.SourceEndpointAddress);
                var sourceRelativeMediaUrl = string.Format("/~/media/{0}.ashx", source.Id.ToString("N"));
                var sourceMediaUrl = new Uri(sourceEndpoint, sourceRelativeMediaUrl);

                using (var client = new WebClient())
                {
                    using (var stream = new MemoryStream(client.DownloadData(sourceMediaUrl)))
                    {
                       media.SetStream(stream, extensionField.Value);
                    }
                }
            }
        }
    }
}