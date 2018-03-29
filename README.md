# Library
This site catalogs all of my books, magazines, CDs, DVDs, and favorite TV Shows. Here is a breakdown of the projects contained within this solution. The solution initially was geared toward my books but then I combined it with the other media types.

## BookLibrary
Main MVC Web application using HTML, CSS, and JavaScript. I use the Bootstrap framework for responsive design and other design elements such as modals and navigation. MVC 5 is used.

## BookLibrary.Functions.Core
These are basic methods used throughout the project such as posting remote data to services, logging, etc.

## BookLibrary.Functions
These are object specific functions.

## BookLibrary.Models
These are all the models used in this solution. I used code-first migrations with EntityFramework to create them.

## BookLibrary.Services
This solution interacts with my Celebrity Central project (http://celebritycentral.gmancoder.com:8081), my Gallery project (http://gallery.gmancoder.com), and my PDF Library project (http://pdflib.gmancoder.com). It also interacts with the Amazon Product API to get details for Books, CDs, and DVDs. Also interacts with the TVDb API to get details on the TV Shows.

## BookLibrary.WebAPI
This is the API endpoint for the website. All sites I develop have an API component. This allows for external syncing and querying of the Library.
