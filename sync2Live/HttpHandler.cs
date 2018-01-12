using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Script.Serialization;


namespace sync2Live
{
    public class HttpHandler
    {
        public static async void Post(string url, Dictionary<string, string> values)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(url, content);
            } 
        }

        public static string PostJson(string url, string jsonParam)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonParam);
                streamWriter.Flush();
                streamWriter.Close();
            }
            string result = "";
            try {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    var pageContent = new StreamReader(wex.Response.GetResponseStream())
                                          .ReadToEnd();
                    result = wex.Message + " : " + pageContent.ToString();
                }
                else {
                    result = wex.Message;

                }
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return result;
        }

        public static string Get(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = WebRequestMethods.Http.Get;
            request.Accept = "application/json";

            using (var twitpicResponse = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = new StreamReader(twitpicResponse.GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var objText = reader.ReadToEnd();
                    return objText;
                }                
            }

            /*using (var client = new HttpClient())
            {
                var response = client.GetStringAsync(url);
                return response.ToString();
            }*/
        }
    }
}
