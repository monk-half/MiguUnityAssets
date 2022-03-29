namespace UGlue.Kit {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public class VHttp<T> where T : DownloadHandler, new(){

        UnityWebRequest m_WebRequest;
        T m_DownloadHandler = new T();
        Action<string> m_actOnError = null;
        Action<T> m_actOnSuccess = null;

        public VHttp(string url, string method = UnityWebRequest.kHttpVerbGET){
            m_WebRequest = new UnityWebRequest(url, method) {
                downloadHandler = m_DownloadHandler
            };
        }

        public VHttp<T> SetHeads(Dictionary<string, string> heads) {
            if (heads != null) {
                foreach (var item in heads) {
                    m_WebRequest.SetRequestHeader(item.Key, item.Value);
                }
            }
            return this;
        }

        public VHttp<T> SetTimeout(int timeout) {
            if (timeout > 0) {
                m_WebRequest.timeout = timeout;
            }
            return this;
        }

        public VHttp<T> OnError(Action<string> onError) {
            if (onError != null) {
                m_actOnError += onError;
            }
            return this;
        }

        public VHttp<T> OnSuccess(Action<T> onSuccess){
            if (onSuccess != null) {
                m_actOnSuccess += onSuccess;
            }
            return this;
        }

        public void Start() {
            if (m_WebRequest != null) {
                Dispatcher.Instance.StartCoroutine(WebRequest(m_WebRequest));
            }
        }

        private IEnumerator WebRequest(UnityWebRequest request) {
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError) {
                Log.W("url:" + request.url + ", Get Error: " + request.error);
                m_actOnError?.Invoke(request.error);
            } else {
                Log.I("url:" + request.url + ", Get Success");
                m_actOnSuccess?.Invoke(m_DownloadHandler);
            }

            request.Dispose();
        }

    }

    public static class VHttp {
        public static void GetText(string url, Action<string> ret, Dictionary<string, string> heads = null, int timeout = -1) {
            Dispatcher.Instance.StartCoroutine(HttpGet(url, new DownloadHandlerBuffer(), heads, timeout, uwrt => ret(uwrt?.text)));
        }

        public static void GetTexture(string url, Action<Texture2D> ret, Dictionary<string, string> heads = null, int timeout = -1) {
            Dispatcher.Instance.StartCoroutine(HttpGet(url, new DownloadHandlerTexture(), heads, timeout, uwrt => ret(uwrt?.texture)));
        }

        /// <summary>
        /// download file to path
        /// </summary>
        /// <param name="url"></param>
        /// <param name="savedPath">path should contains file name</param>
        /// <param name="onSuccess"></param>
        /// <param name="heads"></param>
        /// <param name="timeout"></param>
        public static void DownloadFile(string url, string savedPath, Action<bool> onSuccess = null,
            Dictionary<string, string> heads = null, int timeout = -1) {
            Dispatcher.Instance.StartCoroutine(
                HttpGet(url, new DownloadHandlerFile(savedPath), heads, timeout, (ret) => onSuccess(!(ret == null))));
        }


        private static IEnumerator HttpGet<TT>(string urlIn, TT modeIn, Dictionary<string, string> dicHeadIn = null,
            int iTimeoutIn = -1, Action<TT> retOut = null) where TT : DownloadHandler {

            var uwr = new UnityWebRequest(urlIn, UnityWebRequest.kHttpVerbGET) {
                downloadHandler = modeIn
            };

            if (dicHeadIn != null) {
                foreach (var item in dicHeadIn) {
                    uwr.SetRequestHeader(item.Key, item.Value);
                }
            }

            if (iTimeoutIn > 0) {
                uwr.timeout = iTimeoutIn;
            }

            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError) {
                Log.W("url:" + uwr.url + ", Get Error: " + uwr.error);
                retOut?.Invoke(default);
            } else {
                Log.I("url:" + uwr.url + ", Get Success");
                retOut?.Invoke(modeIn);
            }

            uwr.Dispose();
        }
    }
}