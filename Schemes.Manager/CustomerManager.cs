﻿using Newtonsoft.Json;
using Schemes.Models;
using Schemes.Repository;
using Schemes.ViewModels;
using System.Collections.Generic;
using System.Text;

namespace Schemes.Manager
{
    public class CustomerManager
    {
        private readonly SchemesContext _dbContext;
        public CustomerManager(SchemesContext DbContext)
        {
            _dbContext = DbContext;
        }
        public List<SchemeDetails> GetSchemesList(string name, string langCode)
        {
            List<SchemeDetails> schemeDetails = new CustomerRepository(_dbContext).GetSchemesList(name);
            return schemeDetails;
        }
        public List<SchemeDetails> GetAllSchemesList()
        {
            List<SchemeDetails> schemeDetails = new CustomerRepository(_dbContext).GetAllSchemesList();
            return schemeDetails;
        }
        public bool AddNewScheme(SchemeDetails schemeDetails)
        {
            var result = new CustomerRepository(_dbContext).AddNewScheme(schemeDetails);

            return result;
        }
        public List<CustomerDetails> GetCustomersList()
        {
            var list = new CustomerRepository(_dbContext).GetCustomersList();
            return list;
        } 
        public SchemeDetails GetSchemesDetails(int schemeiD)
        {
            var scheme = new CustomerRepository(_dbContext).GetSchemesDetails(schemeiD);
            return scheme;
        }
        public bool UpdateScheme(SchemeDetails schemeDetails)
        {
            var result = new CustomerRepository(_dbContext).UpdateScheme(schemeDetails);
            return result;
        }
        public bool UpdateCustomerStatus(int customerID)
        {
            var result = new CustomerRepository(_dbContext).UpdateCustomerStatus(customerID);
            return result;
        }
        public LoanDetails GetLoanDetails(string Loantype, string bankName)
        {
            var loanDetails = new CustomerRepository(_dbContext).GetLoanDetails(Loantype, bankName);
            return loanDetails;
        }
        public List<string> GetLoantypes(string bankName)
        {
            var types = new CustomerRepository(_dbContext).GetLoantypes(bankName);
            return types;
        }

        public async Task TranslationAPI(List<string> textsTotranslate,int schemeID)
        {
            string endpoint = "https://saikiran-translatorservice.cognitiveservices.azure.com/";
            string subscriptionKey = "";
            string region = "eastus";
            string[] targetLanguages = { "hi", "ar", "bho", "kn", "ml", "ta", "te","ur" };
            var requestBody = new List<object>();

            foreach (string targetLanguage in targetLanguages)
            {
                foreach (var text in textsTotranslate)
                {
                    requestBody.Add(new { Text = text });
                }
                string url = $"{endpoint}/translate?api-version=3.0&to={targetLanguage}";

                using (HttpClient client = new HttpClient())
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        string requestBodyJson = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);

                        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", region);
                        request.Content = new StringContent(requestBodyJson, Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.SendAsync(request);
                        string responseBody = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            var translations = JsonConvert.DeserializeObject<List<TranslationResponse>>(responseBody);
                            var schemeData = new MultilingualSchemesDataVM();

                            for (int i = 0; i < translations.Count; i++)
                            {
                                string translatedText = translations[i].Translations.First().Text ?? ""; // Use the first translation as translated text
                                string translatedLang = translations[i].Translations.First().To ?? ""; // Use the first translation as translated text

                                switch (i)
                                {
                                    case 0:
                                        schemeData.NameOftheScheme = translatedText;
                                        break;
                                    case 1:
                                        schemeData.Description = translatedText;
                                        break;
                                    case 2:
                                        schemeData.EligibilityCriteria = translatedText;
                                        break;
                                    case 3:
                                        schemeData.Benefits = translatedText;
                                        break;
                                    case 4:
                                        schemeData.Area = translatedText;
                                        break;
                                    case 5:
                                        schemeData.DocumentsRequired = translatedText;
                                        break;
                                    case 6:
                                        schemeData.ApplyAndLink = translatedText;
                                        break;
                                    case 7:
                                        schemeData.AvaliableFor = translatedText;
                                        break;
                                    
                                    default:
                                        // Handle cases where there are more translations than properties
                                        break;
                                }
                                new CustomerRepository(_dbContext).AddNewSchemeByLangCode(schemeData, schemeID, translatedLang);
                            }
                        }
                    }
                }
            }
        }
    }
}
