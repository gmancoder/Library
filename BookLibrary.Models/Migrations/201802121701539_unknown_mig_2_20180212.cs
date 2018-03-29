namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class unknown_mig_2_20180212 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Authors", "PersonId", "dbo.People");
            DropIndex("dbo.Authors", new[] { "PersonId" });
            AlterColumn("dbo.Authors", "PersonId", c => c.Guid());
            CreateIndex("dbo.Authors", "PersonId");
            AddForeignKey("dbo.Authors", "PersonId", "dbo.People", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Authors", "PersonId", "dbo.People");
            DropIndex("dbo.Authors", new[] { "PersonId" });
            AlterColumn("dbo.Authors", "PersonId", c => c.Guid(nullable: false));
            CreateIndex("dbo.Authors", "PersonId");
            AddForeignKey("dbo.Authors", "PersonId", "dbo.People", "Id", cascadeDelete: true);
        }
    }
}
