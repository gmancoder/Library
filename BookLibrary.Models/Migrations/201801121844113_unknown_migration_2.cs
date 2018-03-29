namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class unknown_migration_2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Magazines", "ImageFileName", c => c.String());
            AddColumn("dbo.Magazines", "Discriminator", c => c.String(nullable: false, maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Magazines", "Discriminator");
            DropColumn("dbo.Magazines", "ImageFileName");
        }
    }
}
