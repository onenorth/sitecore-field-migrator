namespace OneNorth.FieldMigrator.Pipelines.MigrateItem
{
    public interface IMigrateItemPipelineProcessor
    {
        void Process(MigrateItemPipelineArgs args);
    }
}
