namespace UGlue.Kit { 
        using System;
        using System.Collections.Generic;
        using System.Text.RegularExpressions;
        using System.Text;
    using System.Linq;

    /// <summary>
    /// 通用的扩展，类的扩展
    /// </summary>
    public static class ClassExtention {
        /// <summary>
        /// 功能：判断是否为空
        /// 示例：
        /// <code>
        /// var simpleObject = new object();
        ///
        /// if (simpleObject.IsNull()) // 等价于 simpleObject == null
        /// {
        ///     // do sth
        /// }
        /// </code>
        /// </summary>
        /// <param name="selfObj">判断对象(this)</param>
        /// <typeparam name="T">对象的类型（可不填）</typeparam>
        /// <returns>是否为空</returns>
        public static bool IsNull<T>(this T selfObj) where T : class {
            return null == selfObj;
        }

        public static T ApplySelfTo<T>(this T selfObj, System.Action<T> toFunction) where T : class {
            toFunction.InvokeGracefully(selfObj);
            return selfObj;
        }
    }

        /// <summary>
        /// Func、Action、delegate 的扩展
        /// </summary>
        public static class FuncOrActionOrEventExtension {
            #region Func Extension

            /// <summary>
            /// 功能：不为空则调用 Func
            /// 示例:
            /// <code>
            /// Func<int> func = ()=> 1;
            /// var number = func.InvokeGracefully(); // 等价于 if (func != null) number = func();
            /// </code>
            /// </summary>
            /// <param name="selfFunc"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static T InvokeGracefully<T>(this Func<T> selfFunc) {
                return null != selfFunc ? selfFunc() : default;
            }

            #endregion

            #region Action

