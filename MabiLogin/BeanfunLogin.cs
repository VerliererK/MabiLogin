using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// The MIT License (MIT)
/// Copyright(c) 2015 Kai Hao
/// https://github.com/kevin940726/BeanfunLogin
/// 
/// ReImplement Using Async WebRequest By VerliererK, 2018 June
/// </summary>
namespace BeanfunLogin
{
    public enum LoginMethod { General, QRCode }

    partial class BeanfunLogin : IDisposable
    {
        //Mabinogi Config
        private readonly string service_code = "600309";
        private readonly string service_region = "A2";

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36";
        private readonly int MillisecondsTimeout = 10 * 1000;
        private CookieContainer CookieContainer;
        private string bfWebToken;

        public event EventHandler OnLoginCompleted;

        public List<GameAccount> accountList = new List<GameAccount>();

        public class GameAccount
        {
            public string sacc = null;
            public string sotp = null;
            public string sname = null;
            public string screatetime = null;

            public GameAccount(string sacc, string sotp, string sname, string screatetime = null)
            { this.sacc = sacc; this.sotp = sotp; this.sname = sname; this.screatetime = screatetime; }
        }

        public BeanfunLogin()
        {
            CookieContainer = new CookieContainer();
        }

        ~BeanfunLogin()
        {
            Dispose();
        }

        public void Dispose()
        {
            keepLogin = false;
            LogOut();
            GC.SuppressFinalize(this);
        }

        private HttpWebRequest CreateWebRequest(string uri)
        {
            var request = WebRequest.CreateHttp(uri);
            request.UserAgent = UserAgent;
            request.CookieContainer = CookieContainer;
            return request;
        }

        private async Task<HttpWebRequest> CreateWebRequestPostAsync(string uri, NameValueCollection postData)
        {
            var request = CreateWebRequest(uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string key in postData.Keys)
                stringBuilder.AppendFormat("{0}={1}&", Uri.EscapeDataString(key), Uri.EscapeDataString(postData[key]));
            stringBuilder.Length -= 1;
            byte[] postdatabytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            request.ContentLength = postdatabytes.Length;
            using (var dataStream = await request.GetRequestStreamAsync())
                dataStream.Write(postdatabytes, 0, postdatabytes.Length);
            return request;
        }

        public async Task Login(string account, string password, LoginMethod method)
        {
            var skey = await GetSessionkeyAsync();
            var akey = string.Empty;
            var cardid = string.Empty;
            switch (method)
            {
                case LoginMethod.General:
                    akey = await GeneralLoginAsync(account, password, skey);
                    break;
                case LoginMethod.QRCode:
                    akey = await QRCodeLoginAsync(skey);
                    if (string.IsNullOrEmpty(akey))
                        return;
                    break;
            }

            await GetGameAccountAsync(skey, akey, method, cardid);

            if (OnLoginCompleted != null)
                OnLoginCompleted?.Invoke(this, null);

            KeepLogin();
        }

        private async Task<string> GetSessionkeyAsync()
        {
            var webRequest = CreateWebRequest("https://tw.beanfun.com/beanfun_block/bflogin");
            webRequest.UserAgent = UserAgent;
            webRequest.CookieContainer = CookieContainer;

            using (var response = await WebRequestExtensions.GetResponseAsync(webRequest, MillisecondsTimeout))
            {
                var skey = ParseSkey(response.ResponseUri.ToString());
                if (string.IsNullOrEmpty(skey))
                    throw new Exception("GeneralLogin Failed: No Sessionkey");
                return skey;
            }
        }

        private async Task<string> GeneralLoginAsync(string accountID, string password, string skey)
        {
            var info = await GetASPInfo("https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey=" + skey);
            string viewstate = info.Item1;
            string eventvalidation = info.Item2;

            // Post Id-Password
            NameValueCollection postData = new NameValueCollection
            {
                { "__VIEWSTATE", viewstate },
                { "__EVENTVALIDATION", eventvalidation },
                { "t_AccountID", accountID },
                { "t_Password", password },
                { "btn_login", "登入" }
            };

            var requestPost = await CreateWebRequestPostAsync("https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey=" + skey, postData);

            string akey = string.Empty;
            using (var response = await WebRequestExtensions.GetResponseAsync(requestPost, MillisecondsTimeout))
                akey = ParseAkey(response.ResponseUri.ToString());
            if (string.IsNullOrEmpty(akey))
                throw new Exception("GeneralLogin Failed: No Authkey");
            return akey;
        }

        private async Task<string> QRCodeLoginAsync(string skey)
        {
            var request = CreateWebRequest("https://tw.newlogin.beanfun.com/generic_handlers/get_qrcodeData.ashx?skey=" + skey);

            string strEncryptData = null;

            using (var response = await WebRequestExtensions.GetResponseAsync(request, MillisecondsTimeout))
            using (Stream ReceiveStream = response.GetResponseStream())
            using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                while (!readStream.EndOfStream)
                {
                    string line = readStream.ReadLine();
                    if (line.Contains("strEncryptData"))
                    {
                        Regex regex = new Regex("\"strEncryptData\": \"([^\"]+)\"");
                        if (regex.IsMatch(line))
                        {
                            strEncryptData = regex.Match(line).Groups[1].Value;
                            break;
                        }
                    }
                }

            if (string.IsNullOrEmpty(strEncryptData))
                throw new Exception("QRCodeLogin Failed: No strEncryptData");

            bool Result = await ShowQRCode(strEncryptData);

            if (Result)
            {
                var akey = await QRCodeCheckSuccessAsync(skey);
                return akey;
            }
            else
            {
                return null;
                //throw new Exception("QRCodeLogin Failed: No Authkey");
            }
        }

