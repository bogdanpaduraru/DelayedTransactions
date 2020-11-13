using DelayedTransactions.Models;
using DelayedTransactions.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DelayedTransactions
{
    public class StoreService : IStoreService
    {
        private const string StoreServiceGetItemsUrl = "http://localhost:5000/api/items";
        private const string StoreServiceGetOffersUrl = "http://localhost:5000/api/offers";

        private readonly HttpClient _httpClient;

        private List<StoreItem> _items;
        private List<StoreOffer> _offers;

        public StoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            GetItems();
            GetOffers();
        }

        public async Task<IActionResult> ConsumeOffer(Guid offerId)
        {
            StoreOffer offer = _offers.Find(x=> x.OfferId == offerId);
            if(offer == null)
            {
                return new ContentResult()
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Content = "Not found"
                };
            }

            return new ContentResult()
            { 
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        private async Task GetItems()
        {
            var result = await _httpClient.GetAsync(StoreServiceGetItemsUrl);
            var itemsString = await result.Content.ReadAsStringAsync();
            try
            {
                _items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StoreItem>>(itemsString);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }   
        }

        public async Task GetOffers()
        {
            var result = await _httpClient.GetAsync(StoreServiceGetOffersUrl);
            var offersString = await result.Content.ReadAsStringAsync();
            try
            {
                _offers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<StoreOffer>>(offersString);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
