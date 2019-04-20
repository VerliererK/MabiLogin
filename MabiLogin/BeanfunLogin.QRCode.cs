using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeanfunLogin
{
    partial class BeanfunLogin
    {
        private Form QRForm;
        private bool IsQRFormOpened = false;

        private void CloseQRForm()
        {
            if (QRForm != null)
            {
                IsQRFormOpened = false;
                QRForm.Close();
                QRForm.Dispose();
                QRForm = null;
            }
        }

        private async Task<bool> ShowQRCode(string strEncryptData)
        {
            CloseQRForm();

            var request = CreateWebRequest(
                "https://tw.newlogin.beanfun.com/qrhandler.ashx?u=https://beanfunstor.blob.core.windows.net/redirect/appCheck.html?url=beanfunapp://Q/gameLogin/gtw/" +
                strEncryptData);

            using (var response = await WebRequestExtensions.GetResponseAsync(request, MillisecondsTimeout))
            using (Stream ReceiveStream = response.GetResponseStream())
            using (var image = System.Drawing.Image.FromStream(ReceiveStream))
            {
                if (QRForm == null) QRForm = new Form();
                QRForm.StartPosition = FormStartPosition.CenterScreen;
                QRForm.AutoSize = true;
                QRForm.AutoSizeMode = AutoSizeMode.GrowOnly;

                var pb = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    Image = image
                };
                pb.SizeMode = PictureBoxSizeMode.CenterImage;

                IsQRFormOpened = true;
                bool Result = false;
                QRForm.Load += async (o, e) =>
                {
                    Result = await QRCodeCheckAsync(strEncryptData);
                    CloseQRForm();
                };
                QRForm.FormClosed += (o, e) => IsQRFormOpened = false;
                QRForm.Controls.Add(pb);
                QRForm.ShowDialog();
                return Result;
            }
        }

        // {"Result":0,"ResultMessage":"Failed","ResultData":null}
        private async Task<bool> QRCodeCheckAsync(string strEncryptData)
        {
            NameValueCollection postData = new NameValueCollection
            {
                { "status", strEncryptData }
            };
            var checkRequest = await CreateWebRequestPostAsync("https://tw.newlogin.beanfun.com/generic_handlers/CheckLoginStatus.ashx", postData);

            try
            {
                using (var response = await WebRequestExtensions.GetResponseAsync(checkRequest, MillisecondsTimeout, false))
                using (Stream ReceiveStream = response.GetResponseStream())
                using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
                {
                    string text = await readStream.ReadToEndAsync();
                    string ResultMessage = null;
                    int Result = 0;

                    Regex regex_ResultMessage = new Regex("\"ResultMessage\":\"([^\"]+)\"");
                    Regex regex_Result = new Regex("\"Result\":([0-9])");
                    if (regex_ResultMessage.IsMatch(text)) ResultMessage = regex_ResultMessage.Match(text).Groups[1].Value;
                    if (regex_Result.IsMatch(text)) int.TryParse(regex_Result.Match(text).Groups[1].Value, out Result);

                    Console.WriteLine(text);

                    if (Result == 1 && ResultMessage == "Success")
                    {
                        return true;
                    }
                    else if (Result == 0 && ResultMessage == "Token Expired")
                    {
                        MessageBox.Show("QRCode Token Expired. Please Try Again.");
                    }
                    else
                    {
                        await Task.Delay(2 * 1000);
                        if (IsQRFormOpened)
                            return await QRCodeCheckAsync(strEncryptData);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Something Wrong!\n" + e.Message);
            }

            return false;
        }


        private async Task<string> QRCodeCheckSuccessAsync(string skey)
        {
            var request = CreateWebRequest("https://tw.newlogin.beanfun.com/login/qr_step2.aspx?skey=" + skey);

            string akey = null;

            using (var response = await WebRequestExtensions.GetResponseAsync(request, MillisecondsTimeout))
            using (Stream ReceiveStream = response.GetResponseStream())
            using (StreamReader readStream = new StreamReader(ReceiveStream, Encoding.UTF8))
            {
                string text = readStream.ReadToEnd();
                Regex regex = new Regex("akey=([^&]+)");
                if (regex.IsMatch(text))
                {
                    var match = regex.Match(text);
                    akey = regex.Match(text).Groups[1].Value;
                }
            }

            if (string.IsNullOrEmpty(akey))
                return null;

            request = CreateWebRequest("https://tw.newlogin.beanfun.com/login/final_step.aspx?akey=" + akey + "&authkey=N&bfapp=1");

            using (var response = await WebRequestExtensions.GetResponseAsync(request, MillisecondsTimeout))
                return akey;
        }
    }
}
