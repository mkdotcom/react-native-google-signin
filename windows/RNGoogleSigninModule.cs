using Newtonsoft.Json.Linq;
using ReactNative.Bridge;
using ReactNative.Collections;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Newtonsoft.Json;
using System.Net.Http;
using Windows.Data.Json;
using System.Linq;
using System.Diagnostics;
using System.Text;

// based on google sample : https://github.com/googlesamples/oauth-apps-for-windows




namespace ReactNative.Modules.RNGoogleSignin
{
    public class RNGoogleSigninModule : ReactContextNativeModuleBase 
    {        
        string clientID = "";
        string windowsScopes = "openid%20email";         // default (not the same than android and ios version ?)
        string redirectURI = "";

        const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth"; 
  
        private User currentUser=null;

        public RNGoogleSigninModule(ReactContext reactContext)
            : base(reactContext)
        {
        }
		
		public override string Name
		{
			get
			{
				return "RNGoogleSignin";
			}
		}
						

        class User
        {
            public String email;
            public String access_token;
            public String accessToken;
            
        }

        IPromise finalPromise;

        [ReactMethod]
        public async void signIn(
            IPromise promise)
        {
            finalPromise = promise;
            go();            
        }

        /// <summary>
        /// Starts an OAuth 2.0 Authorization Request.
        /// </summary>
        private void go()
        {
            if (redirectURI==null || redirectURI.Length==0)
            {
                output("configure() -> redirect URI is not set!");
                return;
            }

            // Generates state and PKCE values.
            string state = randomDataBase64url(32);
            string code_verifier = randomDataBase64url(32);
            string code_challenge = base64urlencodeNoPadding(sha256(code_verifier));
            const string code_challenge_method = "S256";

            // Stores the state and code_verifier values into local settings.
            // Member variables of this class may not be present when the app is resumed with the
            // authorization response, so LocalSettings can be used to persist any needed values.
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["state"] = state;
            localSettings.Values["code_verifier"] = code_verifier;

            // Creates the OAuth 2.0 authorization request.                        
            string scopes = windowsScopes;
            string authorizationRequest = string.Format("{0}?response_type=code&scope=" + scopes + "&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                state,
                code_challenge,
                code_challenge_method);

            output("Opening authorization request URI: " + authorizationRequest);

            // Opens the Authorization URI in the browser.
            var success = Windows.System.Launcher.LaunchUriAsync(new Uri(authorizationRequest));
        }


        [ReactMethod]
        public async void currentUserAsync(
            IPromise promise)
        {
            // done
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string str = (string)localSettings.Values["ReactNativeGoogleSigin_CurrentUser"];
            if (str != null && str.Length > 0)
                currentUser = JsonConvert.DeserializeObject<User>(str);
            else
                currentUser = null;

            promise.Resolve(currentUser); 
        }

        [ReactMethod]
        public async void revokeAccess( 
           IPromise promise)
        {            
            // NOT available on windows

            promise.Resolve(false); 
        }

        [ReactMethod]
        public async void signOut( 
          IPromise promise)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["ReactNativeGoogleSigin_CurrentUser"] = null;
            currentUser = null;

            promise.Resolve(true);  
        }

        public void done(string responseString, string email)
        {           
            currentUser = JsonConvert.DeserializeObject<User>(responseString);
            currentUser.email = email;
            currentUser.accessToken = currentUser.access_token;

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["ReactNativeGoogleSigin_CurrentUser"] = JsonConvert.SerializeObject(currentUser);

            finalPromise.Resolve(currentUser); 
        }
       

