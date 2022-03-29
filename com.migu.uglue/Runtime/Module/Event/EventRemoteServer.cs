namespace UGlue {
    using UGlue.Kit;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;

    public class EventRemoteServer : Singleton<EventRemoteServer> {
        private EventRemoteServer() { }


        private TcpServerRx TcpServerRx;
        private Dictionary<TcpClient, List<string>> m_dicMethods; //客户端， 方法列表

        public override void OnSingletonInit() {
            base.OnSingletonInit();
            m_dicMethods = new Dictionary<TcpClient, List<string>>();

            TcpServerRx = new TcpServerRx(3000)
                .OnServerStop(() => m_dicMethods.Clear())
                .OnServerErr(e => m_dicMethods.Clear())
                .OnClientConnect(OnClientConn)
                .OnClientClose(OnClientDisConn)
                .OnClientData(OnGetData)
                .Start();
        }

        private void OnClientConn(TcpClient clientIn) {
            if (m_dicMethods.ContainsKey(clientIn)) {
                Log.I("客户端重复连接");
                return;
            }
            m_dicMethods.Add(clientIn, new List<string>());
            Log.I("添加新客户端");
        }

        private void OnClientDisConn(TcpClient clientIn) {
            if (!m_dicMethods.ContainsKey(clientIn)) {
                Log.I("无该客户端");
                return;
            }
            m_dicMethods.Remove(clientIn);
            Log.I("移除掉线客户端");
        }

        private void OnGetData(TcpClient clientIn, int lenIn, byte[] data) {
            string s = Encoding.UTF8.GetString(data, 0, lenIn);
            EventRemoteMsg msg = s.ToJsonObj<EventRemoteMsg>();

            switch (msg.m_iMsgType) {
                case EventRemoteMsg.MsgType.Register:
                    if (string.IsNullOrEmpty(msg.m_strEvent)) {
                        Log.D("注册事件为空");
                        return;
                    }
                    if (!m_dicMethods.TryGetValue(clientIn, out List<string> lstEvents)) {
                        Log.D("找不到client");
                        return;
                    }
                    if (lstEvents.Contains(msg.m_strEvent)) {
                        Log.D("重复注册");
                    }

                    lstEvents.Add(msg.m_strEvent);
                    Log.I("事件添加成功：" + msg.m_strEvent);
                    break;
                case EventRemoteMsg.MsgType.Unregister:
                    if (string.IsNullOrEmpty(msg.m_strEvent)) {
                        Log.D("注销事件为空");
                        return;
                    }
                    if (!m_dicMethods.TryGetValue(clientIn, out List<string> lstEvents2)) {
                        Log.D("找不到client");
                        return;
                    }
                    if (!lstEvents2.Contains(msg.m_strEvent)) {
                        Log.D("找不到该方法");
                    }

                    lstEvents2.Remove(msg.m_strEvent);
                    Log.I("事件添加成功：" + msg.m_strEvent);
                    break;
                case EventRemoteMsg.MsgType.Event:
                    Log.I("检测到客户端事件消息，暂未支持该通路");
                    break;
                default:
                    Log.W("无法解析socket消息");
                    return;
            }
        }

        //效率低，改成若符合条件则进行序列化、转为byte。且只进行一次。
        public void Post(string strEventIn, params object[] paramsIn) {
            string postStr = new EventRemoteMsg(EventRemoteMsg.MsgType.Event, strEventIn, paramsIn).ToJsonStr();
            byte[] buffer = Encoding.UTF8.GetBytes(postStr);

            foreach (var item in m_dicMethods) {
                if (item.Value.Contains(strEventIn)) {
                    Log.D("发送消息: " + postStr);
                    item.Key.GetStream().Write(buffer, 0, buffer.Length);
                    item.Key.GetStream().Flush();
                }
            }
        }

        public void HangOn() {
            Log.W("EventBusRemote初始化成功");
        }
    }
}
