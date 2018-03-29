using BookLibrary.Models;
using BookLibrary.Models.Migrations;
using BookLibrary.Models.ServiceModels.Amazon;
using BookLibrary.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BookLibrary.Functions.Core
{
    public static class Core
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static string ApplySortTitle(string Title)
        {
            if (Title.StartsWith("The "))
            {
                return Title.Substring(4);
            }
            else if (Title.StartsWith("A "))
            {
                return Title.Substring(2);
            }
            else if (Title.StartsWith("An "))
            {
                return Title.Substring(3);
            }
            else
            {
                return Title;
            }
        }
        public static string MonthName(int month)
        {
            switch(month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
                default:
                    return "";
            }
        }
        public static bool ParseAmazonXml(string amazonXml, out ItemLookupResponse response, out Exception ex)
        {
            ex = null;
            response = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ItemLookupResponse));
                response = (ItemLookupResponse)serializer.Deserialize(GenerateStreamFromString(amazonXml));
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }
        public static bool DetermineT<T>(T obj, out Guid Id, out string Type, out string Name, out string Image)
        {
            if(typeof(T) == typeof(Book))
            {
                Book b = (Book)Convert.ChangeType(obj, typeof(Book));
                Id = b.Id;
                Type = "Book";
                Name = b.Title;
                Image = b.ImageFileName;
            }
            else if (typeof(T) == typeof(Magazine))
            {
                Magazine b = (Magazine)Convert.ChangeType(obj, typeof(Magazine));
                Id = b.Id;
                Type = "Magazine";
                Name = b.Title;
                Image = "";
            }
            else if(typeof(T) == typeof(Album))
            {
                Album a = (Album)Convert.ChangeType(obj, typeof(Album));
                Id = a.Id;
                Type = "Album";
                Name = a.Title;
                Image = a.ImageFileName;
            }
            else if(typeof(T) == typeof(Movie))
            {
                Movie m = (Movie)Convert.ChangeType(obj, typeof(Movie));
                Id = m.Id;
                Type = "Movie";
                Name = m.Title;
                Image = m.ImageFileName;
            }
            else
            {
                Id = Guid.Empty;
                Type = "Unknown";
                Name = "Unknown";
                Image = "";
                return false;
            }
            return true;
        }

        public static bool GetObjectDetails(Guid objectId, string ObjectType, out string Name, out string Image)
        {
            Name = Image = "";
            switch(ObjectType)
            {
                case "Book":
                    Book b = db.Books.Find(objectId);
                    Name = b.Title;
                    Image = "/Content/Images/books/" + b.ImageFileName;
                    break;
                case "Magazine":
                    Magazine m = db.Magazines.Find(objectId);
                    Name = m.Title;
                    Image = "";
                    break;
                case "Album":
                    Album a = db.Albums.Find(objectId);
                    Name = a.Title;
                    Image = "/Content/Images/albums/" + a.ImageFileName;
                    break;
                case "Movie":
                    Movie mv = db.Movies.Find(objectId);
                    Name = mv.Title;
                    Image = "/Content/Images/movies/" + mv.ImageFileName;
                    break;
                default:
                    return false;
            }
            return true;
        }

        public static string PostRemoteData(string url, string postData = "", string token = "", string method = "POST")
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            

            request.Method = method;
            if (postData != "")
            {
                var data = Encoding.ASCII.GetBytes(postData);
                if (postData.StartsWith("{"))
                {
                    request.ContentType = "application/json";
                }
                else if (postData.StartsWith("<"))
                {
                    request.ContentType = "text/xml";
                }
                else if (postData != "")
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                }
                request.ContentLength = data.Length;

                if (token != "")
                {
                    request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
                }

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            else if (token != "")
            {
                request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            }

            var response = (HttpWebResponse)request.GetResponse(); //fails on this line

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return responseString;
        }

        public static int CalculateSkip(int itemsPerPage, int page)
        {
            return (page - 1) * itemsPerPage;
        }

        public static int Pages(int resultCount, int itemsPerPage)
        {
            int pages = resultCount / itemsPerPage;
            if (pages == 0)
            {
                pages = 1;
            }
            return pages;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static bool DownloadFileFromUrl(string Url, string outputName, out Exception ex)
        {
            ex = null;
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(Url), outputName);
                }
                return true;
            }
            catch(Exception e)
            {
                ex = e;
                return false;
            }
        }

        public static List<LibraryObject> FlattenObjects(List<List<LibraryObject>> objectLists, int take = 0, bool latest = true)
        {
            List<LibraryObject> allObjects = new List<LibraryObject>();
            foreach(List<LibraryObject> objectList in objectLists)
            {
                foreach(LibraryObject obj in objectList)
                {
                    allObjects.Add(obj);
                }
            }

            if(take == 0)
            {
                if (latest)
                {
                    return allObjects.OrderByDescending(o => o.CreatedDate).ToList();
                }
                return allObjects;
            }
            else if(latest)
            {
                return allObjects.OrderByDescending(o => o.CreatedDate).Take(take).ToList();
            }
            return allObjects.Take(take).ToList();
        }

        public static string ParseException(Exception ex)
        {
            if(ex.InnerException != null)
            {
                return ParseException(ex.InnerException);
            }
            return ex.Message;
        }
    }
}
