using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using OneNorth.FieldMigrator.Configuration;
using OneNorth.FieldMigrator.Pipelines.MigrateVersion;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;

namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public class AttachUnversionedMedia : IMigrateItemPipelineProcessor
    {
        private readonly IFieldMigratorConfiguration _configuration;

        public AttachUnversionedMedia() : this(FieldMigratorConfiguration.Instance)
        {

        }

        internal AttachUnversionedMedia(IFieldMigratorConfiguration configuration)
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
                    var mediaItem = new MediaItem(item);
                    var media = MediaManager.GetMedia(mediaItem);
                    media.SetStream(stream, extensionField.Value);
                }
            }
        }
    }
}