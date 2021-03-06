namespace UGlue.Kit {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public static class FileTool {
        /// <summary>
        ///  Create Dictionary
        /// </summary>
        /// <param name="filePath">dest path</param>
        public static void CreateDirectory(string filePath) {
            if (!string.IsNullOrEmpty(filePath)) {
                string dirName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirName)) {
                    Directory.CreateDirectory(dirName);
                }
            }
        }

        /// <summary>
        /// Create file by stream
        /// </summary>
        /// <param name="filePath">dest file path</param>
        /// <param name="bytes">file content</param>
        public static void CreatFile(string filePath, byte[] bytes) {
            CreateDirectory(filePath);
            FileInfo file = new FileInfo(filePath);
            Stream stream = file.Create();

            stream.Write(bytes, 0, bytes.Length);

            stream.Close();
            stream.Dispose();
        }

        /// <summary>
        /// 从sourcePath地址中拷贝文件到destDir目录
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destDir"></param>
        /// <param name="overwrite">是否覆盖</param>
        /// <returns></returns>
        public static bool CopyFile(string sourcePath, string destDir, bool overwrite = true) {
            if (!File.Exists(sourcePath)) {
                return false;
            }
            CreateDirectory(destDir);
            new FileInfo(sourcePath).CopyTo(destDir + Path.GetFileName(sourcePath), overwrite);
            return true;
        }

