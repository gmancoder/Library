namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_image_to_magazine_issue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MagazineIssues", "ImageFileName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MagazineIssues", "ImageFileName");
        }
    }
}
