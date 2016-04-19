using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OneNorth.FieldMigrator.Helpers;

namespace OneNorth.FieldMigrator.sitecore.admin.FieldMigrator
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void TestButton_OnClick(object sender, EventArgs e)
        {
            Migrator.Instance.Migrate();
        }
    }
}