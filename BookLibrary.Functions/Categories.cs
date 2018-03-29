using BookLibrary.Models;
using BookLibrary.Models.AjaxModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using BookLibrary.Models.ServiceModels.Amazon;
using Humanizer;

namespace BookLibrary.Functions
{
    public static class Categories
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public static List<string> CategoryFields
        {
            get
            {
                return new List<String> { "Name", "CategoryId", "CreatedDate", "ModifiedDate" };
            }
        }
        public static List<string> ObjectToCategoryFields
        {
            get
            {
                return new List<string> { "BrowseNodeId", "ObjectId", "CategoryId", "ObjectType" };
            }
        }

        public static List<string> CategoryRequiredFields
        {
            get
            {
                return new List<String> { "Name" };
            }
        }
        public static List<string> ObjectToCategoryRequiredFields
        {
            get
            {
                return new List<string> { "ObjectId", "ObjectType", "CategoryId" };
            }
        }

        public static bool CategoryExists(string Name, Guid? CategoryId = null)
        {
            return db.Categories.Where(c => c.Name == Name && c.CategoryId == CategoryId).Count() > 0;
        }

        public static Category GetCategoryByName(string Name, Guid? CategoryId = null)
        {
            return db.Categories.Where(c => c.Name == Name && c.CategoryId == CategoryId).FirstOrDefault();
        }

        public static Category CreateCategory(string Name, Guid? CategoryId =null)
        {
            Category c = new Category();
            c.Id = Guid.NewGuid();
            c.CreatedDate = c.ModifiedDate = DateTime.Now;
            c.Name = Name;
            c.CategoryId = CategoryId;
            db.Categories.Add(c);
            db.SaveChanges();

            return c;
        }

        public static List<SelectListItem> DrawCategorySelectList(Guid? selectCategoryId)
        {
            List<Category> rootCategories = db.Categories.Where(c => c.CategoryId == null).OrderBy(c => c.Name).ToList();
            List<SelectListItem> selectItems = new List<SelectListItem>();
            selectItems.Add(new SelectListItem()
            {
                Text = "Root Category",
                Value = "",
                Selected = selectCategoryId.HasValue == false
            });
            GetChildCategoriesForSelectList(ref selectItems, rootCategories, 2, selectCategoryId);
            return selectItems;
        }
        private static void GetChildCategoriesForSelectList(ref List<SelectListItem> selectItems, List<Category> categories, int idx, Guid? selectCategoryId)
        {
            string dots = "";
            for(var i = 0; i < idx; i ++)
            {
                dots += ".";
            }

            foreach(Category category in categories)
            {
                selectItems.Add(new SelectListItem()
                {
                    Text = dots + category.Name,
                    Value = category.Id.ToString(),
                    Selected = category.Id == selectCategoryId
                });

                List<Category> childCategories = db.Categories.Where(c => c.CategoryId == category.Id).OrderBy(c => c.Name).ToList();
                GetChildCategoriesForSelectList(ref selectItems, childCategories, idx + 2, selectCategoryId);
            }
        }
        public static string GetCategoryHtml(List<Guid> categoryIds)
        {
            List<Category> rootCategories = db.Categories.Where(c => c.CategoryId == null).ToList();
            string Html = "<ul>";
            foreach(Category category in rootCategories)
            {
                Html += RecurseGetCategoryHtml(categoryIds, category);
                
            }
            Html += "</ul>";
            return Html;
        }
        private static string RecurseGetCategoryHtml(List<Guid> categoryIds, Category category)
        {
            string li_class = "";
            string a_class = "";
            if(categoryIds != null && categoryIds.Contains(category.Id))
            {
                li_class += " jstree-open ";
                a_class = " jstree-clicked ";
            }
            string Html = "<li class='" + li_class + "'><a class='" + a_class + "' href='/Categories/Details/" + category.Id + "' id='"+ category.Id + "'>" + category.Name + "</a>";

            List<Category> childCategories = db.Categories.Where(c => c.CategoryId == category.Id).ToList();
            foreach(Category childCategory in childCategories)
            {
                Html += "<ul>" + RecurseGetCategoryHtml(categoryIds, childCategory) + "</ul>";
            }
            Html += "</li>";

            return Html;
        }

        public static List<List<Category>> DrawBreadcrumbsForObject(Guid objectId)
        {
            List<ObjectToCategory> objectCategories = db.ObjectToCategories.Include(a => a.Category).Where(ac => ac.ObjectId == objectId).ToList();
            List<List<Category>> categoryStreams = new List<List<Category>>();
            foreach (ObjectToCategory ac in objectCategories)
            {
                if (ac.Category.CategoryId == null)
                {
                    List<Category> childCategories = FilterCategoriesToObject(db.Categories.Where(c => c.CategoryId == ac.CategoryId).ToList(), objectId);
                    SpawnCategoryStreams(ref categoryStreams, new List<Category>() { ac.Category }, childCategories, objectId);
                }
            }

            return categoryStreams;

        }

        private static void SpawnCategoryStreams(ref List<List<Category>> streams, List<Category> currentStream, List<Category> categories, Guid objectId)
        {
            if(categories.Count == 0)
            {
                streams.Add(currentStream);
            }
            else if(categories.Count() == 1)
            {
                Category c = categories[0];
                currentStream.Add(c);
                List<Category> childCategories = FilterCategoriesToObject(db.Categories.Where(ca => ca.CategoryId == c.Id).ToList(), objectId);
                SpawnCategoryStreams(ref streams, currentStream, childCategories, objectId);
            }
            else
            {
                foreach(Category c in categories)
                {
                    List<Category> newStream = new List<Category>();
                    foreach(Category category in currentStream)
                    {
                        newStream.Add(category);
                    }
                    //currentStream.CopyTo(newStream.ToArray());
                    newStream.Add(c);
                    List<Category> childCategories = FilterCategoriesToObject(db.Categories.Where(ca => ca.CategoryId == c.Id).ToList(), objectId);
                    SpawnCategoryStreams(ref streams, newStream, childCategories, objectId);
                }
            }
        }

