using System.Collections.Generic;
using System.Linq;
using Sitecore.Data;
using Sitecore.Data.Fields;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class CopyValue : IMigrateFieldPipelineProcessor
    {
        public void Process(MigrateFieldPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null ||
                args.Field == null)
                return;

            var source = args.Source;
            var field = args.Field;

            if (source.StandardValue)
                field.Reset();
            else
                field.Value = source.Value;

            ProcessMedia(field);
        }

        private void ProcessMedia(Field field)
        {
            // Process Media
            var mediaIds = new List<ID>();
            if (field.Type == "File")
            {
                var fileField = (FileField)field;
                if (!ID.IsNullOrEmpty(fileField.MediaID))
                    mediaIds.Add(fileField.MediaID);
            }
            else if (field.Type == "Image")
            {
                var imageField = (ImageField)field;
                if (!ID.IsNullOrEmpty(imageField.MediaID))
                    mediaIds.Add(imageField.MediaID);
            }

            var id = mediaIds.FirstOrDefault();
            if (ID.IsNullOrEmpty(id))
                return;

            
        }
    }
}