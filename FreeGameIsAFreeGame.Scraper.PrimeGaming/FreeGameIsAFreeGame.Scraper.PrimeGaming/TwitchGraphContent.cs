using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FreeGameIsAFreeGame.Scraper.PrimeGaming
{
    using J = JsonPropertyAttribute;

    public partial class TwitchGraphContent
    {
        [J("data")] public Data Data { get; set; }
    }

    public partial class Data
    {
        [J("primeOffers")] public List<PrimeOffer> PrimeOffers { get; set; }
    }

    public partial class PrimeOffer
    {
        [J("id")] public Guid Id { get; set; }
        [J("title")] public string Title { get; set; }
        [J("assets")] public List<Asset> Assets { get; set; }
        [J("description")] public string Description { get; set; }
        [J("deliveryMethod")] public string DeliveryMethod { get; set; }
        [J("priority")] public long Priority { get; set; }
        [J("tags")] public List<Tag> Tags { get; set; }
        [J("content")] public Content Content { get; set; }
        [J("startTime")] public DateTimeOffset StartTime { get; set; }
        [J("endTime")] public DateTimeOffset EndTime { get; set; }
        [J("self")] public PrimeOfferSelf Self { get; set; }
        [J("linkedJourney")] public LinkedJourney LinkedJourney { get; set; }
        [J("__typename")] public string Typename { get; set; }
    }

    public partial class Asset
    {
        [J("type")] public string Type { get; set; }
        [J("purpose")] public string Purpose { get; set; }
        [J("location")] public string Location { get; set; }
        [J("location2x")] public Uri Location2X { get; set; }
        [J("__typename")] public string Typename { get; set; }
    }

    public partial class Content
    {
        [J("externalURL")] public Uri ExternalUrl { get; set; }
        [J("publisher")] public string Publisher { get; set; }
        [J("categories")] public List<string> Categories { get; set; }
        [J("__typename")] public string Typename { get; set; }
    }

    public partial class LinkedJourney
    {
        [J("self")] public LinkedJourneySelf Self { get; set; }
        [J("offers")] public List<Offer> Offers { get; set; }
        [J("__typename")] public string Typename { get; set; }
    }

    public partial class Offer
    {
        [J("self")] public OfferSelf Self { get; set; }
        [J("__typename")] public string Typename { get; set; }
    }

    public partial class OfferSelf
    {
        [J("claimStatus")] public string ClaimStatus { get; set; }
        [J("__typename")] public string Typename { get; set; }
    }

    public partial class LinkedJourneySelf
    {
        [J("enrollmentStatus")] public string EnrollmentStatus { get; set; }
        [J("__typename")] public string Typename { get; set; }
    }

    public partial class PrimeOfferSelf
    {
        [J("hasEntitlement")] public bool HasEntitlement { get; set; }
        [J("claimData")] public object ClaimData { get; set; }
        [J("claimInstructions")] public string ClaimInstructions { get; set; }
        [J("__typename")] public string Typename { get; set; }
    }

    public partial class Tag
    {
        [J("type")] public string Type { get; set; }
        [J("tag")] public string TagTag { get; set; }
        [J("__typename")] public string Typename { get; set; }
    }

    public partial class Welcome
    {
        public static Welcome FromJson(string json) => JsonConvert.DeserializeObject<Welcome>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Welcome self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter {DateTimeStyles = DateTimeStyles.AssumeUniversal}
            },
        };
    }
}
