namespace UGlue {
    using System;
    using UGlue.Kit;
    using UnityEngine;

    public static class Log {

        //日志运行模式，Release模式高性能，debug模式显示调用栈
        public enum MODE { DEBUG, RELEASE }
        public static MODE Mode = MODE.DEBUG;

        //日志等级
        public enum LOG_GRADE { debug, info, warning, error };

        //日志结构体
        public struct LogItem {
            public LOG_GRADE Grade;
            public string Head;
            public string Info;
        }

        //新日志消息
        private static Action<LogItem> OnLog;

        /// <summary>
        /// 新日志事件
        /// </summary>
        /// <param name="logElement"></param>
        private static void OnNewLog(LogItem logElement) {
            string LogInfo = logElement.Head + logElement.Info;
            Debug.Log(LogInfo);
            OnLog?.Invoke(logElement);
        }

        public static void Subscribe(Action<LogItem> action) {
            if (action.IsNull()) {
                return;
            }

            OnLog += action;
        }

        public static void UnSubscribe(Action<LogItem> action) {
            if (action.IsNull()) {
                return;
            }

            OnLog -= action;
        }

        /// <summary>
        /// 获取调用栈信息，反射，耗费性能
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        private static string GetTraceMsg(int depth) {
#if UNITY_IOS && !UNITY_EDITOR
            return "";
#endif
            var method = new System.Diagnostics.StackTrace().GetFrame(depth).GetMethod();
            return method.ReflectedType.Name + "." + method.Name + "()\t ==> ";
        }

        /// <summary>
        /// debug等级的log，release模式下不显示
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <param name="stackDepth"></param>
        /// <returns></returns>
        public static T D<T>(T info, int stackDepth = 2) {
            if (Mode == MODE.RELEASE) {
                return info;
            }

            OnNewLog(new LogItem() {
                Grade = LOG_GRADE.debug,
                Head = "D/" + GetTraceMsg(stackDepth),
                Info = info.ToString()
            });

            return info;
        }

        /// <summary>
        /// info等级的日志，release模式下不显示调用栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <param name="stackDepth"></param>
        /// <returns></returns>
        public static T I<T>(T info, int stackDepth = 2) {
            var itemLog = new LogItem() { Grade = LOG_GRADE.info, Info = info.ToString() };
            itemLog.Head = Mode == MODE.DEBUG ? "I/" + GetTraceMsg(stackDepth) : "I: ";
            OnNewLog(itemLog);
            return info;
        }

        /// <summary>
        /// warning等级的日志，release模式下不显示调用栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <param name="stackDepth"></param>
        /// <returns></returns>
        public static T W<T>(T info, int stackDepth = 2) {
            var itemLog = new LogItem() { Grade = LOG_GRADE.warning, Info = info.ToString() };
            itemLog.Head = Mode == MODE.DEBUG ? "W/" + GetTraceMsg(stackDepth) : "W: ";
            OnNewLog(itemLog);
            return info;
        }

        /// <summary>
        /// error等级的日志，任何模式下都显示调用栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <param name="stackDepth"></param>
        /// <returns></returns>
        public static T E<T>(T info, int stackDepth = 2) {
            OnNewLog(new LogItem() {
                Grade = LOG_GRADE.error,
                Head = "E/" + GetTraceMsg(stackDepth),
                Info = info.ToString()
            });

            return info;
        }


        public static T Logd<T>(this T logInfo) {
            return D(logInfo, 3);
        }

        public static T LogI<T>(this T logInfo) {
            return I(logInfo, 3);
        }

        public static T LogW<T>(this T logInfo) {
            return W(logInfo, 3);
        }

        public static T LogE<T>(this T logInfo) {
            return E(logInfo, 3);
        }
    }
}

