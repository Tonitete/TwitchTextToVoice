using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace TwitchTextToVoice.TwitchIntegration
{
    public class TokenService
    {
        string secret = "33ecm02di5du4ixb8fjugdpa5ifhy5"; // Your client secret
        string clientID = "e8cjlsdxanqiy2ckf2ucuffyfanz8s"; // Your client ID
        string baseurl = "https://id.twitch.tv/oauth2/authorize?";
        string baseurlToken = "https://id.twitch.tv/oauth2/token";
        public TokenResponseEntity tokenEntity;
        public string userName;

        public TokenService()
        {
            userName = string.Empty;
            tokenEntity = GetUserToken();
        }

        private TokenResponseEntity GetUserToken()
        {
            string url = baseurl + "client_id=" + clientID +
                "&redirect_uri=" + "http://localhost:3000/" +
                "&response_type=" + "code" +
                "&scope=" + "chat%3Aread" +
                "&state=" + "0";
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:3000/");
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
            listener.Start();
            HttpListenerContext context = listener.GetContext();
            listener.Close();
            var error = context.Request.QueryString.Get("error");

            if (error == null)
            {
                string code = context.Request.QueryString.Get("code") ?? "";
                using (HttpClient httpclient = new HttpClient())
                {
                    List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", clientID),
                        new KeyValuePair<string, string>("client_secret", secret),
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("redirect_uri", "http://localhost:3000/")
                    };
                    HttpContent content = new FormUrlEncodedContent(keyValuePairs);
                    var response = httpclient.PostAsync(baseurlToken, content).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    var tokenObject = JsonSerializer.Deserialize<TokenResponseEntity>(responseString);
                    if (tokenObject == null || tokenObject.access_token == null)
                    {
                        return new TokenResponseEntity { error = "Error no controlado" };
                    }

                    httpclient.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokenObject.access_token}");
                    httpclient.DefaultRequestHeaders.Add("Client-Id", clientID);
                    response = httpclient.GetAsync("https://api.twitch.tv/helix/users").Result;
                    responseString = response.Content.ReadAsStringAsync().Result;

                    var jsonObject = JsonSerializer.Deserialize<JsonDocument>(responseString).RootElement;
                    userName = jsonObject.GetProperty("data").EnumerateArray().FirstOrDefault().GetProperty("login").GetString();
                    Settings1.Default.channelToJoin = userName;

                    return tokenObject;
                }
            }
            else
            {
                var result = new TokenResponseEntity
                {
                    error = error + " - " + (context.Request.QueryString.Get("error_description") ?? "").Replace('+', ' ')
                };
                return result;
            }
        }

        public void RefreshToken()
        {
            using (HttpClient httpclient = new HttpClient())
            {
                List<KeyValuePair<string, string>> keyValuePairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("client_id", clientID),
                    new KeyValuePair<string, string>("client_secret", secret),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", UrlEncoder.Default.Encode(tokenEntity.refresh_token))
                };
                HttpContent content = new FormUrlEncodedContent(keyValuePairs);
                var response = httpclient.PostAsync(baseurlToken, content).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                var result = JsonSerializer.Deserialize<TokenResponseEntity>(responseString);

                tokenEntity = result;
            }
        }
    }
}
