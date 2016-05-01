using Sitecore.Data;
using Sitecore.Data.Managers;
using Sitecore.SecurityModel;

namespace OneNorth.FieldMigrator.Pipelines.CreateItem
{
    public class CreateItem : ICreateItemPipelineProcessor
    {
        public virtual void Process(CreateItemPipelineArgs args)
        {
            if (args.Item != null ||
                ID.IsNullOrEmpty(args.ItemId) ||
                args.Source == null || 
                args.Parent == null || 
                args.Template == null)
                return;

            args.Item = ItemManager.CreateItem(args.Source.Name, args.Parent, args.Template.ID, args.ItemId, SecurityCheck.Disable);
        }
    }
}