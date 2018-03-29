namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_tv_shows : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TVShows",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Title = c.String(),
                        TVDbId = c.Int(nullable: false),
                        TVDbResponse = c.String(),
                        DisplayImage = c.String(),
                        Url = c.String(),
                        FirstAired = c.String(),
                        Genres = c.String(),
                        Network = c.String(),
                        Overview = c.String(),
                        Rating = c.String(),
                        Runtime = c.String(),
                        Status = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TVShowToTVStars",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TVShowId = c.Guid(nullable: false),
                        TVStarId = c.Guid(nullable: false),
                        ManuallyAdded = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TVShows", t => t.TVShowId, cascadeDelete: true)
                .ForeignKey("dbo.TVStars", t => t.TVStarId, cascadeDelete: true)
                .Index(t => t.TVShowId)
                .Index(t => t.TVStarId);
            
            CreateTable(
                "dbo.TVStars",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PersonId = c.Guid(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.People", t => t.PersonId, cascadeDelete: true)
                .Index(t => t.PersonId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TVShowToTVStars", "TVStarId", "dbo.TVStars");
            DropForeignKey("dbo.TVStars", "PersonId", "dbo.People");
            DropForeignKey("dbo.TVShowToTVStars", "TVShowId", "dbo.TVShows");
            DropIndex("dbo.TVStars", new[] { "PersonId" });
            DropIndex("dbo.TVShowToTVStars", new[] { "TVStarId" });
            DropIndex("dbo.TVShowToTVStars", new[] { "TVShowId" });
            DropTable("dbo.TVStars");
            DropTable("dbo.TVShowToTVStars");
            DropTable("dbo.TVShows");
        }
    }
}
