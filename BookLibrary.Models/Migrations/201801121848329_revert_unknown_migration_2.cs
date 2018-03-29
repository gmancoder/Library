namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class revert_unknown_migration_2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Magazines", "ImageFileName");
            DropColumn("dbo.Magazines", "Discriminator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Magazines", "Discriminator", c => c.String(nullable: false, maxLength: 128));
            AddColumn("dbo.Magazines", "ImageFileName", c => c.String());
        }
    }
}
