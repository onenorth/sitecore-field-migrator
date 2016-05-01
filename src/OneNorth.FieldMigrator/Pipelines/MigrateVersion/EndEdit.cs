namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class EndEdit : IMigrateVersionPipelineProcessor
    {
        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.EditContext == null)
                return;

            args.EditContext.Dispose();
            if (args.EditContext != null)
                args.EditContext = null;
        }
    }
}