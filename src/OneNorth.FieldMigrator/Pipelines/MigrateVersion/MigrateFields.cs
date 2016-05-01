using OneNorth.FieldMigrator.Pipelines.MigrateField;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class MigrateFields : IMigrateVersionPipelineProcessor
    {
        private readonly IMigrateFieldPipeline _migrateFieldPipeline;

        public MigrateFields() : this(MigrateFieldPipeline.Instance)
        {

        }

        internal MigrateFields(IMigrateFieldPipeline migrateFieldPipeline)
        {
            _migrateFieldPipeline = migrateFieldPipeline;
        }

        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            foreach (var itemFieldModel in args.Source.Fields)
            {

                _migrateFieldPipeline.Run(itemFieldModel, args.Item);
            }
        }
    }
}