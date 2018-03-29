namespace BookLibrary.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class change_object_category_key_to_int_step_1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ObjectCategories", "Id2", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ObjectCategories", "Id2");
        }
    }
}
