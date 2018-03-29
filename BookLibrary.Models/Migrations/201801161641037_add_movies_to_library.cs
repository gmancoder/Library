namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_movies_to_library : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Movies",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EntryType = c.String(),
                        ASIN = c.String(),
                        EAN = c.String(),
                        UPC = c.String(),
                        Title = c.String(),
                        ReleaseDate = c.String(),
                        Binding = c.String(),
                        ImageFileName = c.String(),
                        Url = c.String(),
                        AmazonResponse = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MovieStars",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CelebrityId = c.Guid(),
                        MovieId = c.Guid(nullable: false),
                        Name = c.String(),
                        Image = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Movies", t => t.MovieId, cascadeDelete: true)
                .Index(t => t.MovieId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MovieStars", "MovieId", "dbo.Movies");
            DropIndex("dbo.MovieStars", new[] { "MovieId" });
            DropTable("dbo.MovieStars");
            DropTable("dbo.Movies");
        }
    }
}
