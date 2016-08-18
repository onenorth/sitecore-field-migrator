using System;

namespace OneNorth.FieldMigrator.sitecore.admin.FieldMigrator
{
    public partial class Default : Sitecore.sitecore.admin.AdminPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void MigrateButton_OnClick(object sender, EventArgs e)
        {
            Migrator.Instance.Migrate();
        }

        protected override void OnInit(EventArgs e)
        {
            base.CheckSecurity(true); //Required!
            base.OnInit(e);
        }
    }
}