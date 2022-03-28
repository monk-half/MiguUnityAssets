

namespace UGlue {
    using System.Net;
    using UGlue.Kit;
    /// <summary>
    /// TODO 考虑直接继承LogBus
    /// </summary>
    public class LogRemote {
        public LogRemote(LOG_PORT port, MODE mode) {
            Init(port, mode);
        }

        private void Init(LOG_PORT port, MODE mode) {
            Mode = mode;
            m_UdpBus = new UdpBus<Log.LogItem>((int)port, (int)LOG_PORT.Port1, LOG_PORT.End-LOG_PORT.Port1);
            m_UdpBus.OnCommonMsg += OnReceived;
        }

        public void UnInit() {
            m_UdpBus.OnCommonMsg -= OnReceived;
            m_UdpBus = null;
        }

        // 同设备不同端口通信
        public enum LOG_PORT {
            Start = 5188,
            Port1,
            Port2,
            Port3,
            Port4,
            Port5,
            End,
        }

        public enum MODE {
            None = 0,
            Send = 1,
            Receive = 2,
            All = 4
        }
        public MODE Mode { get; set; }
        private UdpBus<Log.LogItem> m_UdpBus;

        public void SendMsg(Log.LogItem item) {
            if ((Mode & MODE.Send) == MODE.Send) {
                m_UdpBus?.Send(item);
            }
        }

        public void OnReceived(IPEndPoint ipend, Log.LogItem item) {
            UnityEngine.Debug.Log("远程日志：" + ipend + ", " + item.Head + item.Info);
        }

    }
}

