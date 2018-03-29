namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class unknown_mig_1_20180212 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Authors", "CelebrityId");
            DropColumn("dbo.Authors", "DisplayImage");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Authors", "DisplayImage", c => c.String());
            AddColumn("dbo.Authors", "CelebrityId", c => c.Guid());
        }
    }
}
