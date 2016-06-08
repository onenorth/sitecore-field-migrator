using OneNorth.FieldMigrator.Models;

namespace OneNorth.FieldMigrator
{
    public static class ItemModelExtensions
    {

        public static string AsRelativePathString(this ItemModel itemModel)
        {
            if (itemModel.RelativePath == null)
                return itemModel.Name;

            return itemModel.RelativePath.AsPathString(x => x.Name) + "/" + itemModel.Name;
        }
    }
}