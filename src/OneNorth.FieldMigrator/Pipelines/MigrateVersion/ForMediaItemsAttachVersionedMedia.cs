using System;
using System.IO;
using System.Linq;
using System.Net;
using OneNorth.FieldMigrator.Configuration;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class ForMediaItemsAttachVersionedMedia : IMigrateVersionPipelineProcessor
    {
        private readonly IFieldMigratorConfiguration _configuration;

        public ForMediaItemsAttachVersionedMedia() : this(FieldMigratorConfiguration.Instance)
        {
            
        }

        internal ForMediaItemsAttachVersionedMedia(IFieldMigratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            var source = args.Source;
            var item = args.Item;

            // Confirm we are processing a Media Item
            if (!item.Paths.IsMediaItem)
                return;

            // Confirm this is a versioned media item
            var blobField = source.Fields.FirstOrDefault(x => string.Equals(x.Name, "Blob", StringComparison.OrdinalIgnoreCase));
            if (blobField == null || blobField.Shared)
                return;

            var mediaItem = new MediaItem(item);
            var media = MediaManager.GetMedia(mediaItem);

            if (blobField.Value == null)
                media.ReleaseStream();
            else
            {
                // Grab extension
                var extensionField = source.Fields.FirstOrDefault(x => string.Equals(x.Name, "Extension", StringComparison.OrdinalIgnoreCase));
                if (extensionField == null)
                    return;

                // Determine the source URL for the media
                var sourceEndpoint = new Uri(_configuration.SourceEndpointAddress);
                var sourceRelativeMediaUrl = string.Format("/~/media/{0}.ashx?la={1}", source.Item.Id.ToString("N"), source.Language);
                var sourceMediaUrl = new Uri(sourceEndpoint, sourceRelativeMediaUrl);

                // Download Media
                var client = new WebClient();
                using (var stream = new MemoryStream(client.DownloadData(sourceMediaUrl)))
                {
                    media.SetStream(stream, extensionField.Value);
                }
            }
        }
    }
}