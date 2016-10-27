## Windows (UWP) Guide



**Works with react-native-windows >=0.36.1**



**Warning:** This is a first draft, many things may not be ok ! Don't use it in production.

**Note:** GoogleSignin will open a browser where the user can login, then will callback the rn app to finish the process. It's a bit different that how the android and ios version works.
	  
**Note:** the module doesn't use a WebView (as Google said using a webview will be deprecated soon)
	  
**Note:** Tested on Windows 10 and Windows Mobile 10


### 1. Installation


#### Google API setup

Follow help here:
https://github.com/googlesamples/oauth-apps-for-windows/blob/master/OAuthUniversalApp/README.md

Warning: You need an "iOS" oauth client id.


#### Visual studio configuration

* Add (with "Add as a link") "RNGoogleSigninModule.cs" and "RNGoogleSigninPackage.cs" from "node_modules\react-native-google-signin\windows" to your Visual Studio project.
* Open the project package manifest file.
* In "Declarations" : add a "Protocol" declaration.
* Set the "DisplayName" 
* Set "Name" (usually your package name in reverse order, check your configuration on the google api console)




#### Project setup

Inside you main component in react native:

* In "componentDidMount()" :

``` 
if (Platform.OS=="windows") 
{ 
	Linking.addEventListener('url', this._handleOpenURL); 
}
```
	
* In "componentWillUnmount()" :

```
if (Platform.OS=="windows") 
{ 
	Linking.removeEventListener('url', this._handleOpenURL); 
}
```
	   
* Then add this function :

```
_handleOpenURL(event) {
	if (Platform.OS=="windows")
	{			
		if (event.url.indexOf(":/oauth2redirect")!=-1)
		{                                    
			console.log("received googlesign auth validation");
			GoogleSignin.processRedirectUrl(event.url); 
		}                
	}
}	
```

#### GoogleSign.configure(...)

``` 
GoogleSignin.configure({
  //scopes: [], // not used for windows (for now)
  iosPackageName: "com.mycompany.myapp", // needed for windows
  windowsScopes: "openid%20email", // special for windows, don't use url type scopes nor an array
  iosClientId: <FROM DEVELOPPER CONSOLE>, // client ID (must be ios)
  //webClientId: <FROM DEVELOPPER CONSOLE>, // client ID of type WEB for your server (needed to verify user ID and offline access)
  //offlineAccess: true // if you want to access Google API on behalf of the user FROM YOUR SERVER
})
.then(() => {
  // you can now call currentUserAsync()
});
```

#### Known issues

* revokeAccess(): not available
* configure/windowsScope: only tested with "openid%20email"
* configure/scopes: ignored
* webClientId: ignored
* configure/offlineAccess: ignored (maybe always "true" at this time)
* Default scopes is "openid%20email" -> may not be the same than on android and ios versions.
* RNGoogleSigninButton :
 * It's not "native" (done in reactnative) 
 * English only
 * Has no "Google" icon
 * Doesn't support GoogleSigninButton.Size.Icon
 * Doesn't support GoogleSigninButton.Color.Dark




#### Credits

This windows implementation was created based on sample project by google:
https://github.com/googlesamples/oauth-apps-for-windows

Thanks to @rozele (https://github.com/rozele) from https://github.com/ReactWindows/react-native-windows for upgrading react-native-windows Linking component with the feature this module requires.
