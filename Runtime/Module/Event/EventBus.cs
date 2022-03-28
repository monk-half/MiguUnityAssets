namespace UGlue {
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UGlue.Kit;

    public static class JsonExterntion {
        public static T ReType<T>(this object obj) {
            return obj.ReTypeJson<T>();
        }
    }

    public class EventBus : MonoSingleton<EventBus> {
        public override void OnSingletonInit() {
            base.OnSingletonInit();
        }

        public static void Init() {
            Log.I(Instance == null ? "EventBus Init Failed" : "EventBus Init Success");
        }

        public enum POST_THREAD { Posting, Main, BackGround }
        public enum POST_CHANNEL { Native = 0x001, Remote = 0x002, All = Native | Remote}
        public struct EventHead {
            public string key;
            public POST_CHANNEL channel;
            public IPEndPoint iPEndPoint;
        }
        public enum EVENT_PORT {
            Start = LogRemote.LOG_PORT.End,
            Port1,
            Port2,
            Port3,
            Port4,
            Port5,
            End,
        }
        private struct RemoteMsg {
            public string m_strEvent;
            public object[] m_arrParams;
        }
        public delegate void OnEvent(EventHead head, params object[] param);
        private Dictionary<string, OnEvent> m_dicNativeEvent = new Dictionary<string, OnEvent>();
        private Dictionary<string, OnEvent> m_dicRemoteEvent = new Dictionary<string, OnEvent>();
        private UdpBus<RemoteMsg> m_UdpBus = null;

        private void CreateUdpRemote(int port) {
            if (m_UdpBus.IsNull()) {
                m_UdpBus = new UdpBus<RemoteMsg>(port, (int)EVENT_PORT.Port1, EVENT_PORT.End - EVENT_PORT.Port1);
                m_UdpBus.OnCommonMsg += OnRemoteMsg;
            }
        }

        private void OnRemoteMsg(IPEndPoint ipend, RemoteMsg msg) {
            Log.I("收到远程消息：" + ipend);
            EventHead head = new EventHead() {
                channel = POST_CHANNEL.Remote,
                iPEndPoint = ipend,
                key = msg.m_strEvent
            };
            PostEvent(m_dicRemoteEvent, head, msg.m_arrParams);
        }

        private bool PostRemoteMsg(IPEndPoint ipend, RemoteMsg msg) {
            if (ipend.IsNull()) {
                m_UdpBus?.Send(msg);
            } else {
                m_UdpBus?.Send(ipend, msg);
            }
            return true;
        }

        private bool AddEvent(string key, OnEvent func, POST_CHANNEL channel) {
            bool ret = true;
            
            if ((channel & POST_CHANNEL.Remote) == POST_CHANNEL.Remote) {
                ret &= AddEvent(key, func, m_dicRemoteEvent);
            }
            if ((channel & POST_CHANNEL.Native) == POST_CHANNEL.Native) {
                ret &= AddEvent(key, func, m_dicNativeEvent);
            }
            return ret;
        }

        private bool AddEvent(string key, OnEvent func, Dictionary<string, OnEvent> events) {
            //无该事件记录
            if (!events.ContainsKey(key)) {
                events.Add(key, func);
                Log.D("注册首个事件" + key + ", " + func.Target + "." + func.Method.Name);
                return true;
            }

            //有记录 重复
            events.TryGetValue(key, out OnEvent onEvent);
            if (onEvent.Equals(func)) {
                Log.D("事件重复注册" + key + ", " + func);
                return false;
            }

            onEvent += func;
            events[key] = onEvent;
            Log.D("注册事件：" + key + ", " + func.Method);
            return true;
        }

        private bool RemoveEvent(string key, OnEvent func, POST_CHANNEL channel) {
            bool ret = true;
            if ((channel & POST_CHANNEL.Remote) == POST_CHANNEL.Remote) {
                ret &= RemoveEvent(key, func, m_dicRemoteEvent);
            }
            if ((channel & POST_CHANNEL.Native) == POST_CHANNEL.Native) {
                ret &= RemoveEvent(key, func, m_dicNativeEvent);
            }
            return ret;
        }

        private bool RemoveEvent(string key, OnEvent func, Dictionary<string, OnEvent> events) {
            if (!events.ContainsKey(key)) {
                Log.D("无可注销事件：" + key + ", " + func);
                return false;
            }

            events.TryGetValue(key, out OnEvent onEvent);
            onEvent -= func;
            if (onEvent == null) {
                events.Remove(key);
            } else {
                events[key] = onEvent;
            }
            Log.D("注销事件：" + key + ", " + func);
            return true;
        }

        private bool PostEvent(Dictionary<string, OnEvent> events, EventHead head, params object[] param) {
            if (!events.ContainsKey(head.key)) {
                return false;
            }

            events.TryGetValue(head.key, out OnEvent onEvent);
            try {
                onEvent?.Invoke(head, param);
            } catch (Exception e){
                Log.W(e);
            }

            return true;
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="key">方法名</param>
        /// <param name="func">回调</param>
        /// <param name="channel">通道，默认订阅本地通道</param>
        /// <returns></returns>
        public static bool Register(string key, OnEvent func, POST_CHANNEL channel = POST_CHANNEL.Native) {
            return Instance.AddEvent(key, func, channel);
        }

        /// <summary>
        /// 反订阅消息
        /// </summary>
        /// <param name="key">方法名</param>
        /// <param name="func">回调</param>
        /// <param name="channel">通道，默认全部反订阅</param>
        /// <returns></returns>
        public static bool UnRegister(string key, OnEvent func, POST_CHANNEL channel = POST_CHANNEL.All) {
            return Instance.RemoveEvent(key, func, channel);
        }

        //发布消息
        public static bool Post(string keyIn, params object[] param) {
            var head = new EventHead() { key = keyIn, channel = POST_CHANNEL.Native, iPEndPoint = null };
            return Instance.PostEvent(Instance.m_dicNativeEvent, head, param);
        }

        //本地消息
        public static bool Post(string keyIn, POST_THREAD thread = POST_THREAD.Posting, params object[] param) {
            switch (thread) {
                case POST_THREAD.BackGround:
                    Dispatcher.InvokeAsync(() => Post(keyIn, param));
                    return true;
                case POST_THREAD.Main:
                    Dispatcher.InvokeMain(() => Post(keyIn, param));
                    return true;
                case POST_THREAD.Posting:
                default:
                    return Post(keyIn, param);
            }
        }

        public static void EnableRemoteEvent(EVENT_PORT port = EVENT_PORT.Port1) {
            Instance.CreateUdpRemote((int)port);
        }

        //远程单播
        public static void PostRemote(string keyIn,IPEndPoint ipend, params object[] param) {
            RemoteMsg remoteMsg = new RemoteMsg() {m_arrParams = param, m_strEvent = keyIn};
            Instance.PostRemoteMsg(ipend, remoteMsg);
        }

        //多通道，远程默认广播
        public static void Post(string keyIn, POST_CHANNEL channel, params object[] param) {
            if ((channel & POST_CHANNEL.Native) == POST_CHANNEL.Native) {
                Post(keyIn, param);
            }
            if ((channel & POST_CHANNEL.Remote) == POST_CHANNEL.Remote) {
                PostRemote(keyIn, null, param);
            }
        }
    }
}
