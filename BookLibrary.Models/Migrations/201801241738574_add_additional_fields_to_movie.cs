namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_additional_fields_to_movie : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movies", "RunningTime", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.Movies", "Publisher", c => c.String());
            AddColumn("dbo.Movies", "ProductGroup", c => c.String());
            AddColumn("dbo.Movies", "Manufacturer", c => c.String());
            AddColumn("dbo.Movies", "Genre", c => c.String());
            AddColumn("dbo.Movies", "Director", c => c.String());
            AddColumn("dbo.Movies", "AudienceRating", c => c.String());
            AddColumn("dbo.Movies", "IsAdultProduct", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Movies", "IsAdultProduct");
            DropColumn("dbo.Movies", "AudienceRating");
            DropColumn("dbo.Movies", "Director");
            DropColumn("dbo.Movies", "Genre");
            DropColumn("dbo.Movies", "Manufacturer");
            DropColumn("dbo.Movies", "ProductGroup");
            DropColumn("dbo.Movies", "Publisher");
            DropColumn("dbo.Movies", "RunningTime");
        }
    }
}
