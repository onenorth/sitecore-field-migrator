using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OneNorth.FieldMigrator.Helpers;
using Sitecore;
using Sitecore.Resources.Media;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class AttachVersionedMedia : IMigrateVersionPipelineProcessor
    {
        private readonly IHardRockWebServiceProxy _hardRockWebServiceProxy;

        public AttachVersionedMedia() : this(HardRockWebServiceProxy.Instance)
        {
            
        }

        internal AttachVersionedMedia(IHardRockWebServiceProxy hardRockWebServiceProxy)
        {
            _hardRockWebServiceProxy = hardRockWebServiceProxy;
        }

        private readonly List<string> _supportedLanguages = new List<string>();
        public virtual List<string> SupportedLanguages { get { return _supportedLanguages; } }
        protected virtual void AddSupportedLanguage(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                return;

            _supportedLanguages.Add(field.ToLower());
        }

        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            var source = args.Source;
            var item = args.Item;

            if (!SupportedLanguages.Contains(item.Language.Name.ToLower()))
                return;

            // Confirm we are processing a Media Item
            if (!item.Paths.IsMediaItem || item.TemplateID == TemplateIDs.MediaFolder)
                return;

            // Confirm the target is a versioned media item
            if (item.Template.FullName.Contains("Unversioned"))
                return;

            var media = MediaManager.GetMedia(item);

            var blobField = source.Fields.First(x => string.Equals(x.Name, "Blob", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(blobField.Value))
                media.ReleaseStream();
            else
            {
                // Grab extension
                var extensionField = source.Fields.FirstOrDefault(x => string.Equals(x.Name, "Extension", StringComparison.OrdinalIgnoreCase));
                if (extensionField == null)
                    return;

                _hardRockWebServiceProxy.SetContextLanguage(item.Language.Name);
                using (var stream = new MemoryStream(_hardRockWebServiceProxy.MediaDownloadAttachment(source.Item.Id)))
                {
                    media.SetStream(stream, extensionField.Value);
                }
            }
        }
    }
}