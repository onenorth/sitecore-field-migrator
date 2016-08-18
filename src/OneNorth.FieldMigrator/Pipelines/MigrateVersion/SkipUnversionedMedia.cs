using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class SkipUnversionedMedia : IMigrateVersionPipelineProcessor
    {

        private readonly List<string> _exceptions = new List<string>();
        public virtual List<string> Exceptions { get { return _exceptions; } }
        protected virtual void AddException(string field)
        {
            if (string.IsNullOrWhiteSpace(field))
                return;

            _exceptions.Add(field);
        }

        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Item == null ||
                args.Source == null)
                return;

            var source = args.Source;
            var item = args.Item;

            // Confirm we are processing a Media Item
            if (!item.Paths.IsMediaItem || item.TemplateID == TemplateIDs.MediaFolder)
                return;

            // Is the original media item unversioned.
            var blobField = source.Fields.FirstOrDefault(x => x.Name == "Blob");
            if (blobField == null || !blobField.Shared)
                return;

            // This is an unversioned media item. Are any exceptions registered
            if (Exceptions.Contains(source.Language.ToLower()))
                return;

            // Do not process the media item
            args.AbortPipeline();
        }
    }
}