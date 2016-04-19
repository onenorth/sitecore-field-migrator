using System;
using System.Collections.Generic;
using OneNorth.FieldMigrator.Models;

namespace OneNorth.FieldMigrator.Helpers
{
    public interface IHardRockWebServiceProxy
    {
        IEnumerable<ChildModel> GetChildren(Guid parentId);
        ItemModel GetItem(Guid id, bool deep = true);
    }
}