        private static List<Category> FilterCategoriesToObject(List<Category> categories, Guid objectId)
        {
            List<Category> newCategories = new List<Category>();
            foreach (Category c in categories)
            {
                if (db.ObjectToCategories.Where(ac => ac.CategoryId == c.Id && ac.ObjectId == objectId).Count() > 0)
                {
                    newCategories.Add(c);
                }
            }
            return newCategories;
        }

        public static List<CategoryResult> GetResults(Guid? categoryId)
        {
            List<CategoryResult> results = new List<CategoryResult>();
            List<Category> categories = new List<Category>();
            List<ObjectToCategory> objectsInCategory = new List<ObjectToCategory>();
            if(categoryId.HasValue && categoryId.Value == Guid.NewGuid())
            {
                categoryId = null;
            }
            categories = db.Categories.Where(c => c.CategoryId == categoryId).OrderBy(o => o.Name).ToList();
            if(categoryId.HasValue)
            {
                objectsInCategory = db.ObjectToCategories.Where(a => a.CategoryId == categoryId.Value).ToList();
            }    
            
            foreach(Category category in categories)
            {
                results.Add(new CategoryResult
                {
                    Id = category.Id,
                    Name = category.Name,
                    ResultType = "Category"
                });
            }

            foreach(ObjectToCategory objectCategory in objectsInCategory)
            {
                string Name;
                string Image;
                Core.Core.GetObjectDetails(objectCategory.ObjectId, objectCategory.ObjectType, out Name, out Image);
                results.Add(new CategoryResult
                {
                    Id = objectCategory.ObjectId,
                    Name = Name,
                    Image = objectCategory.ObjectType == "Magazine" ? Magazines.GetMagazineThumb(objectCategory.ObjectId) : Image,
                    ResultType = objectCategory.ObjectType.Pluralize()
                });
            }

            return results.OrderBy(r => r.Name).ToList();
        }

        public static bool PopulateCategories<T>(List<BrowseNode> nodes, T obj)
        {
            Guid Id;
            string Type;
            string Name;
            string Image;
            if (!Core.Core.DetermineT<T>(obj, out Id, out Type, out Name, out Image))
            {
                return false;
            }
            foreach (BrowseNode node in nodes)
            {
                RecursePopulateCategories(node, Id, Type);
            }
            return true;
        }

        private static Guid? RecursePopulateCategories(BrowseNode node, Guid Id, string Type)
        {
            bool newCategory = false;
            if (node != null)
            {
                Guid? parentId = null;
                if(node.Ancestors != null && node.Ancestors.Count() > 0)
                    parentId = RecursePopulateCategories(node.Ancestors[0], Id, Type);

                Category category = GetCategoryByName(node.Name, parentId);
                if (category == null)
                {
                    category = new Category();
                    category.Id = Guid.NewGuid();
                    category.Name = node.Name;
                    category.CreatedDate = DateTime.Now;
                    category.ModifiedDate = DateTime.Now;
                    newCategory = true;
                }
                
                if (newCategory)
                {
                    if (parentId.HasValue)
                    {
                        category.CategoryId = parentId.Value;
                    }
                    db.Categories.Add(category);
                    db.SaveChanges();
                }
                ObjectToCategory objectCategory = db.ObjectToCategories.Where(oc => oc.ObjectId == Id && oc.ObjectType == Type && oc.CategoryId == category.Id).FirstOrDefault();
                if (objectCategory == null)
                {
                    objectCategory = new ObjectToCategory();
                    //objectCategory.Id = Guid.NewGuid();
                    objectCategory.BrowseNodeId = Convert.ToInt64(node.BrowseNodeId);
                    objectCategory.ObjectType = Type;
                    objectCategory.ObjectId = Id;
                    objectCategory.CategoryId = category.Id;
                    db.ObjectToCategories.Add(objectCategory);
                    db.SaveChanges();
                }

                return category.Id;
            }
            return null;
        }

        public static BrowseNode CategoryPathToBrowseNodeTree(List<string> path)
        {
            BrowseNode newNode = new BrowseNode();
            string Name = path[0];
            newNode.Name = Name;
            path.Remove(Name);
            newNode.Ancestors = new List<BrowseNode>();
            BrowseNode ancestorTree = AncestorToTree(path);
            if (ancestorTree == null)
            {
                newNode.IsCategoryRoot = true;
            }
            else
            {
                newNode.Ancestors.Add(ancestorTree);
            }
            return newNode;
        }

        private static BrowseNode AncestorToTree(List<string> path)
        {
            if(path.Count() > 0)
            {
                BrowseNode ancestor = new BrowseNode();
                string Name = path[0];
                ancestor.Name = Name;
                path.Remove(Name);
                ancestor.Ancestors = new List<BrowseNode>();
                BrowseNode ancestorTree = AncestorToTree(path);
                if (ancestorTree == null)
                {
                    ancestor.IsCategoryRoot = true;
                }
                else
                {
                    ancestor.Ancestors.Add(ancestorTree);
                }
                return ancestor;
            }
            return null;
        } 

        public static bool Cleanup(string type, Guid id)
        {
            List<ObjectToCategory> categories = db.ObjectToCategories.Where(o => o.ObjectType == type && o.ObjectId == id).ToList();
            foreach(ObjectToCategory category in categories)
            {
                db.ObjectToCategories.Remove(category);
                db.SaveChanges();
            }
            return true;
        }
    }
}