//         public static FileInfo[] GetAllFiles(string directory, string filter) {
//             var root = new DirectoryInfo(directory);
//             Array
//             root.GetFiles().
//         }

        /// <summary>
        /// Get file name in path. for more function, check out Path class 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileName(string path) {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// 保存字符串到path
        /// </summary>
        /// <param name="dataIn">内容</param>
        /// <param name="pathIn">路径</param>
        /// <returns></returns>
        public static bool Save(this string contentIn, string pathIn) {
            try {
                CreateDirectory(pathIn);
                File.WriteAllText(pathIn, contentIn);
            } catch (Exception e) {
                Log.W("Save File Error: " + e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 保存byte数组到path
        /// </summary>
        /// <param name="dataIn">内容</param>
        /// <param name="pathIn">路径</param>
        /// <returns></returns>
        public static bool Save(this byte[] contentIn, string pathIn) {
            try {
                CreateDirectory(pathIn);
                File.WriteAllBytes(pathIn, contentIn);
            } catch (Exception e) {
                Log.W("Save File Error: " + e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判断远程文件是否存在(http)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool Exists(string url) {
            if (string.IsNullOrEmpty(url)) {
                return false;
            }

            if (url.Contains("http")) {
                //Debug.Log("远程文件：" + url);
                try {
                    //创建根据网络地址的请求对象
                    System.Net.HttpWebRequest httpWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.CreateDefault(new Uri(url));
                    httpWebRequest.Method = "HEAD";
                    httpWebRequest.Timeout = 1000;
                    //返回响应状态是否是成功比较的布尔值
                    return (((System.Net.HttpWebResponse)httpWebRequest.GetResponse()).StatusCode == System.Net.HttpStatusCode.OK);
                } catch {
                    return false;
                }
            } else {
                //Debug.Log("本地文件:"+url);
                return File.Exists(url);
            }

        }

    }

    /// <summary>
    /// 对 System.IO 的一些扩展
    /// </summary>
    public static class IOExtension {
        /// <summary>
        /// 创建新的文件夹,如果存在则不创建
        /// <code>
        /// var testDir = "Assets/TestFolder";
        /// testDir.CreateDirIfNotExists();
        /// // 结果为，在 Assets 目录下创建 TestFolder
        /// </code>
        /// </summary>
        public static string CreateDirIfNotExists(this string dirFullPath) {
            if (!Directory.Exists(dirFullPath)) {
                Directory.CreateDirectory(dirFullPath);
            }

            return dirFullPath;
        }

        /// <summary>
        /// 删除文件夹，如果存在
        /// <code>
        /// var testDir = "Assets/TestFolder";
        /// testDir.DeleteDirIfExists();
        /// // 结果为，在 Assets 目录下删除了 TestFolder
        /// </code>
        /// </summary>
        public static void DeleteDirIfExists(this string dirFullPath) {
            if (Directory.Exists(dirFullPath)) {
                Directory.Delete(dirFullPath, true);
            }
        }

        /// <summary>
        /// 清空 Dir（保留目录),如果存在。
        /// <code>
        /// var testDir = "Assets/TestFolder";
        /// testDir.EmptyDirIfExists();
        /// // 结果为，清空了 TestFolder 里的内容
        /// </code>
        /// </summary>
        public static void EmptyDirIfExists(this string dirFullPath) {
            if (Directory.Exists(dirFullPath)) {
                Directory.Delete(dirFullPath, true);
            }

            Directory.CreateDirectory(dirFullPath);
        }

        /// <summary>
        /// 删除文件 如果存在
        /// <code>
        /// // 示例
        /// var filePath = "Assets/Test.txt";
        /// File.Create("Assets/Test);
        /// filePath.DeleteFileIfExists();
        /// // 结果为，删除了 Test.txt
        /// </code>
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <returns> 是否进行了删除操作 </returns>
        public static bool DeleteFileIfExists(this string fileFullPath) {
            if (File.Exists(fileFullPath)) {
                File.Delete(fileFullPath);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 合并路径
        /// <code>
        /// // 示例：
        /// Application.dataPath.CombinePath("Resources").LogInfo();  // /projectPath/Assets/Resources
        /// </code>
        /// </summary>
        /// <param name="selfPath"></param>
        /// <param name="toCombinePath"></param>
        /// <returns> 合并后的路径 </returns>
        public static string CombinePath(this string selfPath, string toCombinePath) {
            return Path.Combine(selfPath, toCombinePath);
        }

        #region 未经过测试

        /// <summary>
        /// 读取文本
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ReadText(this string fileFullPath) {
            var result = string.Empty;

            using (var fs = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read)) {
                using (var sr = new StreamReader(fs)) {
                    result = sr.ReadToEnd();
                }
            }

            return result;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFolder(string path) {
#if UNITY_STANDALONE_OSX
            System.Diagnostics.Process.Start("open", path);
#elif UNITY_STANDALONE_WIN
            System.Diagnostics.Process.Start("explorer.exe", path);
#endif
        }
#endif

        /// <summary>
        /// 获取文件夹名
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetDirectoryName(string fileName) {
            fileName = IOExtension.MakePathStandard(fileName);
            return fileName.Substring(0, fileName.LastIndexOf('/'));
        }

        /// <summary>
        /// 获取文件名
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetFileName(string path, char separator = '/') {
            path = IOExtension.MakePathStandard(path);
            return path.Substring(path.LastIndexOf(separator) + 1);
        }

        /// <summary>
        /// 获取不带后缀的文件名
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtention(string fileName, char separator = '/') {
            return GetFilePathWithoutExtention(GetFileName(fileName, separator));
        }

        /// <summary>
        /// 获取不带后缀的文件路径
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFilePathWithoutExtention(string fileName) {
            if (fileName.Contains("."))
                return fileName.Substring(0, fileName.LastIndexOf('.'));
            return fileName;
        }

        /// <summary>
        /// 使目录存在,Path可以是目录名必须是文件名
        /// </summary>
        /// <param name="path"></param>
        public static void MakeFileDirectoryExist(string path) {
            string root = Path.GetDirectoryName(path);
            if (!Directory.Exists(root)) {
                Directory.CreateDirectory(root);
            }
        }

        /// <summary>
        /// 使目录存在
        /// </summary>
        /// <param name="path"></param>
        public static void MakeDirectoryExist(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 结合目录
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string Combine(params string[] paths) {
            string result = "";
            foreach (string path in paths) {
                result = Path.Combine(result, path);
            }

            result = MakePathStandard(result);
            return result;
        }

        /// <summary>
        /// 获取父文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathParentFolder(string path) {
            if (string.IsNullOrEmpty(path)) {
                return string.Empty;
            }

            return Path.GetDirectoryName(path);
        }


        /// <summary>
        /// 使路径标准化，去除空格并将所有'\'转换为'/'
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string MakePathStandard(string path) {
            return path.Trim().Replace("\\", "/");
        }

        public static List<string> GetDirSubFilePathList(this string dirABSPath, bool isRecursive = true,
            string suffix = "") {
            var pathList = new List<string>();
            var di = new DirectoryInfo(dirABSPath);

            if (!di.Exists) {
                return pathList;
            }

            var files = di.GetFiles();
            foreach (var fi in files) {
                if (!string.IsNullOrEmpty(suffix)) {
                    if (!fi.FullName.EndsWith(suffix, System.StringComparison.CurrentCultureIgnoreCase)) {
                        continue;
                    }
                }

                pathList.Add(fi.FullName);
            }

            if (isRecursive) {
                var dirs = di.GetDirectories();
                foreach (var d in dirs) {
                    pathList.AddRange(GetDirSubFilePathList(d.FullName, isRecursive, suffix));
                }
            }

            return pathList;
        }

        public static List<string> GetDirSubDirNameList(this string dirABSPath) {
            var di = new DirectoryInfo(dirABSPath);

            var dirs = di.GetDirectories();

            return dirs.Select(d => d.Name).ToList();
        }

        public static string GetFileName(this string absOrAssetsPath) {
            var name = absOrAssetsPath.Replace("\\", "/");
            var lastIndex = name.LastIndexOf("/");

            return lastIndex >= 0 ? name.Substring(lastIndex + 1) : name;
        }

        public static string GetFileNameWithoutExtend(this string absOrAssetsPath) {
            var fileName = GetFileName(absOrAssetsPath);
            var lastIndex = fileName.LastIndexOf(".");

            return lastIndex >= 0 ? fileName.Substring(0, lastIndex) : fileName;
        }

        public static string GetFileExtendName(this string absOrAssetsPath) {
            var lastIndex = absOrAssetsPath.LastIndexOf(".");

            if (lastIndex >= 0) {
                return absOrAssetsPath.Substring(lastIndex);
            }

            return string.Empty;
        }

        public static string GetDirPath(this string absOrAssetsPath) {
            var name = absOrAssetsPath.Replace("\\", "/");
            var lastIndex = name.LastIndexOf("/");
            return name.Substring(0, lastIndex + 1);
        }

        public static string GetLastDirName(this string absOrAssetsPath) {
            var name = absOrAssetsPath.Replace("\\", "/");
            var dirs = name.Split('/');

            return absOrAssetsPath.EndsWith("/") ? dirs[dirs.Length - 2] : dirs[dirs.Length - 1];
        }

        #endregion
    }
}


