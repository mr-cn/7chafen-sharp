using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace _7chafen_sharp
{
    public class API
    {
        public static async Task<string> Login(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                var form = new List<KeyValuePair<string, string>>();
                form.Add(new KeyValuePair<string, string>("pwd", password));
                form.Add(new KeyValuePair<string, string>("usercode", username));

                HttpResponseMessage response = await client.PostAsync("https://studentapp.7net.cc/user/login",
                    new FormUrlEncodedContent(form));
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject loginResult = JObject.Parse(responseBody);
                try
                {
                    if ((string) loginResult["status"] == "200" && (string) loginResult["message"] == "success")
                    {
                        return (string) loginResult["data"]["token"];
                    }

                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }
        }

        public static async Task<List<Exam>> GetExams(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("token", token);

                HttpResponseMessage response =
                    await client.GetAsync("https://studentapp.7net.cc/exam/claim?pageIndex=1&pageLength=10");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(responseBody);
                try
                {
                    if ((string) result["status"] == "200" && (string) result["message"] == "success")
                    {
                        IList<JToken> results = result["data"].Children().Select(x => x["data"]).Children().ToList();

                        var exams = new List<Exam>();
                        foreach (var one in results)
                        {
                            exams.Add(one.ToObject<Exam>());
                        }

                        return exams;
                    }

                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }
        }

        public static async Task<User> GetUserInfo(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("token", token);

                HttpResponseMessage response =
                    await client.GetAsync("https://studentapp.7net.cc/userInfo/info");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(responseBody);
                try
                {
                    if ((string) result["status"] == "200" && (string) result["message"] == "success")
                    {
                        return result["data"].ToObject<User>();
                    }

                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }
        }

        public static async Task<bool> Validate(string token)
        {
            if (await GetUserInfo(token) != null)
            {
                return true;
            }

            return false;
        }

        public class Exam
        {
            public string name { get; set; }
            public string guid { get; set; }
        }

        public class User
        {
            public string name { get; set; }
            public string gradeName { get; set; }
            public string schoolName { get; set; }
        }
    }
}