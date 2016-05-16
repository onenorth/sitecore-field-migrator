using System.Collections.Generic;
using System.Linq;
using OneNorth.FieldMigrator.Helpers;
using OneNorth.FieldMigrator.Pipelines.MigrateVersion;
using Sitecore.Data;
using Sitecore.Data.Fields;

namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class ProcessMedia : IMigrateFieldPipelineProcessor
    {
        private readonly IMigrationHelper _migrationHelper;

        public ProcessMedia() : this(MigrationHelper.Instance)
        {

        }

        internal protected ProcessMedia(IMigrationHelper migrationHelper)
        {
            _migrationHelper = migrationHelper;
        }

        public void Process(MigrateFieldPipelineArgs args)
        {
            if (args.Field == null)
                return;

            var field = args.Field;

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

            // TODO: Rich Text

            var id = mediaIds.FirstOrDefault();
            if (ID.IsNullOrEmpty(id))
                return;

            _migrationHelper.MigrateRoot(id.Guid);
        }
    }
}