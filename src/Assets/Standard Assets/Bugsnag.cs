using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Text.RegularExpressions;

public class Bugsnag : MonoBehaviour {
    public class NativeBugsnag {
        #if UNITY_IPHONE
            [DllImport ("__Internal")]
            public static extern void SetUserId(string userId);
        
            [DllImport ("__Internal")]
            public static extern void SetContext(string context);
        
            [DllImport ("__Internal")]
            public static extern void SetReleaseStage(string releaseStage);
        
            [DllImport ("__Internal")]
            public static extern void Notify(string errorClass, string errorMessage, string stackTrace);
                
            [DllImport ("__Internal")]
            public static extern void Register(string apiKey);
                
            [DllImport ("__Internal")]
            public static extern void SetUseSSL(bool useSSL);
            
            [DllImport ("__Internal")]
            public static extern void SetAutoNotify(bool autoNotify);
                
            [DllImport ("__Internal")]
            public static extern void AddToTab(string tabName, string attributeName, string attributeValue);
                
            [DllImport ("__Internal")]
            public static extern void ClearTab(string tabName);
        #elif UNITY_ANDROID
            [DllImport ("bugsnag")]
            public static extern void SetUserId(string userId);
        
            [DllImport ("bugsnag")]
            public static extern void SetContext(string context);
        
            [DllImport ("bugsnag")]
            public static extern void SetReleaseStage(string releaseStage);
        
            public static void Notify(string errorClass, string errorMessage, string stackTrace) {
                AndroidJavaObject bugsnagAndroidClass = new AndroidJavaObject("com.bugsnag.android.BugsnagUnity");

				using(AndroidJavaObject jsonArray = new AndroidJavaObject("org.json.JSONArray")) {
                    jsonArray.Call<AndroidJavaObject>("put", 1);
                    jsonArray.Call<AndroidJavaObject>("put", 2);
                    jsonArray.Call<AndroidJavaObject>("put", 3);
                    
                    using(AndroidJavaObject jsonObject = new AndroidJavaObject("org.json.JSONObject")) {
                        using(AndroidJavaObject key = new AndroidJavaObject("java.lang.String", "file")) {
                            using(AndroidJavaObject val = new AndroidJavaObject("java.lang.String", "fileNameHere")) {
                                jsonObject.Call<AndroidJavaObject>("put", key, val);
                            }
                        }
                        jsonArray.Call<AndroidJavaObject>("put", jsonObject);
                    }
                    
                    bugsnagAndroidClass.CallStatic("notifyTest", errorClass, errorMessage, jsonArray);
                }
            }
                
            [DllImport ("bugsnag")]
            public static extern void Register(string apiKey);
            
            [DllImport ("bugsnag")]
            public static extern void SetUseSSL(bool useSSL);
            
            [DllImport ("bugsnag")]
            public static extern void SetAutoNotify(bool autoNotify);
            
            [DllImport ("bugsnag")]
            public static extern void AddToTab(string tabName, string attributeName, string attributeValue);
            
            [DllImport ("bugsnag")]
            public static extern void ClearTab(string tabName);
        #else
            public static void SetUserId(string userId) {}
            public static void SetContext(string context) {}
            public static void SetReleaseStage(string releaseStage) {}
            public static void Notify(string errorClass, string errorMessage, string stackTrace) {}
            public static void Register(string apiKey) {}
            public static void SetUseSSL(bool useSSL) {}
            public static void SetAutoNotify(bool autoNotify) {}
            public static void AddToTab(string tabName, string attributeName, string attributeValue) {}
            public static void ClearTab(string tabName) {}
        #endif
    }
        
    // We dont use the LogType enum in Unity as the numerical order doesnt suit our purposes
    public enum LogSeverity {
        Log,
        Warning,
        Assert,
        Error,
        Exception
    }

    public string BugsnagApiKey = "";
    public bool AutoNotify = true;
    public bool UseSSL = true;
    public LogSeverity NotifyLevel = LogSeverity.Exception;
    public string UserId {
        set {
            NativeBugsnag.SetUserId(value);
        }
    }
        
    public string ReleaseStage {
        set {
            NativeBugsnag.SetReleaseStage(value);
        }
    }
        
    public string Context { 
        set {
            NativeBugsnag.SetContext(value);
        }
    }
        
    void Awake() {
        DontDestroyOnLoad(this);
        NativeBugsnag.Register(BugsnagApiKey);
        NativeBugsnag.SetUseSSL(UseSSL);
        NativeBugsnag.SetAutoNotify(AutoNotify);
    }
    
    void OnEnable () {
        Application.RegisterLogCallback(HandleLog);
    }
    
    void OnDisable () {
        // Remove callback when object goes out of scope
        Application.RegisterLogCallback(null);
    }
        
    void OnLevelWasLoaded(int level) {
        NativeBugsnag.SetContext(string.Format("{0}: Level {1}", Application.loadedLevelName, level));
    }
    
    void HandleLog (string logString, string stackTrace, LogType type) {
        LogSeverity severity = LogSeverity.Exception;
        
        switch (type) {
        case LogType.Assert:
            severity = LogSeverity.Assert;
            break;
        case LogType.Error:
            severity = LogSeverity.Error;
            break;
        case LogType.Exception:
            severity = LogSeverity.Exception;
            break;
        case LogType.Log:
            severity = LogSeverity.Log;
            break;
        case LogType.Warning:
            severity = LogSeverity.Warning;
            break;
        default:
            break;
        }

        if(severity >= NotifyLevel && AutoNotify) {
            string errorClass, errorMessage = null;
            
            Regex exceptionRegEx = new Regex(@"^(?<errorClass>\S+):\s*(?<message>.*)");
            Match match = exceptionRegEx.Match(logString);

            if(match.Success) {
                errorClass = match.Groups["errorClass"].Value;
                errorMessage = match.Groups["message"].Value.Trim();
            } else {
                errorClass = logString;
            }
                
            NativeBugsnag.Notify(errorClass, errorMessage, stackTrace);
        }
    }

    public static void Notify(Exception e) {
        if(e != null && e.StackTrace != null) {
      			NativeBugsnag.Notify(e.GetType().ToString(), e.Message, e.StackTrace);
        }
    }
        
    public static void AddToTab(string tabName, string attributeName, string attributeValue) {
        NativeBugsnag.AddToTab(tabName, attributeName, attributeValue);
    }
        
    public static void ClearTab(string tabName) {
        NativeBugsnag.ClearTab(tabName);
    }
}