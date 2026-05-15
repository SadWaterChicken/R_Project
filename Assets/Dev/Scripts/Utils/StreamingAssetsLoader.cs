using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class StreamingAssetsLoader
{
    // filename is relative to Assets/StreamingAssets, include extension (e.g. "Shops/MageShop.json")
    public static IEnumerator LoadJsonFromStreamingAssets(string filename, Action<string> onSuccess, Action<string> onError = null)
    {
        if (string.IsNullOrEmpty(filename))
        {
            onError?.Invoke("Filename is null or empty");
            yield break;
        }

        var fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, filename);

        // On Android streamingAssetsPath is inside the APK and must be loaded with UnityWebRequest
        string uri;
#if UNITY_ANDROID && !UNITY_EDITOR
        uri = fullPath; // UnityWebRequest handles the internal jar path on Android
#else
        uri = "file://" + fullPath;
#endif

        using var www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (www.result != UnityWebRequest.Result.Success)
#else
        if (www.isNetworkError || www.isHttpError)
#endif
        {
            onError?.Invoke(www.error);
            yield break;
        }

        onSuccess?.Invoke(www.downloadHandler.text);
    }
}