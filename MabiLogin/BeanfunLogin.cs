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
    public enum LoginMethod { General, PlaySafe, QRCode }

    partial class BeanfunLogin
    {
        //Mabinogi Config
        private readonly string service_code = "600309";
        private readonly string service_region = "A2";

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36";
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
                case LoginMethod.PlaySafe:
                    var ps = await PlaySafeLoginAsync(account, password, skey);
                    akey = ps.Item1;
                    cardid = ps.Item2;
                    break;
            }

            await GetGameAccountAsync(skey, akey, method, cardid);

            if (OnLoginCompleted != null)
                OnLoginCompleted?.Invoke(this, null);
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
                { "btn_login.x", 1.ToString() },
                { "btn_login.y", 1.ToString() }
            };

            var requestPost = await CreateWebRequestPostAsync("https://tw.newlogin.beanfun.com/login/id-pass_form.aspx?skey=" + skey, postData);

            string akey = string.Empty;
            using (var response = await WebRequestExtensions.GetResponseAsync(requestPost, MillisecondsTimeout))
                akey = ParseAkey(response.ResponseUri.ToString());
            if (string.IsNullOrEmpty(akey))
                throw new Exception("GeneralLogin Failed: No Authkey");
            return akey;
        }

        private async Task<Tuple<string, string>> PlaySafeLoginAsync(string accountID, string password, string skey)
        {
            var info = await GetASPInfo("https://tw.newlogin.beanfun.com/login/playsafe_form.aspx?skey=" + skey);
            string viewstate = info.Item1;
            string eventvalidation = info.Item2;
            string sotp = null;
            DateTime date = DateTime.Now;
            string d = (date.Year - 1900).ToString() + (date.Month - 1).ToString() + date.Day.ToString() + date.Hour.ToString() + date.Minute.ToString() + date.Second.ToString() + date.Millisecond.ToString();

            var request = CreateWebRequest("https://tw.newlogin.beanfun.com/generic_handlers/get_security_otp.ashx?d=" + d);
            using (var response = await WebRequestExtensions.GetResponseAsync(request, MillisecondsTimeout))
            using (Stream ReceiveStream = response.GetResponseStream())
            using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                while (!readStream.EndOfStream)
                {
                    string line = readStream.ReadLine();
                    if (line.Contains("<playsafe_otp>"))
                    {
                        Regex regex = new Regex("<playsafe_otp>(\\w+)</playsafe_otp>");
                        if (regex.IsMatch(line))
                            sotp = regex.Match(line).Groups[1].Value;
                    }
                }

            if (string.IsNullOrEmpty(sotp))
                throw new Exception("PlaySafe Failed: No Playsafe_otp");

            PlaySafe ps = new PlaySafe();
            var readername = ps.GetReader();
            if (string.IsNullOrEmpty(readername))
                throw new Exception("PlaySafe Failed: No ReaderName");
            if (string.IsNullOrEmpty(ps.cardType))
                throw new Exception("PlaySafe Failed: No CardType");

            string original = null;
            string signature = null;
            if (ps.cardType == "F")
            {
                ps.cardid = ps.GetPublicCN(readername);
                if (string.IsNullOrEmpty(ps.cardid)) throw new Exception("PlaySafe Failed: No CardID");
                var opinfo = ps.GetOPInfo(readername, password);
                if (string.IsNullOrEmpty(opinfo)) throw new Exception("PlaySafe Failed: No OpInfo");
                original = ps.cardType + "~" + sotp + "~" + accountID + "~" + opinfo;
                signature = ps.EncryptData(readername, password, original);
                if (string.IsNullOrEmpty(signature)) throw new Exception("PlaySafe Failed: No EncryptedData");
            }
            else if (ps.cardType == "G")
            {
                original = ps.cardType + "~" + sotp + "~" + accountID + "~";
                signature = ps.FSCAPISign(password, original);
            }

            NameValueCollection postData = new NameValueCollection
            {
                { "__VIEWSTATE", viewstate },
                { "__EVENTVALIDATION", eventvalidation },
                { "card_check_id", ps.cardid },
                { "original", original },
                { "signature", signature },
                { "serverotp", sotp },
                { "t_AccountID", accountID },
                { "t_Password", password },
                { "btn_login", "Login" },
                { "btn_login.y", 1.ToString() }
            };

            var requestPost = await CreateWebRequestPostAsync("https://tw.newlogin.beanfun.com/login/playsafe_form.aspx?skey=" + skey, postData);

            string akey = string.Empty;
            using (var response = await WebRequestExtensions.GetResponseAsync(requestPost, MillisecondsTimeout))
                akey = ParseAkey(response.ResponseUri.ToString());
            if (string.IsNullOrEmpty(akey))
                throw new Exception("PlaySafeLogin Failed: No Authkey");
            return new Tuple<string, string>(akey, ps.cardid);
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
            if (method == LoginMethod.PlaySafe)
            {
                authUri += "&cardid=" + Uri.EscapeDataString(cardid);
                var info = await GetASPInfo(authUri);
                authUri += "&__VIEWSTATE=" + Uri.EscapeDataString(info.Item1) + "&__EVENTVALIDATION=" + Uri.EscapeDataString(info.Item2) + "&btnCheckPLASYSAFE=Hidden+Button";
            }

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
        internal static Task<WebResponse> GetResponseAsync(this WebRequest request, int millisecondsTimeout)
        {
            return Task.Factory.StartNew(() =>
            {
                var t = Task.Factory.FromAsync(
                    request.BeginGetResponse,
                    request.EndGetResponse,
                    null);

                if (!t.Wait(millisecondsTimeout)) throw new TimeoutException();

                return t.Result;
            });
        }
    }
}
