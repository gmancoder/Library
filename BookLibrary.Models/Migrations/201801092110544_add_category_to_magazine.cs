namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_category_to_magazine : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Magazines", "CategoryId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Magazines", "CategoryId");
        }
    }
}
