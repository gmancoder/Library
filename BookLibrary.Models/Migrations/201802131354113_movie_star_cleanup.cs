namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class movie_star_cleanup : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MovieStars", "PersonId", "dbo.People");
            DropIndex("dbo.MovieStars", new[] { "PersonId" });
            AlterColumn("dbo.MovieStars", "PersonId", c => c.Guid(nullable: false));
            CreateIndex("dbo.MovieStars", "PersonId");
            AddForeignKey("dbo.MovieStars", "PersonId", "dbo.People", "Id", cascadeDelete: true);
            DropColumn("dbo.MovieStars", "CelebrityId");
            DropColumn("dbo.MovieStars", "Name");
            DropColumn("dbo.MovieStars", "Image");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MovieStars", "Image", c => c.String());
            AddColumn("dbo.MovieStars", "Name", c => c.String());
            AddColumn("dbo.MovieStars", "CelebrityId", c => c.Guid());
            DropForeignKey("dbo.MovieStars", "PersonId", "dbo.People");
            DropIndex("dbo.MovieStars", new[] { "PersonId" });
            AlterColumn("dbo.MovieStars", "PersonId", c => c.Guid());
            CreateIndex("dbo.MovieStars", "PersonId");
            AddForeignKey("dbo.MovieStars", "PersonId", "dbo.People", "Id");
        }
    }
}
