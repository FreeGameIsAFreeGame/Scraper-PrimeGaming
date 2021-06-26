using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io;
using FreeGameIsAFreeGame.Core;
using FreeGameIsAFreeGame.Core.Models;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace FreeGameIsAFreeGame.Scraper.PrimeGaming
{
    public class PrimeGamingScraper : IScraper
    {
        private const string GRAPH_QUERY =
            "{\"operationName\":\"Prime_OfferList_Offers\",\"variables\":{},\"query\":\"query Prime_OfferList_Offers($dateOverride: Time) {\\n  primeOffers(dateOverride: $dateOverride) {\\n    ...Offer\\n    __typename\\n  }\\n}\\n\\nfragment Offer on PrimeOffer {\\n  id\\n  title\\n  assets {\\n    type\\n    purpose\\n    location\\n    location2x\\n    __typename\\n  }\\n  description\\n  deliveryMethod\\n  priority\\n  tags {\\n    type\\n    tag\\n    __typename\\n  }\\n  content {\\n    externalURL\\n    publisher\\n    categories\\n    __typename\\n  }\\n  startTime\\n  endTime\\n  self {\\n    hasEntitlement\\n    claimData\\n    claimInstructions\\n    __typename\\n  }\\n  linkedJourney {\\n    ...Journey\\n    __typename\\n  }\\n  __typename\\n}\\n\\nfragment Journey on Journey {\\n  self {\\n    enrollmentStatus\\n    __typename\\n  }\\n  offers {\\n    self {\\n      claimStatus\\n      __typename\\n    }\\n    __typename\\n  }\\n  __typename\\n}\\n\",\"extensions\":{}}";

        private IBrowsingContext context;
        private ILogger logger;

        private RestClient restClient;
        private CookieContainer cookieContainer;

        string IScraper.Identifier => "PrimeGaming";

        /// <inheritdoc />
        public Task Initialize(CancellationToken token)
        {
            context = BrowsingContext.New(Configuration.Default
                .WithDefaultLoader()
                .WithDefaultCookies());

            logger = LogManager.GetLogger(GetType().FullName);

            return Task.CompletedTask;
        }

        async Task<IEnumerable<IDeal>> IScraper.Scrape(CancellationToken token)
        {
            restClient = new RestClient("https://twitch.amazon.com");
            restClient.CookieContainer = cookieContainer = new CookieContainer();

            string csrfToken = await GetCSRFToken(token);

            if (token.IsCancellationRequested)
                return default;

            string content = await GetContent(csrfToken, token);
            TwitchGraphContent twitchGraphContent = JsonConvert.DeserializeObject<TwitchGraphContent>(content);
            List<IDeal> deals = ParseGraphContent(twitchGraphContent);

            return deals;
        }

        private async Task<string> GetCSRFToken(CancellationToken token)
        {
            RestRequest request = new RestRequest("tp/loot", Method.GET);
            IRestResponse response = await restClient.ExecuteAsync(request, token);
            if (token.IsCancellationRequested)
                return default;

            string content = response.Content;
            IDocument document = await context.OpenAsync(x => x.Content(content), token);
            if (token.IsCancellationRequested)
                return default;

            IHtmlInputElement inputElement = document.Body.QuerySelector<IHtmlInputElement>("input[name='csrf-key']");
            return inputElement.Value;
        }

        private async Task<string> GetContent(string csrfToken, CancellationToken token)
        {
            RestRequest request = new RestRequest("graphql", Method.POST);
            request.AddHeader(HeaderNames.Accept, "*/*");
            request.AddHeader(HeaderNames.AcceptEncoding, "gzip, deflate, br");
            request.AddHeader(HeaderNames.AcceptLanguage, "en-US,en;q=0.5");
            request.AddHeader(HeaderNames.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0");
            request.AddHeader("csrf-token", csrfToken);
            request.AddParameter("application/json", GRAPH_QUERY, ParameterType.RequestBody);

            IRestResponse response = await restClient.ExecuteAsync(request, token);
            if (token.IsCancellationRequested)
                return default;

            string content = response.Content;
            return content;
        }

        private List<IDeal> ParseGraphContent(TwitchGraphContent content)
        {
            List<IDeal> deals = new List<IDeal>();

            foreach (PrimeOffer offer in content.Data.PrimeOffers)
            {
                if (offer.DeliveryMethod != "DIRECT_ENTITLEMENT")
                    continue;

                if (offer.Tags.Count == 0)
                    continue;
                if (offer.Tags.All(x => x.TagTag != "FGWP"))
                    continue;

                deals.Add(new Deal
                {
                    Discount = 100,
                    Image = GetImageUrl(offer),
                    Link = "https://gaming.amazon.com/home",
                    Title = offer.Title,
                    Start = offer.StartTime.UtcDateTime,
                    End = offer.EndTime.UtcDateTime
                });
            }

            return deals;
        }

        private string GetImageUrl(PrimeOffer offer)
        {
            Asset result = offer.Assets.FirstOrDefault(x => x.Purpose == "DETAIL");
            if (result != null)
                return result.Location2X.ToString();

            result = offer.Assets.FirstOrDefault(x => x.Purpose == "THUMBNAIL");
            if (result != null)
                return result.Location2X.ToString();

            result = offer.Assets.FirstOrDefault(x => x.Purpose == "ICON");
            if (result != null)
                return result.Location2X.ToString();

            return "https://upload.wikimedia.org/wikipedia/commons/thumb/c/ce/Twitch_logo_2019.svg/512px-Twitch_logo_2019.svg.png";
        }

        /// <inheritdoc />
        public Task Dispose()
        {
            context?.Dispose();
            return Task.CompletedTask;
        }
    }
}
