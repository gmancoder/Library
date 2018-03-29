namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_release_date_text : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MagazineIssues", "ReleaseDateText", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MagazineIssues", "ReleaseDateText");
        }
    }
}
