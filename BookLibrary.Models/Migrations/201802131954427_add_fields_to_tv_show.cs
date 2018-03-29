namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_fields_to_tv_show : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TVShows", "SortTitle", c => c.String());
            AddColumn("dbo.TVShows", "Stars", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.TVShows", "Stars");
            DropColumn("dbo.TVShows", "SortTitle");
        }
    }
}