            /// <summary>
            /// 功能：不为空则调用 Action
            /// 示例:
            /// <code>
            /// System.Action action = () => Log.I("action called");
            /// action.InvokeGracefully(); // if (action != null) action();
            /// </code>
            /// </summary>
            /// <param name="selfAction"> action 对象 </param>
            /// <returns> 是否调用成功 </returns>
            public static bool InvokeGracefully(this Action selfAction) {
                if (null != selfAction) {
                    selfAction();
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 不为空则调用 Action<T>
            /// 示例:
            /// <code>
            /// System.Action<int> action = (number) => Log.I("action called" + number);
            /// action.InvokeGracefully(10); // if (action != null) action(10);
            /// </code>
            /// </summary>
            /// <param name="selfAction"> action 对象</param>
            /// <typeparam name="T">参数</typeparam>
            /// <returns> 是否调用成功</returns>
            public static bool InvokeGracefully<T>(this Action<T> selfAction, T t) {
                if (null != selfAction) {
                    selfAction(t);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 不为空则调用 Action<T,K>
            /// <code>
            /// System.Action<int,string> action = (number,name) => Log.I("action called" + number + name);
            /// action.InvokeGracefully(10,"qframework"); // if (action != null) action(10,"qframework");
            /// </code>
            /// </summary>
            /// <param name="selfAction"></param>
            /// <returns> call succeed</returns>
            public static bool InvokeGracefully<T, K>(this Action<T, K> selfAction, T t, K k) {
                if (null != selfAction) {
                    selfAction(t, k);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 不为空则调用委托
            /// <code>
            /// // delegate
            /// TestDelegate testDelegate = () => { };
            /// testDelegate.InvokeGracefully();
            /// </code>
            /// </summary>
            /// <param name="selfAction"></param>
            /// <returns> call suceed </returns>
            public static bool InvokeGracefully(this Delegate selfAction, params object[] args) {
                if (null != selfAction) {
                    selfAction.DynamicInvoke(args);
                    return true;
                }

                return false;
            }

            #endregion
        }

        /// <summary>
        /// 泛型工具
        /// 实例：
        /// <code>
        /// var typeName = GenericExtention.GetTypeName<string>();
        /// typeName.LogInfo(); // string
        /// </code>
        /// </summary>
        public static class GenericUtil {
            /// <summary>
            /// 获取泛型名字
            /// <code>
            /// var typeName = GenericExtention.GetTypeName<string>();
            /// typeName.LogInfo(); // string
            /// </code>
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public static string GetTypeName<T>() {
                return typeof(T).ToString();
            }
        }

        /// <summary>
        /// 可枚举的集合扩展（Array、List<T>、Dictionary<K,V>)
        /// </summary>
        public static class IEnumerableExtension {
            #region Array Extension

            /// <summary>
            /// 遍历数组
            /// <code>
            /// var testArray = new[] { 1, 2, 3 };
            /// testArray.ForEach(number => number.LogInfo());
            /// </code>
            /// </summary>
            /// <returns>The each.</returns>
            /// <param name="selfArray">Self array.</param>
            /// <param name="action">Action.</param>
            /// <typeparam name="T">The 1st type parameter.</typeparam>
            /// <returns> 返回自己 </returns>
            public static T[] ForEach<T>(this T[] selfArray, Action<T> action) {
                Array.ForEach(selfArray, action);
                return selfArray;
            }

            /// <summary>
            /// 遍历 IEnumerable
            /// <code>
            /// // IEnumerable<T>
            /// IEnumerable<int> testIenumerable = new List<int> { 1, 2, 3 };
            /// testIenumerable.ForEach(number => number.LogInfo());
            /// // 支持字典的遍历
            /// new Dictionary<string, string>()
            ///         .ForEach(keyValue => Log.I("key:{0},value:{1}", keyValue.Key, keyValue.Value));
            /// </code>
            /// </summary>
            /// <returns>The each.</returns>
            /// <param name="selfArray">Self array.</param>
            /// <param name="action">Action.</param>
            /// <typeparam name="T">The 1st type parameter.</typeparam>
            public static IEnumerable<T> ForEach<T>(this IEnumerable<T> selfArray, Action<T> action) {
                if (action == null) throw new ArgumentException();
                foreach (var item in selfArray) {
                    action(item);
                }

                return selfArray;
            }

            #endregion

            #region List Extension

            /// <summary>
            /// 倒序遍历
            /// <code>
            /// var testList = new List<int> { 1, 2, 3 };
            /// testList.ForEachReverse(number => number.LogInfo()); // 3, 2, 1
            /// </code>
            /// </summary>
            /// <returns>返回自己</returns>
            /// <param name="selfList">Self list.</param>
            /// <param name="action">Action.</param>
            /// <typeparam name="T">The 1st type parameter.</typeparam>
            public static List<T> ForEachReverse<T>(this List<T> selfList, Action<T> action) {
                if (action == null) throw new ArgumentException();

                for (var i = selfList.Count - 1; i >= 0; --i)
                    action(selfList[i]);

                return selfList;
            }

            /// <summary>
            /// 倒序遍历（可获得索引)
            /// <code>
            /// var testList = new List<int> { 1, 2, 3 };
            /// testList.ForEachReverse((number,index)=> number.LogInfo()); // 3, 2, 1
            /// </code>
            /// </summary>
            /// <returns>The each reverse.</returns>
            /// <param name="selfList">Self list.</param>
            /// <param name="action">Action.</param>
            /// <typeparam name="T">The 1st type parameter.</typeparam>
            public static List<T> ForEachReverse<T>(this List<T> selfList, Action<T, int> action) {
                if (action == null) throw new ArgumentException();

                for (var i = selfList.Count - 1; i >= 0; --i)
                    action(selfList[i], i);

                return selfList;
            }

            /// <summary>
            /// 遍历列表(可获得索引）
            /// <code>
            /// var testList = new List<int> {1, 2, 3 };
            /// testList.Foreach((number,index)=>number.LogInfo()); // 1, 2, 3,
            /// </code>
            /// </summary>
            /// <typeparam name="T">列表类型</typeparam>
            /// <param name="list">目标表</param>
            /// <param name="action">行为</param>
            public static void ForEach<T>(this List<T> list, Action<int, T> action) {
                for (var i = 0; i < list.Count; i++) {
                    action(i, list[i]);
                }
            }

            #endregion

            #region Dictionary Extension

            /// <summary>
            /// 合并字典
            /// <code>
            /// // 示例
            /// var dictionary1 = new Dictionary<string, string> { { "1", "2" } };
            /// var dictionary2 = new Dictionary<string, string> { { "3", "4" } };
            /// var dictionary3 = dictionary1.Merge(dictionary2);
            /// dictionary3.ForEach(pair => Log.I("{0}:{1}", pair.Key, pair.Value));
            /// </code>
            /// </summary>
            /// <returns>The merge.</returns>
            /// <param name="dictionary">Dictionary.</param>
            /// <param name="dictionaries">Dictionaries.</param>
            /// <typeparam name="TKey">The 1st type parameter.</typeparam>
            /// <typeparam name="TValue">The 2nd type parameter.</typeparam>
            public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
                params Dictionary<TKey, TValue>[] dictionaries) {
                return dictionaries.Aggregate(dictionary,
                    (current, dict) => current.Union(dict).ToDictionary(kv => kv.Key, kv => kv.Value));
            }

            /// <summary>
            /// 遍历字典
            /// <code>
            /// var dict = new Dictionary<string,string> {{"name","liangxie},{"age","18"}};
            /// dict.ForEach((key,value)=> Log.I("{0}:{1}",key,value);//  name:liangxie    age:18
            /// </code>
            /// </summary>
            /// <typeparam name="K"></typeparam>
            /// <typeparam name="V"></typeparam>
            /// <param name="dict"></param>
            /// <param name="action"></param>
            public static void ForEach<K, V>(this Dictionary<K, V> dict, Action<K, V> action) {
                var dictE = dict.GetEnumerator();

                while (dictE.MoveNext()) {
                    var current = dictE.Current;
                    action(current.Key, current.Value);
                }

                dictE.Dispose();
            }

            /// <summary>
            /// 字典添加新的词典
            /// </summary>
            /// <typeparam name="K"></typeparam>
            /// <typeparam name="V"></typeparam>
            /// <param name="dict"></param>
            /// <param name="addInDict"></param>
            /// <param name="isOverride"></param>
            public static void AddRange<K, V>(this Dictionary<K, V> dict, Dictionary<K, V> addInDict,
                bool isOverride = false) {
                var dictE = addInDict.GetEnumerator();

                while (dictE.MoveNext()) {
                    var current = dictE.Current;
                    if (dict.ContainsKey(current.Key)) {
                        if (isOverride)
                            dict[current.Key] = current.Value;
                        continue;
                    }

                    dict.Add(current.Key, current.Value);
                }

                dictE.Dispose();
            }

            #endregion
        }   



        /// <summary>
        /// 字符串扩展
        /// </summary>
        public static class StringExtention {
            public static void Example() {
                var emptyStr = string.Empty;
                emptyStr.IsNotNullAndEmpty();
                emptyStr.IsNullOrEmpty();
                emptyStr = emptyStr.Append("appended").Append("1").ToString();
                emptyStr.IsNullOrEmpty();
            }

            /// <summary>
            /// Check Whether string is null or empty
            /// </summary>
            /// <param name="selfStr"></param>
            /// <returns></returns>
            public static bool IsNullOrEmpty(this string selfStr) {
                return string.IsNullOrEmpty(selfStr);
            }

            /// <summary>
            /// Check Whether string is null or empty
            /// </summary>
            /// <param name="selfStr"></param>
            /// <returns></returns>
            public static bool IsNotNullAndEmpty(this string selfStr) {
                return !string.IsNullOrEmpty(selfStr);
            }

            /// <summary>
            /// Check Whether string trim is null or empty
            /// </summary>
            /// <param name="selfStr"></param>
            /// <returns></returns>
            public static bool IsTrimNotNullAndEmpty(this string selfStr) {
                return !string.IsNullOrEmpty(selfStr.Trim());
            }

            /// <summary>
            /// 缓存
            /// </summary>
            private static readonly char[] mCachedSplitCharArray = { '.' };

            /// <summary>
            /// Split
            /// </summary>
            /// <param name="selfStr"></param>
            /// <param name="splitSymbol"></param>
            /// <returns></returns>
            public static string[] Split(this string selfStr, char splitSymbol) {
                mCachedSplitCharArray[0] = splitSymbol;
                return selfStr.Split(mCachedSplitCharArray);
            }

            /// <summary>
            /// 首字母大写
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string UppercaseFirst(this string str) {
                return char.ToUpper(str[0]) + str.Substring(1);
            }

            /// <summary>
            /// 首字母小写
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string LowercaseFirst(this string str) {
                return char.ToLower(str[0]) + str.Substring(1);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public static string ToUnixLineEndings(this string str) {
                return str.Replace("\r\n", "\n").Replace("\r", "\n");
            }

            /// <summary>
            /// 转换成 CSV
            /// </summary>
            /// <param name="values"></param>
            /// <returns></returns>
            public static string ToCSV(this string[] values) {
                return string.Join(", ", values
                    .Where(value => !string.IsNullOrEmpty(value))
                    .Select(value => value.Trim())
                    .ToArray()
                );
            }

            public static string[] ArrayFromCSV(this string values) {
                return values
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .ToArray();
            }

            public static string ToSpacedCamelCase(this string text) {
                var sb = new StringBuilder(text.Length * 2);
                sb.Append(char.ToUpper(text[0]));
                for (var i = 1; i < text.Length; i++) {
                    if (char.IsUpper(text[i]) && text[i - 1] != ' ') {
                        sb.Append(' ');
                    }

                    sb.Append(text[i]);
                }

                return sb.ToString();
            }

            /// <summary>
            /// 有点不安全,编译器不会帮你排查错误。
            /// </summary>
            /// <param name="selfStr"></param>
            /// <param name="args"></param>
            /// <returns></returns>
            public static string FillFormat(this string selfStr, params object[] args) {
                return string.Format(selfStr, args);
            }

            /// <summary>
            /// 添加前缀
            /// </summary>
            /// <param name="selfStr"></param>
            /// <param name="toAppend"></param>
            /// <returns></returns>
            public static StringBuilder Append(this string selfStr, string toAppend) {
                return new StringBuilder(selfStr).Append(toAppend);
            }

            /// <summary>
            /// 添加后缀
            /// </summary>
            /// <param name="selfStr"></param>
            /// <param name="toPrefix"></param>
            /// <returns></returns>
            public static string AddPrefix(this string selfStr, string toPrefix) {
                return new StringBuilder(toPrefix).Append(selfStr).ToString();
            }

            /// <summary>
            /// 格式化
            /// </summary>
            /// <param name="selfStr"></param>
            /// <param name="toAppend"></param>
            /// <param name="args"></param>
            /// <returns></returns>
            public static StringBuilder AppendFormat(this string selfStr, string toAppend, params object[] args) {
                return new StringBuilder(selfStr).AppendFormat(toAppend, args);
            }

            /// <summary>
            /// 最后一个单词
            /// </summary>
            /// <param name="selfUrl"></param>
            /// <returns></returns>
            public static string LastWord(this string selfUrl) {
                return selfUrl.Split('/').Last();
            }

            /// <summary>
            /// 解析成数字类型
            /// </summary>
            /// <param name="selfStr"></param>
            /// <param name="defaulValue"></param>
            /// <returns></returns>
            public static int ToInt(this string selfStr, int defaulValue = 0) {
                var retValue = defaulValue;
                return int.TryParse(selfStr, out retValue) ? retValue : defaulValue;
            }

            /// <summary>
            /// 解析到时间类型
            /// </summary>
            /// <param name="selfStr"></param>
            /// <param name="defaultValue"></param>
            /// <returns></returns>
            public static DateTime ToDateTime(this string selfStr, DateTime defaultValue = default(DateTime)) {
                var retValue = defaultValue;
                return DateTime.TryParse(selfStr, out retValue) ? retValue : defaultValue;
            }


            /// <summary>
            /// 解析 Float 类型
            /// </summary>
            /// <param name="selfStr"></param>
            /// <param name="defaulValue"></param>
            /// <returns></returns>
            public static float ToFloat(this string selfStr, float defaulValue = 0) {
                var retValue = defaulValue;
                return float.TryParse(selfStr, out retValue) ? retValue : defaulValue;
            }

            /// <summary>
            /// 是否存在中文字符
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static bool HasChinese(this string input) {
                return Regex.IsMatch(input, @"[\u4e00-\u9fa5]");
            }

            /// <summary>
            /// 是否存在空格
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public static bool HasSpace(this string input) {
                return input.Contains(" ");
            }

            /// <summary>
            /// 删除特定字符
            /// </summary>
            /// <param name="str"></param>
            /// <param name="target"></param>
            /// <returns></returns>
            public static string RemoveString(this string str, params string[] targets) {
                return targets.Aggregate(str, (current, t) => current.Replace(t, string.Empty));
            }
        }
}

