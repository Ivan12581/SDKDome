using UnityEngine;
using UnityEngine.Networking;

public class AssetsBundleManager : Singleton<AssetsBundleManager>
{
    public UnityWebRequest GetWebRequest(LoadResourceType loadType, string url)
    {
        UnityWebRequest webRequest = loadType switch
        {
            LoadResourceType.TXT => UnityWebRequest.Get(url),
            LoadResourceType.TEXTURE2D => UnityWebRequestTexture.GetTexture(url),
            LoadResourceType.MP3 => UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG),
            LoadResourceType.OGG => UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS),
            LoadResourceType.AssetBundle => UnityWebRequestAssetBundle.GetAssetBundle(url),
            _ => UnityWebRequest.Get(url),
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