
namespace OneNorth.FieldMigrator.Pipelines.MigrateField
{
    public class CopyFieldValue : IMigrateFieldPipelineProcessor
    {
        public void Process(MigrateFieldPipelineArgs args)
        {
            if (args.Source == null ||
                args.Item == null)
                return;

            var source = args.Source;
            var item = args.Item;

            var field = item.Fields[source.Name];
            if (field == null)
                return;

            if (source.StandardValue)
                field.Reset();
            else
                field.Value = source.Value;
        }
    }
}