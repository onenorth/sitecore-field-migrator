using System;

namespace OneNorth.FieldMigrator.sitecore.admin.FieldMigrator
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void MigrateButton_OnClick(object sender, EventArgs e)
        {
            Migrator.Instance.Migrate();
        }
    }
}