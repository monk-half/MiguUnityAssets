namespace UGlue.Kit
{
    using System.Text;
    using UnityEngine.Networking;
    public static class WebRequestExtern
    {
        public static UnityWebRequest SetBody(this UnityWebRequest request,string jsonStr)
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonStr));
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            return request;
        }

        public static UnityWebRequest SetHead(this UnityWebRequest request, string key, string value)
        {
            request.SetRequestHeader(key, value);
            return request;
        }

        public static UnityWebRequest SetDownloadHandler(this UnityWebRequest request, DownloadHandler handler)
        {
            request.downloadHandler = handler;
            return request;
        }

        public static bool IsSucceed(this UnityWebRequest request)
        {
            return !(request.isNetworkError || request.isHttpError);
        }
    }
}
