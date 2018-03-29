namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_manually_added_to_movie_star : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MovieStars", "ManuallyAdded", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MovieStars", "ManuallyAdded");
        }
    }
}
