using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// The MIT License (MIT)
/// Copyright(c) 2015 Kai Hao
/// https://github.com/kevin940726/BeanfunLogin
///  
/// ReImplement Using Async WebRequest By VerliererK, 2018 June
/// </summary>
namespace BeanfunLogin
{
    partial class BeanfunLogin
    {
        public async System.Threading.Tasks.Task<string> GetGamePasswordAsync(GameAccount acc)
        {
            return await GetOTPAsync(acc, service_code, service_region);
        }

        private string DecryptDES(string hexString, string key)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None,
                Key = Encoding.ASCII.GetBytes(key)
            };
            byte[] s = new byte[hexString.Length / 2];
            int j = 0;
            for (int i = 0; i < hexString.Length / 2; i++)
            {
                s[i] = byte.Parse(hexString[j].ToString() + hexString[j + 1].ToString(), System.Globalization.NumberStyles.HexNumber);
                j += 2;
            }
            ICryptoTransform desencrypt = des.CreateDecryptor();
            return Encoding.ASCII.GetString(desencrypt.TransformFinalBlock(s, 0, s.Length)).Replace("\0", "");
        }

        private async System.Threading.Tasks.Task<string> GetOTPAsync(GameAccount acc, string service_code, string service_region)
        {
            string longPollingKey = "";

            var webRequest = CreateWebRequest("https://tw.beanfun.com/beanfun_block/game_zone/game_start_step2.aspx?service_code=" + service_code + "&service_region=" + service_region + "&sotp=" + acc.sotp);
            using (var r = await WebRequestExtensions.GetResponseAsync(webRequest, MillisecondsTimeout))
            using (Stream ReceiveStream = r.GetResponseStream())
            using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                while (!readStream.EndOfStream)
                {
                    var line = readStream.ReadLine();
                    if (line.Contains("GetResultByLongPolling"))
                    {
                        Regex regex = new Regex("GetResultByLongPolling&key=(.*)\"");
                        if (!regex.IsMatch(line))
                            throw new Exception("GetGamePassword Failed: OTPNoLongPollingKey");
                        longPollingKey = regex.Match(line).Groups[1].Value;
                    }
                    else if (string.IsNullOrEmpty(acc.screatetime) && line.Contains("ServiceAccountCreateTime"))
                    {
                        Regex regex = new Regex("ServiceAccountCreateTime: \"([^\"]+)\"");
                        if (!regex.IsMatch(line))
                            throw new Exception("GetGamePassword Failed: OTPNoCreateTime");
                        acc.screatetime = regex.Match(line).Groups[1].Value;
                    }
                }

            string secretCode = "";
            webRequest = CreateWebRequest("https://tw.newlogin.beanfun.com/generic_handlers/get_cookies.ashx");
            using (var r = await WebRequestExtensions.GetResponseAsync(webRequest, MillisecondsTimeout))
            using (Stream ReceiveStream = r.GetResponseStream())
            using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                while (!readStream.EndOfStream)
                {
                    var line = readStream.ReadLine();
                    if (line.Contains("m_strSecretCode"))
                    {
                        Regex regex = new Regex("var m_strSecretCode = '(.*)';");
                        if (!regex.IsMatch(line)) throw new Exception("GetGamePassword Failed: OTPNoSecretCode");
                        secretCode = regex.Match(line).Groups[1].Value;
                        break;
                    }
                }

            webRequest = CreateWebRequest("http://tw.beanfun.com/beanfun_block/generic_handlers/get_webstart_otp.ashx?SN=" + longPollingKey + "&WebToken=" + bfWebToken + "&SecretCode=" + secretCode + "&ppppp=1F552AEAFF976018F942B13690C990F60ED01510DDF89165F1658CCE7BC21DBA&ServiceCode=" + service_code + "&ServiceRegion=" + service_region + "&ServiceAccount=" + acc.sacc + "&CreateTime=" + acc.screatetime.Replace(" ", "%20") + "&d=" + Environment.TickCount);
            using (var r = await WebRequestExtensions.GetResponseAsync(webRequest, MillisecondsTimeout))
            using (Stream ReceiveStream = r.GetResponseStream())
            using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
            {
                string DESValue = readStream.ReadToEnd();
                DESValue = DESValue.Substring(2);
                string key = DESValue.Substring(0, 8);
                string plain = DESValue.Substring(8);
                string otp = DecryptDES(plain, key);
                if (string.IsNullOrEmpty(otp))
                    throw new Exception("GetGamePassword Failed: Decrypt OTP Failed");
                return otp;
            }
        }
    }
}
