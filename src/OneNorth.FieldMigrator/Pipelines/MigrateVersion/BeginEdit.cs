using Sitecore.Data.Items;
using Sitecore.SecurityModel;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class BeginEdit : IMigrateVersionPipelineProcessor
    {
        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Item == null ||
                args.EditContext != null)
                return;

            args.EditContext = new EditContext(args.Item, SecurityCheck.Disable);
        }
    }
}