        private async Task<Tuple<string, string>> GetASPInfo(string uri)
        {
            var request = CreateWebRequest(uri);

            string viewstate = null;
            string eventvalidation = null;

            using (var response = await WebRequestExtensions.GetResponseAsync(request, MillisecondsTimeout))
            using (Stream ReceiveStream = response.GetResponseStream())
            using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                while (!readStream.EndOfStream)
                {
                    string line = readStream.ReadLine();
                    if (line.Contains("VIEWSTATE"))
                    {
                        Regex regex = new Regex("id=\"__VIEWSTATE\" value=\"(.*)\" />");
                        if (regex.IsMatch(line))
                            viewstate = regex.Match(line).Groups[1].Value;
                    }
                    else if (line.Contains("__EVENTVALIDATION"))
                    {
                        Regex regex = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*)\" />");
                        if (regex.IsMatch(line))
                            eventvalidation = regex.Match(line).Groups[1].Value;
                    }
                }

            if (string.IsNullOrEmpty(viewstate))
                throw new Exception("GeneralLogin Failed: No ViewState");
            if (string.IsNullOrEmpty(eventvalidation))
                throw new Exception("GeneralLogin Failed: No Eventvalidation");

            //viewstate = Uri.EscapeDataString(viewstate);
            //eventvalidation = Uri.EscapeDataString(eventvalidation);
            return new Tuple<string, string>(viewstate, eventvalidation);
        }

        private async Task GetGameAccountAsync(string skey, string akey, LoginMethod method, string cardid)
        {
            await GetbfWebTokenAsync(skey, akey);
            string authUri = "https://tw.beanfun.com/beanfun_block/auth.aspx?channel=game_zone&page_and_query=game_start.aspx%3Fservice_code_and_region%3D" + service_code + "_" + service_region + "&web_token=" + bfWebToken;

            var request = CreateWebRequest(authUri);
            using (var response = await WebRequestExtensions.GetResponseAsync(request, MillisecondsTimeout))
            using (Stream ReceiveStream = response.GetResponseStream())
            using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                while (!readStream.EndOfStream)
                {
                    string line = readStream.ReadLine();
                    if (line.Contains("id=\"ulServiceAccountList\" class=\"ServiceAccountList\""))
                    {
                        line = readStream.ReadLine();
                        accountList.Clear();
                        var regex = new Regex("<div id=\"(\\w+)\" sn=\"(\\d+)\" name=\"([^\"]+)\"");
                        foreach (Match match in regex.Matches(line))
                        {
                            if (match.Groups[1].Value == "" || match.Groups[2].Value == "" || match.Groups[3].Value == "")
                                throw new Exception("GetGameAccount Failed: No AccountList");
                            accountList.Add(new GameAccount(match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value));
                        }
                        break;
                    }
                }

            if (accountList.Count == 0)
                throw new Exception("GetGameAccount Failed: No AccountList");
        }

        private async Task GetbfWebTokenAsync(string skey, string akey)
        {
            NameValueCollection postData = new NameValueCollection
            {
                { "SessionKey", skey },
                { "AuthKey", akey }
            };
            var request = await CreateWebRequestPostAsync("https://tw.beanfun.com/beanfun_block/bflogin/return.aspx", postData);

            using (var response = await WebRequestExtensions.GetResponseAsync(request, MillisecondsTimeout))
            {
                var cc = request.CookieContainer;
                var beanfun = cc.GetCookies(new Uri("https://tw.beanfun.com"));
                foreach (Cookie cook in beanfun)
                    if (cook.Name == "bfWebToken")
                    {
                        bfWebToken = cook.Value;
                    }
            }
            if (string.IsNullOrEmpty(bfWebToken))
                throw new Exception("GetGameAccount Failed: No bfWebToken");
        }

        bool keepLogin = true;
        private async void KeepLogin()
        {
            while (keepLogin)
            {
                var request = CreateWebRequest("http://tw.beanfun.com/beanfun_block/generic_handlers/echo_token.ashx?webtoken=1");

                using (var response = await request.GetResponseAsync())
                using (Stream ReceiveStream = response.GetResponseStream())
                using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                {
                    string result = readStream.ReadToEnd();
                    Console.WriteLine(result);
                    if (!result.Contains("ResultCode:1"))
                        keepLogin = false;
                }
                if (keepLogin)
                    await Task.Delay(60 * 1000);
            }
        }

        private async void LogOut()
        {
            keepLogin = false;
            var request = CreateWebRequest("https://tw.newlogin.beanfun.com/logout.aspx?service=999999_T0");
            using (var response = await request.GetResponseAsync()) { }
        }

        private string ParseSkey(string uri)
        {
            if (uri != null)
            {
                Regex regex = new Regex("skey=(.*)&display");
                if (regex.IsMatch(uri))
                {
                    var match = regex.Match(uri);
                    return regex.Match(uri).Groups[1].Value;
                }
            }
            return null;
        }
        private string ParseAkey(string uri)
        {
            if (uri != null)
            {
                Regex regex = new Regex("akey=(.*)");
                if (regex.IsMatch(uri))
                {
                    var match = regex.Match(uri);
                    return regex.Match(uri).Groups[1].Value;
                }
            }
            return null;
        }
    }

    internal static class WebRequestExtensions
    {
        internal static Task<WebResponse> GetResponseAsync(this WebRequest request, int millisecondsTimeout, bool throwException = true)
        {
            return Task.Factory.StartNew(() =>
            {
                var t = Task.Factory.FromAsync(
                    request.BeginGetResponse,
                    request.EndGetResponse,
                    null);

                if (!t.Wait(millisecondsTimeout) && throwException) throw new TimeoutException();

                return t.Result;
            });
        }
    }
}
