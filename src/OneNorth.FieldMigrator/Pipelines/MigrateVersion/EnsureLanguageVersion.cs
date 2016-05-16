using System.Collections.Generic;
using System.Xml;
using Sitecore.Data.Managers;
using Sitecore.Xml;

namespace OneNorth.FieldMigrator.Pipelines.MigrateVersion
{
    public class EnsureLanguageVersion : IMigrateVersionPipelineProcessor
    {
        private readonly Dictionary<string, string> _overrides = new Dictionary<string, string>();
        public virtual Dictionary<string, string> Overrides { get { return _overrides; } }
        protected virtual void AddOverride(XmlNode node)
        {
            var key = XmlUtil.GetAttribute("sourceLanguageName", node, "");
            var value = XmlUtil.GetAttribute("targetLanguageName", node, "");

            if (!_overrides.ContainsKey(key))
                _overrides.Add(key, value);
        }

        public virtual void Process(MigrateVersionPipelineArgs args)
        {
            if (args.Item == null ||
                args.Source == null)
                return;

            var item = args.Item;

            var languageName = (Overrides.ContainsKey(args.Source.Language)) ? Overrides[args.Source.Language] : args.Source.Language;

            // Ensure we are working with the correct language
            var language = LanguageManager.GetLanguage(languageName);
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