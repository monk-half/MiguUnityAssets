/*
 * 序列化工具
 * 支持 对象<--->XML string | 对象<--->Json string | 对象<--->Byte Array
 * 
 * 
 */


namespace UGlue.Kit {
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    public static class SerializeHelper {


        #region XML
        /// <summary>
        /// 从filepath(支持http路径)中加载数据，并序列化到指定对象(T) 
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sObj">对象</param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static T LoadFromXml<T>(string filePath)where T: class{
            if (!FileTool.Exists(filePath)) {
                Log.W("Xml file not exist: " + filePath);
                return default;
            }

            try {
                using (StreamReader reader = new StreamReader(filePath)) {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    return (T)xmlSerializer.Deserialize(reader);
                }
            } catch (Exception e) {
                Log.W("Load from xml failed: " + e);
            }

            return default;
        }

        /// <summary>
        /// 从filepath(支持http路径)中加载数据，并序列化到指定对象(T)
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sourceObj">对象this扩展</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static T LoadFromXml<T>(this T sourceObj, string filePath) where T : class {
            return LoadFromXml<T>(filePath);
        }

        /// <summary>
        /// 将对象序列化为xml，并保存到指定目录(filePath)
        /// </summary>
        /// <typeparam name="T">序列化对象类型：引用类型</typeparam>
        /// <param name="sourceObj">序列化对象this扩展</param>
        /// <param name="filePath">目标路径</param>
        /// <returns></returns>
        public static bool SaveToXml(this object sourceObj, string filePath){
            if (string.IsNullOrEmpty(filePath)) {
                Log.W("File Path is illegal");
                return false;
            }

            try {
                FileTool.CreateDirectory(filePath);
                using (StreamWriter writer = new StreamWriter(filePath)) {
                    XmlSerializer xmlSerializer = new XmlSerializer(sourceObj.GetType());
                    xmlSerializer.Serialize(writer, sourceObj);
                    Log.I("save to xml success: " + filePath);
                    return true;
                }
            } catch (Exception e) {
                Log.W("save to xml error: " + e);
                return false;
            }
        }
        #endregion

        #region json
        /// <summary>
        /// 对象序列化为json字符串
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>字符串</returns>
        public static string ToJsonStr(this object obj) {
            try {
                return JsonConvert.SerializeObject(obj);
            } catch (Exception e) {
                Log.W("serialize error: " + e);
                return null;
            }
        }

        /// <summary>
        /// 类型强制转换(可能失效)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ReTypeJson<T>(this object obj) {
            var type = obj.GetType();

            if (type.Equals(typeof(JArray))) {
                Log.D("Jarray");
                return ((JArray)obj).ToObject<T>();
            }
            if (type.Equals(typeof(JObject))) {
                Log.D("JObject");
                return ((JObject)obj).ToObject<T>();
            }
            Log.D("Obj");
            return (T)obj;
        }

        /// <summary>
        /// 字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">待序列化类型</typeparam>
        /// <param name="sourceStr">字符串(this扩展)</param>
        /// <returns>序列化对象</returns>
        public static T ToJsonObj<T>(this string sourceStr){
            try {
                return JsonConvert.DeserializeObject<T>(sourceStr);
            } catch (Exception e) {
                Log.W("Deserialize Json string error: " + e);
                return default;
            }
        }

        /// <summary>
        /// 对象序列化为字符串，并保存到path
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public static bool SaveToJson<T>(this T obj, string path){
            bool ret = obj.ToJsonStr().Save(path);
            if (ret) {
                Log.I("save to json success: " + path);
            } else {
                Log.I("save to json failed");
            }
            return ret;
        }

        /// <summary>
        /// 从path中加载文件并序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="path">路径</param>
        /// <returns>序列化后的对象</returns>
        public static T LoadFromJson<T>(this T obj, string path){
            return LoadFromJson<T>(path);
        }

        /// <summary>
        /// 从path中加载文件并序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="path">路径</param>
        /// <returns>序列化后的对象</returns>
        public static T LoadFromJson<T>(string path){
            return File.ReadAllText(path).ToJsonObj<T>();
        }
        #endregion

        #region ProtoBuf

        /// <summary>
        /// 对象序列化为protobuff格式的byte数组
        /// </summary>
        /// <typeparam name="T">对象类型(note: 对象属性必须标记为[ProtoContract])</typeparam>
        /// <param name="obj">待序列化对象</param>
        /// <returns>序列化结果</returns>
        public static byte[] ToProtoArr<T>(this T obj) where T : class {
            try {
                using (MemoryStream ms = new MemoryStream()) {
                    ProtoBuf.Serializer.Serialize<T>(ms, obj);
                    return ms.ToArray();
                }
            } catch (Exception e) {
                Log.W("serialize Proto array error, please check the class attribute(ProtoContract)：" + e);
                return new byte[0];
            }

        }

        /// <summary>
        /// protobuff格式的byte数组反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="bytes">待反序列化的数据</param>
        /// <returns>生成的对象</returns>
        public static T ToProtoObj<T>(this byte[] bytes) where T : class {
            T t = default;
            try {
                if (bytes == null || bytes.Length == 0) {
                    throw new System.ArgumentNullException("bytes");
                }
                t = ProtoBuf.Serializer.Deserialize<T>(new MemoryStream(bytes));
            } catch (Exception e) {
                Log.W("Deserialize Proto to object error: " + e);              
            }
            return t;
        }

        /// <summary>
        /// 将对象序列化并保存到path
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="path">路径</param>
        /// <returns>结果是否成功</returns>
        public static bool SaveToProto<T>(this T obj, string path) where T : class {
            bool ret = FileTool.Save(obj.ToProtoArr<T>(), path);
            if (ret) {
                Log.I("save to proto success: " + path);
            } else {
                Log.I("save to proto failed");
            }
            return ret;
        }

        /// <summary>
        /// 从path中载入byte数组并反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="path">路径</param>
        /// <returns>生成的对象</returns>
        public static T LoadFromProto<T>(string path) where T : class {
            return File.ReadAllBytes(path).ToProtoObj<T>();
        }

        /// <summary>
        /// 从path中载入byte数组并反序列化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="obj">对象</param>
        /// <param name="path">对象类型</param>
        /// <returns>生成的对象</returns>
        public static T LoadFromProto<T>(this T obj, string path) where T : class {
            return LoadFromProto<T>(path);
        }
        #endregion

        #region encoding
        public static byte[] GetByte(this string strData) {
            return Encoding.UTF8.GetBytes(strData);
        }

        public static string GetString(this byte[] byteArr, int idx = -1, int cnt = -1) {
            return (idx == -1 || cnt == -1) 
                ? Encoding.UTF8.GetString(byteArr) : Encoding.UTF8.GetString(byteArr, idx, cnt);
        }
        #endregion
    }
}
