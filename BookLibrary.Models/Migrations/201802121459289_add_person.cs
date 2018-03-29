namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_person : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.People",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CelebrityId = c.Guid(),
                        Name = c.String(),
                        SortName = c.String(),
                        DisplayImage = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Artists", "PersonId", c => c.Guid());
            AddColumn("dbo.Authors", "PersonId", c => c.Guid());
            AddColumn("dbo.MovieStars", "PersonId", c => c.Guid());
            CreateIndex("dbo.Artists", "PersonId");
            CreateIndex("dbo.Authors", "PersonId");
            CreateIndex("dbo.MovieStars", "PersonId");
            AddForeignKey("dbo.Artists", "PersonId", "dbo.People", "Id");
            AddForeignKey("dbo.Authors", "PersonId", "dbo.People", "Id");
            AddForeignKey("dbo.MovieStars", "PersonId", "dbo.People", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MovieStars", "PersonId", "dbo.People");
            DropForeignKey("dbo.Authors", "PersonId", "dbo.People");
            DropForeignKey("dbo.Artists", "PersonId", "dbo.People");
            DropIndex("dbo.MovieStars", new[] { "PersonId" });
            DropIndex("dbo.Authors", new[] { "PersonId" });
            DropIndex("dbo.Artists", new[] { "PersonId" });
            DropColumn("dbo.MovieStars", "PersonId");
            DropColumn("dbo.Authors", "PersonId");
            DropColumn("dbo.Artists", "PersonId");
            DropTable("dbo.People");
        }
    }
}
