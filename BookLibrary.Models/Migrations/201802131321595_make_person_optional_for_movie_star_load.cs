namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class make_person_optional_for_movie_star_load : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MovieStars", "PersonId", "dbo.People");
            DropIndex("dbo.MovieStars", new[] { "PersonId" });
            AlterColumn("dbo.MovieStars", "PersonId", c => c.Guid());
            CreateIndex("dbo.MovieStars", "PersonId");
            AddForeignKey("dbo.MovieStars", "PersonId", "dbo.People", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MovieStars", "PersonId", "dbo.People");
            DropIndex("dbo.MovieStars", new[] { "PersonId" });
            AlterColumn("dbo.MovieStars", "PersonId", c => c.Guid(nullable: false));
            CreateIndex("dbo.MovieStars", "PersonId");
            AddForeignKey("dbo.MovieStars", "PersonId", "dbo.People", "Id", cascadeDelete: true);
        }
    }
}
