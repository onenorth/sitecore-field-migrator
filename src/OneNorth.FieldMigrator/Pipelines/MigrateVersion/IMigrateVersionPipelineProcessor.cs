

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public interface IMigrateVersionPipelineProcessor
    {
        void Process(MigrateVersionPipelineArgs args);
    }
}