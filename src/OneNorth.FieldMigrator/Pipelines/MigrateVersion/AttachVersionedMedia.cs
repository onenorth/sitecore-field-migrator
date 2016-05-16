using System;
using System.IO;
using System.Linq;
using System.Net;
using OneNorth.FieldMigrator.Configuration;
using Sitecore.Data.Items;
using Sitecore.Resources.Media;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class AttachVersionedMedia : IMigrateVersionPipelineProcessor
    {
        private readonly IFieldMigratorConfiguration _configuration;

        public AttachVersionedMedia() : this(FieldMigratorConfiguration.Instance)
        {
            
        }

        internal AttachVersionedMedia(IFieldMigratorConfiguration configuration)
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
                var mediaItem = new MediaItem(item);
                var media = MediaManager.GetMedia(mediaItem);
                media.SetStream(stream, extensionField.Value);
            } 
        }
    }
}