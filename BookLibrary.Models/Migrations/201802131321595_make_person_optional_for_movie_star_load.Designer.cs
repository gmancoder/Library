// <auto-generated />
namespace BookLibrary.Models.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.1.3-40302")]
    public sealed partial class make_person_optional_for_movie_star_load : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(make_person_optional_for_movie_star_load));
        
        string IMigrationMetadata.Id
        {
            get { return "201802131321595_make_person_optional_for_movie_star_load"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}