using System.Collections;
using UnityEngine.Networking;

public class LoadingManager : Singleton<LoadingManager>
{
    public IEnumerator Load()
    {
        LoadResourceType loadResourceType = LoadResourceType.TEXTURE2D;
        UnityWebRequest webRequest = AssetsBundleManager.Instance.GetWebRequest(loadResourceType, "");
        using (AssetsBundleManager.Instance.GetWebRequest(loadResourceType, ""))
        {
            UnityWebRequestAsyncOperation webRequestAO = webRequest.SendWebRequest();
            yield return webRequestAO;
        }
    }
}