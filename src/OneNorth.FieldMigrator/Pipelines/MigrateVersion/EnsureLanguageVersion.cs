
using Sitecore.Data.Managers;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class EnsureLanguageVersion : IMigrateVersionPipelineProcessor
    {
        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Item == null ||
                args.Source == null)
                return;

            var item = args.Item;

            // Ensure we are working with the correct language
            var language = LanguageManager.GetLanguage(args.Source.Language);
            item = (item.Language == language) ? item : item.Database.GetItem(item.ID, language);

            // Create version if it does not exist
            if (item.Versions.Count == 0)
            {
                var disableFiltering = Sitecore.Context.Site.DisableFiltering;
                try
                {
                    Sitecore.Context.Site.DisableFiltering = true;
                    item = item.Versions.AddVersion();
                }
                finally
                {
                    Sitecore.Context.Site.DisableFiltering = disableFiltering;
                }
            }

            args.Item = item;
        }
    }
}