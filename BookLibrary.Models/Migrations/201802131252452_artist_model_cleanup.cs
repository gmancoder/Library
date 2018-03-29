namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class artist_model_cleanup : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Artists", "CelebrityId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Artists", "CelebrityId", c => c.Guid());
        }
    }
}
