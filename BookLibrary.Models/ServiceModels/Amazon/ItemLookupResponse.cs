using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BookLibrary.Models.ServiceModels.Amazon
{
    [Serializable()]
    [XmlRoot(ElementName = "ItemLookupResponse", Namespace = "http://webservices.amazon.com/AWSECommerceService/2011-08-01")]
    public class ItemLookupResponse
    {
        [XmlElement("OperationRequest")]
        public OperationRequest OperationRequest { get; set; }
        [XmlElement("Items")]
        public Items Items { get; set; }
    }

    [Serializable()]
    public class OperationRequest
    {
        [XmlElement("RequestId")]
        public Guid RequestId;
        [XmlArray("Arguments")]
        [XmlArrayItem("Argument", typeof(Argument))]
        public List<Argument> Arguments { get; set; }
        [XmlElement("RequestProcessingTime")]
        public Decimal RequestProcessingTime { get; set; }
    }

    [Serializable()]
    public class Argument
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("Value")]
        public string Value { get; set; }
    }

    [Serializable()]
    public class Items
    {
        [XmlElement("Request")]
        public Request Request { get; set; }
        [XmlElement("Item")]
        public Item Item { get; set; }
    }

    [Serializable()]
    public class Item
    {
        [XmlElement("ASIN")]
        public string ASIN { get; set; }
        [XmlElement("DetailPageURL")]
        public string DetailPageURL { get; set; }
        [XmlArray("ItemLinks")]
        [XmlArrayItem("ItemLink", typeof(ItemLink))]
        public List<ItemLink> ItemLinks { get; set; }
        [XmlElement("SalesRank")]
        public int SalesRank { get; set; }
        [XmlElement("SmallImage")]
        public ImageItem SmallImage { get; set; }
        [XmlElement("MediumImage")]
        public ImageItem MediumImage { get; set; }
        [XmlElement("LargeImage")]
        public ImageItem LargeImage { get; set; }
        [XmlArray("ImageSets")]
        [XmlArrayItem("ImageSet", typeof(ImageSet))]
        public List<ImageSet> ImageSets { get; set; }
        [XmlElement("ItemAttributes")]
        public ItemAttributes ItemAttributes { get; set; }
        [XmlElement("OfferSummary")]
        public OfferSummary OfferSummary { get; set; }
        [XmlElement("Offers")]
        public Offers Offers { get; set; }
        [XmlElement("CustomerReviews")]
        public CustomerReviews CustomerReviews { get; set; }
        [XmlArray("EditorialReviews")]
        [XmlArrayItem("EditorialReview", typeof(EditorialReview))]
        public List<EditorialReview> EditorialReviews { get; set; }
        [XmlArray("SimilarProducts")]
        [XmlArrayItem("SimilarProduct", typeof(SimilarProduct))]
        public List<SimilarProduct> SimilarProducts { get; set; }
        [XmlArray("Tracks")]
        [XmlArrayItem("Disc", typeof(Disc))]
        public List<Disc> Discs { get; set; }
        [XmlArray("BrowseNodes")]
        [XmlArrayItem("BrowseNode", typeof(BrowseNode))]
        public List<BrowseNode> BrowseNodes { get; set; }
    }

    [Serializable()]
    public class EditorialReview
    {
        [XmlElement("Source")]
        public string Source { get; set; }
        [XmlElement("Content")]
        public string Content { get; set; }
        [XmlElement("IsLinkSuppressed")]
        public bool? IsLinkSuppressed { get; set; }
    }

    [Serializable()]
    public class BrowseNode
    {
        [XmlElement("BrowseNodeId")]
        public string BrowseNodeId { get; set; }
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlArray("Ancestors")]
        [XmlArrayItem("BrowseNode", typeof(BrowseNode))]
        public List<BrowseNode> Ancestors { get; set; }
        [XmlElement("IsCategoryRoot")]
        public bool? IsCategoryRoot { get; set; }
        [XmlArray("Children")]
        [XmlArrayItem("BrowseNode", typeof(BrowseNode))]
        public List<BrowseNode> Children { get; set; }
    }

    [Serializable()]
    public class Disc
    {
        [XmlAttribute("Number")]
        public int Number { get; set; }
        
        [XmlElement("Track")]
        public List<AmazonTrack> TrackList { get; set; }
    }

    [Serializable()]
    public class AmazonTrack
    {
        [XmlAttribute("Number")]
        public int Number { get; set; }
        [XmlText]
        public string Value { get; set; }
    }

    [Serializable()]
    public class SimilarProduct
    {
        [XmlElement("ASIN")]
        public string ASIN { get; set; }
        [XmlElement("Title")]
        public string Title { get; set; }
    }

    [Serializable()]
    public class CustomerReviews
    {
        [XmlElement("IFrameURL")]
        public string IFrameURL { get; set; }
        [XmlElement("HasReviews")]
        public bool? HasReviews { get; set; }
    }

    [Serializable()]
    public class Offers
    {
        [XmlElement("TotalOffers")]
        public int? TotalOffers { get; set; }
        [XmlElement("TotalOfferPages")]
        public int? TotalOfferPages { get; set; }
        [XmlElement("MoreOffersUrl")]
        public string MoreOffersUrl { get; set; }
        [XmlElement("Offer")]
        public List<Offer> OfferList { get; set; }
    }

    [Serializable()]
    public class Offer
    {
        [XmlElement("OfferAttributes")]
        public OfferAttributes OfferAttributes { get; set; }
        [XmlElement("OfferListing")]
        public OfferListing OfferListing { get; set; }
    }

    [Serializable()]
    public class OfferAttributes
    {
        [XmlElement("Condition")]
        public string Condition { get; set; }
    }

    [Serializable()]
    public class OfferListing
    {
        [XmlElement("OfferListingId")]
        public string OfferListingId { get; set; }
        [XmlElement("Price")]
        public Price Price { get; set; }
        [XmlElement("Availability")]
        public string Availability { get; set; }
        [XmlElement("AvailabilityAttributes")]
        public AvailabilityAttributes AvailabilityAttributes { get; set; }
        [XmlElement("IsEligibleForSuperSaverShipping")]
        public bool? IsEligibleForSuperSaverShipping { get; set; }
        [XmlElement("IsEligibleForPrime")]
        public bool IsEligibleForPrime { get; set; }
    }

    [Serializable()]
    public class AvailabilityAttributes
    {
        [XmlElement("AvailabilityType")]
        public string AvailabilityType { get; set; }
        [XmlElement("MinimumHours")]
        public int? MinimumHours { get; set; }
        [XmlElement("MaximumHours")]
        public int? MaximumHours { get; set; }
    }

    [Serializable()]
    public class OfferSummary
    {
        [XmlElement("LowestNewPrice")]
        public Price LowestNewPrice { get; set; }
        [XmlElement("LowestUsedPrice")]
        public Price LowestUsedPrice { get; set; }
        [XmlElement("TotalNew")]
        public int TotalNew { get; set; }
        [XmlElement("TotalUsed")]
        public int TotalUsed { get; set; }
        [XmlElement("TotalCollectable")]
        public int TotalCollectable { get; set; }
        [XmlElement("TotalRefurbished")]
        public int TotalRefurbished { get; set; }
    }

    [Serializable]
    public class Price
    {
        [XmlElement("Amount")]
        public int Amount { get; set; }
        [XmlElement("CurrencyCode")]
        public string CurrencyCode { get; set; }
        [XmlElement("FormattedPrice")]
        public string FormattedPrice { get; set; }
    }

    [Serializable()]
    public class ItemAttributes
    {
        [XmlElement("Artist")]
        public List<string> Artists { get; set; }
        [XmlElement("Actor")]
        public List<string> Actors { get; set; }
        [XmlElement("Author")]
        public List<string> Authors { get; set; }
        [XmlElement("Binding")]
        public string Binding { get; set; }
        [XmlArray("CatalogNumberList")]
        [XmlArrayItem("CatalogNumberListElement", typeof(string))]
        public List<string> CatalogNumberList { get; set; }
        [XmlElement("EAN")]
        public string EAN { get; set; }
        [XmlArray("EANList")]
        [XmlArrayItem("EANListElement", typeof(string))]
        public List<string> EANList { get; set; }
        [XmlElement("ItemDimensions")]
        public ItemDimensions ItemDimensions { get; set; }
        [XmlElement("Label")]
        public string Label { get; set; }
        [XmlArray("Languages")]
        [XmlArrayItem("Language", typeof(Language))]
        public List<Language> Languages { get; set; }
        [XmlElement("ISBN")]
        public string ISBN { get; set; }
        [XmlElement("Manufacturer")]
        public string Manufacturer { get; set; }
        [XmlElement("MPN")]
        public string MPN { get; set; }
        [XmlElement("NumberOfDiscs")]
        public int NumberOfDiscs { get; set; }
        [XmlElement("NumberOfItems")]
        public int NumberOfItems { get; set; }
        [XmlElement("PackageDimensions")]
        public ItemDimensions PackageDimensions { get; set; }
        [XmlElement("PartNumber")]
        public string PartNumber { get; set; }
        [XmlElement("ProductGroup")]
        public string ProductGroup { get; set; }
        [XmlElement("ProductTypeName")]
        public string ProductTypeName { get; set; }
        [XmlElement("PublicationDate")]
        public DateTime PublicationDate { get; set; }
        [XmlElement("Publisher")]
        public string Publisher { get; set; }
        [XmlElement("ReleaseDate")]
        public DateTime ReleaseDate { get; set; }
        [XmlElement("Studio")]
        public string Studio { get; set; }
        [XmlElement("Title")]
        public string Title { get; set; }
        [XmlElement("UPC")]
        public string UPC { get; set; }
        [XmlElement("RunningTime")]
        public Int32 RunningTime { get; set; }
        [XmlElement("Genre")]
        public List<string> Genre { get; set; }
        [XmlElement("Director")]
        public string Director { get; set; }
        [XmlElement("AudienceRating")]
        public string AudienceRating { get; set; }
        [XmlElement("IsAdultProduct")]
        public bool IsAdultProduct { get; set; }
        [XmlArray("UPCList")]
        [XmlArrayItem("UPCListElement", typeof(string))]
        public List<string> UPCList { get; set; }
    }

    [Serializable()]
    public class Language
    {
        [XmlElement("Name")]
        public string Name { get; set; }
        [XmlElement("Type")]
        public string Type { get; set; }
    }

    [Serializable()]
    public class ItemDimensions
    {
        [XmlElement("Height")]
        public Dimension Height { get; set; }
        [XmlElement("Length")]
        public Dimension Length { get; set; }
        [XmlElement("Weight")]
        public Dimension Weight { get; set; }
        [XmlElement("Width")]
        public Dimension Width { get; set; }
    }

    [Serializable()]
    public class Dimension
    {
        [XmlAttribute("Units")]
        public string Units { get; set; }
        [XmlText]
        public int Value { get; set; }
    }

    [Serializable()]
    public class ImageSet
    {
        [XmlAttribute("Category")]
        public string Category { get; set; }
        [XmlElement("SwatchImage")]
        public ImageItem SwatchImage { get; set; }
        [XmlElement("ThumbnailImage")]
        public ImageItem ThumbnailImage { get; set; }
        [XmlElement("TinyImage")]
        public ImageItem TinyImage { get; set; }
        [XmlElement("MediumImage")]
        public ImageItem MediumImage { get; set; }
        [XmlElement("LargeImage")]
        public ImageItem LargeImage { get; set; }
    }

    [Serializable()]
    public class ImageItem
    {
        [XmlElement("URL")]
        public string URL { get; set; }
        [XmlElement("Height")]
        public Dimension Height { get; set; }
        [XmlElement("Width")]
        public Dimension Width { get; set; }
    }

    [Serializable()]
    public class ItemLink
    {
        [XmlElement("Description")]
        public string Description { get; set; }
        [XmlElement("URL")]
        public string URL { get; set; }
    }

    [Serializable()]
    public class Request
    {
        [XmlElement("IsValid")]
        public string IsValid { get; set; }
        [XmlElement("ItemLookupRequest")]
        public ItemLookupRequest ItemLookupRequest { get; set; }
    }

    [Serializable()]
    public class ItemLookupRequest
    {
        [XmlElement("IdType")]
        public string IdType { get; set; }
        [XmlElement("ItemId")]
        public string ItemId { get; set; }
        [XmlElement("ResponseGroup")]
        public string ResponseGroup { get; set; }
        [XmlElement("VariationPage")]
        public string VariationPage { get; set; }
    }
}
