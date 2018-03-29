namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class make_person_required_on_author_remove_name : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Authors", "PersonId", "dbo.People");
            DropIndex("dbo.Authors", new[] { "PersonId" });
            AlterColumn("dbo.Authors", "PersonId", c => c.Guid(nullable: false));
            CreateIndex("dbo.Authors", "PersonId");
            AddForeignKey("dbo.Authors", "PersonId", "dbo.People", "Id", cascadeDelete: true);
            DropColumn("dbo.Authors", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Authors", "Name", c => c.String());
            DropForeignKey("dbo.Authors", "PersonId", "dbo.People");
            DropIndex("dbo.Authors", new[] { "PersonId" });
            AlterColumn("dbo.Authors", "PersonId", c => c.Guid());
            CreateIndex("dbo.Authors", "PersonId");
            AddForeignKey("dbo.Authors", "PersonId", "dbo.People", "Id");
        }
    }
}
