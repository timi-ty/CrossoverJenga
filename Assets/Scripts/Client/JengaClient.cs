using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Client
{
    public class JengaClient : MonoBehaviour
    {
        private const string URL = "https://ga1vqcu3o1.execute-api.us-east-1.amazonaws.com/Assessment/stack";
        
        #region Singleton Instance

        private static JengaClient Instance { get; set; }
        
        #endregion
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (!Instance)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Debug.LogWarning("More than 1 Client Object was found " + "active in the scene. Rectifying...");
                Destroy(gameObject);
            }
        }
        
        public static void GetJengaBlocks(Action<JengaStackData> onBlocksFetched, Action onRequestFailed)
        {
            Instance.GetRequest("", response =>
            {
                var properJson = "{" + $"\"jengaStackData\" : {response}" + "}";
                
                var blocks = JsonUtility.FromJson<JengaStackData>(properJson);

                onBlocksFetched?.Invoke(blocks);
            }, (_) => { onRequestFailed?.Invoke(); });
        }
        
        private void GetRequest(string endpoint, Action<string> onResponseReady = null,
            Action<string> onRequestFailed = null, bool authenticate = true)
        {
            BaseGetRequest(endpoint, onResponseReady, error =>
            {
                onRequestFailed?.Invoke(error);
            }, authenticate);
        }

        private void BaseGetRequest(string endpoint, Action<string> onResponseReady = null,
            Action<string> onRequestFailed = null, bool authenticate = true)
        {
            StartCoroutine(GetRequestRoutine(endpoint, onResponseReady, onRequestFailed));
        }

        private IEnumerator GetRequestRoutine(string endpoint, Action<string> onResponseReady,
            Action<string> onRequestFailed)
        {
            var requestUrl = $"{URL}{endpoint}";

            var webRequest = UnityWebRequest.Get(requestUrl);
            
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var response = Encoding.UTF8.GetString(webRequest.downloadHandler.data);

                onResponseReady?.Invoke(response);
            }
            else
            {
                var responseData = webRequest.downloadHandler.data;
                string response = "";
                if(responseData != null) response = Encoding.UTF8.GetString(responseData);
                Debug.LogWarningFormat("Request to {0} failed with error: {1}. Returned response: {2}", webRequest.url, webRequest.error, response);
                onRequestFailed?.Invoke(response);
            }
        }
    }
}