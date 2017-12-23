using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using RestSharp;
using System.Runtime.InteropServices.ComTypes;
using System.Net;
using System.IO;

namespace ReadObiCatalog
{
    class JsonMagento
    {
        string _token;
        string _user;
        string _pass;

        static HttpClient client = new HttpClient();

        const string WEBSITE = "http://localhost/store/rest/V1";
        const string METHOD = "POST";
        const string CONTENT_TYPE = "application/json";

        enum generatedType
        {
            TOKEN,
            CATEGORY,
            PRODUCT
        }

        public bool isTokenNull()
        {
            if (this._token == null)
                return true;

            return false;
        }

        public JsonMagento(string user, string pass)
        {
            this._user = user;
            this._pass = pass;
            //this.generateToken();
            this._token = "TOKEN";
        }

        private string _createCorrectUrl(generatedType genType)
        {
            string path;

            switch (genType)
            {
                case generatedType.TOKEN:
                    path = "integration/admin/token";
                    break;

                case generatedType.CATEGORY:
                    path = "categories";
                    break;

                case generatedType.PRODUCT:
                    path = "products";
                    break;

                default:
                    Console.WriteLine("Nieznany typ");
                    return null;
            }

            if (WEBSITE[WEBSITE.Length - 1] != '/')
                path = '/' + path;

            return WEBSITE + path;
        }

        private bool generateToken()
        {
            string url = _createCorrectUrl(generatedType.TOKEN);
            string json = "{ \"username\": \"" + this._user + "\", \"password\": \"" + this._pass + "\" }";

            string result = _post(json, url);

            if (result != null)
                this._token = result.Trim('"');

            return true;
        }

        /*
         * RETURNS NEW CATEGORY's ID (-1 means failure)
         */
        public int addNewCategory(string json)
        {
            string url = _createCorrectUrl(generatedType.CATEGORY);

            string result = _post(json, url);

            if (result == null)
                return -1;

            return getId(result);
        }

        /* 
         * RETURNS CHOOSEN CATEGORY ID
         */
        public bool getCategoryId(string categoryName)
        {

            return true;
        }

        /*
         * RETURNS NEW CATEGORY's ID (-1 means failure)
         */
        public int addNewProducts(string json)
        {
            string url = _createCorrectUrl(generatedType.PRODUCT);

            string result = _post(json, url);

            if (result == null)
                return -1;

            return 1;
        }


        private string _post(string json, string url)
        {
            string result = "";

            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    if (this._token != null)
                        client.Headers[HttpRequestHeader.Authorization] = "Bearer " + this._token;
                    //result = client.UploadString(url, "POST", json);
                    byte[] toBytes = Encoding.UTF8.GetBytes(json);
                    byte[] result2 = client.UploadData(url, "POST", toBytes);

                    result = Encoding.UTF8.GetString(result2);

                }
            }
            catch (WebException ex)
            {
                try
                {
                    var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    Console.WriteLine(resp);
                }
                catch (NullReferenceException ex_new)
                {
                    Console.WriteLine(ex_new.Message);
                }

                Console.WriteLine(ex.Message);
                return null;
            }

            Console.WriteLine(result);

            return result;
        }

        /*
         * GET ID FROM JSON
         */
        private int getId(string json)
        {
            //TODO: rozwinąć metodę, żeby zawierała sprawdzenie czy na pewno zwraca ID oraz czy jest liczbą a nie null/0
            return Convert.ToInt32(json.Split(',')[0].Split(':')[1]);
        }
    }
}
