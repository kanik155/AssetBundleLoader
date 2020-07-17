using System.Collections;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoadAssetBundleScene : MonoBehaviour
{
    private const string BundleServerUrl = "https://storage.googleapis.com/production-comony/";

    private void Start()
    {
        var uuid = "333f28fd-66e8-4fbb-9c30-5982f9557164";
        StartCoroutine(LoadAssetBundle(uuid));
    }

    IEnumerator LoadAssetBundle(string uuid)
    {
        // .manifestをダウンロード
        using (var request = UnityWebRequest.Get(BundleServerUrl + uuid + ".manifest"))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                // .manifestからhashを取得
                var matches = Regex.Matches(request.downloadHandler.text, "Hash: (?<hash>[0-9a-z]+)");
                Hash128 hash = Hash128.Parse(matches[0].Groups["hash"].ToString());

                // hashによるキャッシングでassetbundleをロード
                using (var unityWebRequestAsset = UnityWebRequestAssetBundle.GetAssetBundle(BundleServerUrl + uuid, hash, 0))
                {
                    yield return unityWebRequestAsset.SendWebRequest();

                    if (unityWebRequestAsset.isNetworkError || unityWebRequestAsset.isHttpError)
                    {
                        Debug.Log(unityWebRequestAsset.error);
                    }
                    else
                    {
                        // スペースをロード
                        var bundle = DownloadHandlerAssetBundle.GetContent(unityWebRequestAsset);
                        var sceneName = Path.GetFileNameWithoutExtension(bundle.GetAllScenePaths()[0]);
                        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                        var scene = SceneManager.GetSceneByName(bundle.name);
                        while (!scene.isLoaded)
                        {
                            yield return null;
                        }

                        SceneManager.SetActiveScene(scene);
                    }
                }
            }
        }
    }
}