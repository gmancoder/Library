namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class undefined_migration_20180125 : DbMigration
    {
        public override void Up()
        {
            //DropForeignKey("dbo.MovieStars", "Movie_Id", "dbo.Movies");
            //DropIndex("dbo.MovieStars", new[] { "Movie_Id" });
            //DropColumn("dbo.MovieToMovieStars", "MovieId");
            //RenameColumn(table: "dbo.MovieToMovieStars", name: "Movie_Id", newName: "MovieId");
            //AddForeignKey("dbo.MovieToMovieStars", "MovieId", "dbo.Movies", "Id", cascadeDelete: true);
            //DropColumn("dbo.MovieStars", "Movie_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MovieStars", "Movie_Id", c => c.Guid());
            DropForeignKey("dbo.MovieToMovieStars", "MovieId", "dbo.Movies");
            RenameColumn(table: "dbo.MovieToMovieStars", name: "MovieId", newName: "Movie_Id");
            AddColumn("dbo.MovieToMovieStars", "MovieId", c => c.Guid(nullable: false));
            CreateIndex("dbo.MovieStars", "Movie_Id");
            AddForeignKey("dbo.MovieStars", "Movie_Id", "dbo.Movies", "Id");
        }
    }
}
