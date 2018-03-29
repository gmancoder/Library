namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_movie_to_movie_star : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MovieStars", "MovieId", "dbo.Movies");
            DropIndex("dbo.MovieStars", new[] { "MovieId" });
            //RenameColumn(table: "dbo.MovieStars", name: "MovieId", newName: "Movie_Id");
            CreateTable(
                "dbo.MovieToMovieStars",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MovieId = c.Guid(nullable: false),
                        MovieStarId = c.Guid(nullable: false),
                        ManuallyAdded = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Movies", t => t.MovieId, cascadeDelete: true)
                .ForeignKey("dbo.MovieStars", t => t.MovieStarId, cascadeDelete: true)
                .Index(t => t.MovieId)
                .Index(t => t.MovieStarId);
            
            //AlterColumn("dbo.MovieStars", "Movie_Id", c => c.Guid());
            //CreateIndex("dbo.MovieStars", "Movie_Id");
            //AddForeignKey("dbo.MovieStars", "Movie_Id", "dbo.Movies", "Id");
            DropColumn("dbo.MovieStars", "ManuallyAdded");
            DropColumn("dbo.MovieStars", "MovieId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MovieStars", "ManuallyAdded", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.MovieStars", "Movie_Id", "dbo.Movies");
            DropForeignKey("dbo.MovieToMovieStars", "MovieStarId", "dbo.MovieStars");
            DropForeignKey("dbo.MovieToMovieStars", "MovieId", "dbo.Movies");
            DropIndex("dbo.MovieToMovieStars", new[] { "MovieStarId" });
            DropIndex("dbo.MovieToMovieStars", new[] { "MovieId" });
            DropIndex("dbo.MovieStars", new[] { "Movie_Id" });
            AlterColumn("dbo.MovieStars", "Movie_Id", c => c.Guid(nullable: false));
            DropTable("dbo.MovieToMovieStars");
            RenameColumn(table: "dbo.MovieStars", name: "Movie_Id", newName: "MovieId");
            CreateIndex("dbo.MovieStars", "MovieId");
            AddForeignKey("dbo.MovieStars", "MovieId", "dbo.Movies", "Id", cascadeDelete: true);
        }
    }
}