        private static async void RunOnDispatcher(DispatchedHandler action)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, action);
        }

        /*private void OnInvoked(IUICommand target, ICallback callback)
		{
			callback.Invoke(ActionButtonClicked, target.Id);
		}*/

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string randomDataBase64url(uint length)
        {
            IBuffer buffer = CryptographicBuffer.GenerateRandom(length);
            return base64urlencodeNoPadding(buffer);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputStirng"></param>
        /// <returns></returns>
        public static IBuffer sha256(string inputStirng)
        {
            HashAlgorithmProvider sha = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            IBuffer buff = CryptographicBuffer.ConvertStringToBinary(inputStirng, BinaryStringEncoding.Utf8);
            return sha.HashData(buff);
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string base64urlencodeNoPadding(IBuffer buffer)
        {
            string base64 = CryptographicBuffer.EncodeToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }

        [ReactMethod]
        public async void processRedirectUrl(string url, 
          IPromise promise)
        {
            Uri uri = new Uri(url);
            googleSigninOauthRedirect(uri);            
        }

        [ReactMethod]
        public void configure(string _windowsScopes, string _iosClientId, string _iosPackageName,
          IPromise promise)
        {
            clientID = _iosClientId;
            windowsScopes = _windowsScopes;
            redirectURI = _iosPackageName + ":/oauth2redirect"; 

            output("RNGoogleSigninModule : configure : redirectURI : " + redirectURI);
            output("RNGoogleSigninModule : configure : windowsScopes : " + windowsScopes);
            output("RNGoogleSigninModule : configure : clientID : " + clientID);

            promise.Resolve(true);
        }

        /// /////////////////////////
        // debug ajout google signin            
        public void googleSigninOauthRedirect(Uri uri)
        {            
            // Gets URI from navigation parameters.
            Uri authorizationResponse = uri; // (Uri)e.Parameter;
            string queryString = authorizationResponse.Query;
            output("MainPage received authorizationResponse: " + authorizationResponse);

            // Parses URI params into a dictionary
            // ref: http://stackoverflow.com/a/11957114/72176
            Dictionary<string, string> queryStringParams =
                    queryString.Substring(1).Split('&')
                            .ToDictionary(c => c.Split('=')[0],
                                        c => Uri.UnescapeDataString(c.Split('=')[1]));

            if (queryStringParams.ContainsKey("error"))
            {
                output(String.Format("OAuth authorization error: {0}.", queryStringParams["error"]));
                return;
            }

            if (!queryStringParams.ContainsKey("code")
                || !queryStringParams.ContainsKey("state"))
            {
                output("Malformed authorization response. " + queryString);
                return;
            }

            // Gets the Authorization code & state
            string code = queryStringParams["code"];
            string incoming_state = queryStringParams["state"];

            // Retrieves the expected 'state' value from local settings (saved when the request was made).
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            string expected_state = (String)localSettings.Values["state"];

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization
            if (incoming_state != expected_state)
            {
                output(String.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }

            // Resets expected state value to avoid a replay attack.
            localSettings.Values["state"] = null;

            // Authorization Code is now ready to use!
            output(Environment.NewLine + "Authorization code: " + code);

            string code_verifier = (String)localSettings.Values["code_verifier"];
            performCodeExchangeAsync(code, code_verifier);           
        }

        class UserInfo
        {
            public string email;
            // note: there are others informations but we just need "email" for now
        }

        private void output(string s)
        {
            Debug.WriteLine("output: " + s);
        }


        async void performCodeExchangeAsync(string code, string code_verifier)
        {            
            const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";         

            // Builds the Token request
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&scope=&grant_type=authorization_code",
                code,
                System.Uri.EscapeDataString(redirectURI),
                clientID,
                code_verifier
                );
            StringContent content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            HttpClientHandler handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            HttpClient client = new HttpClient(handler);

            output(Environment.NewLine + "Exchanging code for tokens...");
            HttpResponseMessage response = await client.PostAsync(tokenEndpoint, content);
            string responseString = await response.Content.ReadAsStringAsync();
            output(responseString);


            if (!response.IsSuccessStatusCode)
            {
                output("Authorization code exchange failed.");
                return;
            }

            string email = "";           

            if (true) // get user email (to optimize: is there a way to get the user email directly from the previous request?)
            {
                const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

                // Sets the Authentication header of our HTTP client using the acquired access token.
                JsonObject tokens = JsonObject.Parse(responseString);
                string accessToken = tokens.GetNamedString("access_token");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                // Makes a call to the Userinfo endpoint, and prints the results.
                output("Making API Call to Userinfo...");
                HttpResponseMessage userinfoResponse = client.GetAsync(userInfoEndpoint).Result;
                string userinfoResponseContent = await userinfoResponse.Content.ReadAsStringAsync();
                output(userinfoResponseContent);

                UserInfo ui = JsonConvert.DeserializeObject<UserInfo>(userinfoResponseContent);
                email = ui.email;
            }

            // ok
            done(responseString, email);
            
        }
        
    }
}