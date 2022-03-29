namespace UGlue{
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using UGlue.Kit;

    public class EventRemoteClient : Singleton<EventRemoteClient> {
        private EventRemoteClient() { }


        private Dictionary<TcpClientRx, Dictionary<string, OnEvent>> m_dicClient;

        public delegate void OnEvent(string key, params object[] param);
        
        public override void OnSingletonInit() {
            base.OnSingletonInit();
            m_dicClient = new Dictionary<TcpClientRx, Dictionary<string, OnEvent>>();
        }

        private void RegisterClient(string strIpIn, string methodIn, OnEvent eventIn) {
            foreach (var item in m_dicClient) {
                if(item.Key.IP.Equals(strIpIn)){
                    if (item.Value.TryGetValue(methodIn, out OnEvent eventOut)) {
                        eventOut += eventIn;
                    } else {
                        item.Value.Add(methodIn, eventIn);
                    }
                    return;
                }
            }
            m_dicClient.Add(CreateTcpClient(strIpIn, methodIn), new Dictionary<string, OnEvent>() { { methodIn, eventIn } });
        }


        private TcpClientRx CreateTcpClient(string strIP, string methodIn) {
            string strMsg = new EventRemoteMsg(EventRemoteMsg.MsgType.Register, methodIn).ToJsonStr();

            return new TcpClientRx(strIP, 3000)
                .OnClientConnected((c)=>{    
                    Log.I("发送字符串：" + strMsg);
                    byte[] buffer = Encoding.UTF8.GetBytes(strMsg);
                    c.GetStream().Write(buffer, 0, buffer.Length);
                    c.GetStream().Flush();
                })
                .OnClientError((c, e) => Log.I("tcp 出错" + e))
                .OnClientClosed((c) => Log.I("tcp 关闭"))
                .OnGetData(OnData)
                .Connect();
        }

        private void OnData(TcpClient client, int length, byte[] data) {
            string s = Encoding.UTF8.GetString(data, 0, length);
            Log.I("msg: " + s);
            EventRemoteMsg smsg = s.ToJsonObj<EventRemoteMsg>();

            foreach (var item in m_dicClient) {
                if (item.Key.Client.Equals(client)) {
                    if (item.Value.TryGetValue(smsg.m_strEvent, out OnEvent eventOut)) {
                        eventOut(smsg.m_strEvent, smsg.m_arrParams);
                    }
                }
            }
            
        }

        public static void Register(string strIpIn, string methodIn, OnEvent eventIn) {
            Instance.RegisterClient(strIpIn, methodIn, eventIn);
        }

    }
}
