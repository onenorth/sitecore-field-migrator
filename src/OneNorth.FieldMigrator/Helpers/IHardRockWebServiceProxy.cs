using System;
using System.Collections.Generic;
using OneNorth.FieldMigrator.Models;

namespace OneNorth.FieldMigrator.Helpers
{
    public interface IHardRockWebServiceProxy
    {
        IEnumerable<ChildModel> GetChildren(Guid parentId);
        List<FolderModel> GetFullPath(Guid itemId, string language, int version);
        ItemModel GetItem(Guid id, bool deep = true);
        void TraverseTree(Guid rootId, Action<ItemModel> itemAction, FolderModel[] relativePath = null, bool? hasChildren = null);
    }
}
