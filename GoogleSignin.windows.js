import React, { Component } from 'react';

import {
  View,
  NativeAppEventEmitter,
  NativeModules,
  requireNativeComponent,
  Text,
  TouchableOpacity,
} from 'react-native';

const { RNGoogleSignin } = NativeModules;

//const RNGoogleSigninButton = requireNativeComponent('RNGoogleSigninButton', null);

class GoogleSigninButton extends Component {
  componentDidMount() {
    /*this._clickListener = NativeAppEventEmitter.addListener('RNGoogleSignInWillDispatch', () => {
      GoogleSigninSingleton.signinIsInProcess = true;
      this.props.onPress && this.props.onPress();
    });*/
  }

  componentWillUnmount() {
    this._clickListener && this._clickListener.remove();
  }

  signinButtonClicked()
  {
	  if (this.props.onPress!=null)
		this.props.onPress();
  }
  
  render() {
    /*return (
       <RNGoogleSigninButton {...this.props}/>	  
    );*/
	
	return (
	<TouchableOpacity style={{flex:0, backgroundColor:'#4285F4', paddingVertical:15, paddingHorizontal:60}}
		onPress={() => this.signinButtonClicked() }>
		<Text style={{color:'white'}}>Sign in with Google</Text>
	</TouchableOpacity>
	)
  }
}

GoogleSigninButton.Size = {
  Icon: null, // RNGoogleSignin.BUTTON_SIZE_ICON,
  Standard: null, //RNGoogleSignin.BUTTON_SIZE_STANDARD,
  Wide: null, //RNGoogleSignin.BUTTON_SIZE_WIDE
};

GoogleSigninButton.Color = {
  Auto: null, //RNGoogleSignin.BUTTON_COLOR_AUTO,
  Light: null, //RNGoogleSignin.BUTTON_COLOR_LIGHT,
  Dark: null, //RNGoogleSignin.BUTTON_COLOR_DARK
};

class GoogleSignin {

  constructor() {
    this._user = null;
    this.signinIsInProcess = false;
  }

  hasPlayServices(params = {autoResolve: true}) {
    return Promise.resolve(true);
  }

  configure(params={}) {
	  
	if (!params.iosClientId) {
      throw new Error('GoogleSignin - Missing iOS app ClientID');
    }
	
	if (!params.iosPackageName) {
      throw new Error('GoogleSignin - Missing iosPackageName');
    }
	 
    /*if (!params.webClientId) {
      throw new Error('GoogleSignin - Missing web ClientID');
    }

    if (params.offlineAccess && !params.webClientId) {
      throw new Error('GoogleSignin - offline use requires server web ClientID');
    }

    params = [
      params.scopes || [], params.webClientId, params.offlineAccess ? params.webClientId : ''
    ];*/

    RNGoogleSignin.configure(params.windowsScopes, params.iosClientId, params.iosPackageName);
    return true;
  }

  currentUserAsync() {
	  console.log("GoogleSignin.windows.js : currentUserAsync step0");
    return new Promise((resolve, reject) => {
      	  
	  var ret = RNGoogleSignin.currentUserAsync();
	  this._user = ret;
	  resolve(ret);
	  	 
    });
  }

  currentUser() {
    return {...this._user};
  }

  processRedirectUrl(url)
  {
	  RNGoogleSignin.processRedirectUrl(url);
  }
  
  signIn() {
	  
    return new Promise((resolve, reject) => {
		
	  console.log("GoogleSignin.windows.js : signIn step1");
	  
	  if (this.signinIsInProcess)
	  {
		  resolve(null);
		  return;
	  }
	  console.log("GoogleSignin.windows.js : signIn step2");
	  
	var ret = RNGoogleSignin.signIn(); 
	
	console.log("RNGoogleSignin.signIn() in GoogleSignin.windows.js -> ret : " + JSON.stringify(ret));
	
	resolve(ret);
	
    });
  }

  signOut() {
    return new Promise((resolve, reject) => {
	  this._user = null;
      RNGoogleSignin.signOut();
      resolve();
    });
  }

  revokeAccess() {
    return new Promise((resolve, reject) => {
     
	  this._user = null;
      RNGoogleSignin.revokeAccess();
	  resolve();
    });
  }


}

const GoogleSigninSingleton = new GoogleSignin();

module.exports = {GoogleSignin: GoogleSigninSingleton, GoogleSigninButton};
