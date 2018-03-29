namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_audio_to_library : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Albums",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EntryType = c.String(),
                        ArtistId = c.Guid(nullable: false),
                        NumberOfDiscs = c.Int(),
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Artists", t => t.ArtistId, cascadeDelete: true)
                .Index(t => t.ArtistId);
            
            CreateTable(
                "dbo.Artists",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CelebrityId = c.Guid(),
                        ArtistId = c.Guid(),
                        Name = c.String(),
                        IsGroup = c.Boolean(nullable: false),
                        DisplayImage = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Artists", t => t.ArtistId)
                .Index(t => t.ArtistId);
            
            CreateTable(
                "dbo.Tracks",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ArtistId = c.Guid(nullable: false),
                        AlbumId = c.Guid(nullable: false),
                        Name = c.String(),
                        DiscNumber = c.Int(nullable: false),
                        TrackNumber = c.Int(nullable: false),
                        Lyrics = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        ModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Albums", t => t.AlbumId, cascadeDelete: true)
                .ForeignKey("dbo.Artists", t => t.ArtistId, cascadeDelete: false)
                .Index(t => t.ArtistId)
                .Index(t => t.AlbumId);
            
            CreateTable(
                "dbo.TrackOfTheDays",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TrackId = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tracks", t => t.TrackId, cascadeDelete: true)
                .Index(t => t.TrackId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TrackOfTheDays", "TrackId", "dbo.Tracks");
            DropForeignKey("dbo.Tracks", "ArtistId", "dbo.Artists");
            DropForeignKey("dbo.Tracks", "AlbumId", "dbo.Albums");
            DropForeignKey("dbo.Artists", "ArtistId", "dbo.Artists");
            DropForeignKey("dbo.Albums", "ArtistId", "dbo.Artists");
            DropIndex("dbo.TrackOfTheDays", new[] { "TrackId" });
            DropIndex("dbo.Tracks", new[] { "AlbumId" });
            DropIndex("dbo.Tracks", new[] { "ArtistId" });
            DropIndex("dbo.Artists", new[] { "ArtistId" });
            DropIndex("dbo.Albums", new[] { "ArtistId" });
            DropTable("dbo.TrackOfTheDays");
            DropTable("dbo.Tracks");
            DropTable("dbo.Artists");
            DropTable("dbo.Albums");
        }
    }
}
