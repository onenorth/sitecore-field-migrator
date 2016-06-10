using System;
using System.Collections.Generic;
using OneNorth.FieldMigrator.Models;

namespace OneNorth.FieldMigrator.Helpers
{
    public interface IHardRockWebServiceProxy
    {
        byte[] MediaDownloadAttachment(Guid mediaItemId);
        List<FolderModel> GetFullPath(Guid itemId, string language, int version);
        void SetContextLanguage(string languageName);
        void TraverseTree(Guid rootId, Action<ItemModel> itemAction);
    }
}
