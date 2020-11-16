using UnityEngine;
using UnityEngine.Networking;

public class AssetsBundleManager : Singleton<AssetsBundleManager>
{
    public UnityWebRequest GetWebRequest(LoadResourceType loadType, string url)
    {
        UnityWebRequest webRequest;
        switch (loadType)
        {
            case LoadResourceType.TXT:
                webRequest = UnityWebRequest.Get(url);
                break;

            case LoadResourceType.TEXTURE2D:
                webRequest = UnityWebRequestTexture.GetTexture(url);
                break;

            case LoadResourceType.MP3:
                webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
                break;

            case LoadResourceType.OGG:
                webRequest = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
                break;

            case LoadResourceType.AssetBundle:
                webRequest = UnityWebRequestAssetBundle.GetAssetBundle(url);
                break;

            default:
                webRequest = UnityWebRequest.Get(url);
                break;
        };
        return webRequest;
    }
}

/// <summary>
/// 资源类型
/// </summary>
public enum LoadResourceType
{
    TXT,
    TEXTURE2D,
    MP3,
    OGG,
    AssetBundle,
}