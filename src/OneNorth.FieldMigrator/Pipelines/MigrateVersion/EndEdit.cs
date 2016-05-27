namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class EndEdit : IMigrateVersionPipelineProcessor
    {
        public bool DisableEvents { get; set; }

        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Item == null)
                return;

            args.Item.Editing.EndEdit(DisableEvents);
        }
    }
}