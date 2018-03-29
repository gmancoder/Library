namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_starring_string_to_movie : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Movies", "Starring", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Movies", "Starring");
        }
    }
}
