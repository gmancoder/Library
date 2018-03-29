namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migration_20180110_1 : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Magazines", "CategoryId");
            AddForeignKey("dbo.Magazines", "CategoryId", "dbo.Categories", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Magazines", "CategoryId", "dbo.Categories");
            DropIndex("dbo.Magazines", new[] { "CategoryId" });
        }
    }
}
