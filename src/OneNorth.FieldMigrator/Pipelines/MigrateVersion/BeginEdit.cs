
namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class BeginEdit : IMigrateVersionPipelineProcessor
    {
        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Item == null)
                return;

            args.Item.Editing.BeginEdit();
        }
    }
}