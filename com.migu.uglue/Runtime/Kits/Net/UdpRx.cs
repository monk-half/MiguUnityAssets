

namespace UGlue.Kit {
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using UGlue.Kit;

    public class UdpRx {
        private IPEndPoint m_NativeEndPoint;
        private IPEndPoint m_RemoteEndPoint;
        private string m_strMulticastAddr;
        private UdpClient m_UdpClient;
        private bool m_bRunning;
        private Action<Msg> OnReceived;
        private SafeQueue<Msg> m_queMsg;
        public struct Msg {
            public IPEndPoint endPoint;
            public byte[] content;
        }
        public static readonly string[] MulticastAddr = {"226.1.1.2" , "226.1.1.3", "226.1.1.4", "266.1.1.5"};
        public IPEndPoint IPEndPoint { get { return m_NativeEndPoint; } }


        public UdpRx(int port) {
            Init(port);
        }

        /// <summary>
        /// 组播模式
        /// IP组播通信需要一个特殊的组播地址，
        /// IP组播地址是一组D类IP地址，范围从224.0.0.0 到239.255.255.255。
        /// 其中还有很多地址是为特殊的目的保留的。224.0.0.0到224.0.0.255的地址最好不要用，
        /// 因为他们大多是为了特殊的目的保持的（比如IGMP协议）
        /// </summary>
        /// <param name="multicastAddr"></param>
        /// <param name="port"></param>
        public UdpRx(string multicastAddr, int port) {
            Init(port);
            m_strMulticastAddr = multicastAddr;
            JoinMulticast(multicastAddr);
        }

        private void Init(int port) {
            m_queMsg = new SafeQueue<Msg>(20);
            m_NativeEndPoint = new IPEndPoint(IPAddress.Any, port);
            m_UdpClient = new UdpClient(m_NativeEndPoint);
            m_bRunning = true;
            new Thread(ListenLoop).Start();
            new Thread(SendLoop).Start();
        }

        public void UnInit() {
            m_bRunning = false;
        }

        private void ListenLoop() {
            while (m_bRunning) {
                byte[] d = m_UdpClient.Receive(ref m_RemoteEndPoint);
                Msg msg = new Msg() { content = d, endPoint = m_RemoteEndPoint };
                OnReceived?.Invoke(msg);
            }
        }

        private void SendLoop() {
            while (m_bRunning) {
                if (m_queMsg.Count > 0) {
                    Msg data = m_queMsg.Dequeue();
                    m_UdpClient.Send(data.content, data.content.Length, data.endPoint);
                } else {
                    Thread.Sleep(500);
                }
            }

            UnityEngine.Debug.Log("udp 销毁");
            if (m_strMulticastAddr.IsNotNullAndEmpty()) {
                DrpoMulticast(m_strMulticastAddr);
            }
            m_UdpClient.Close();
        }

        public UdpRx JoinMulticast(string ipAddr) {
            m_strMulticastAddr = ipAddr;
            m_UdpClient.JoinMulticastGroup(IPAddress.Parse(ipAddr));
            return this;
        }

        public UdpRx DrpoMulticast(string ipAddr) {
            m_strMulticastAddr = null;
            m_UdpClient.DropMulticastGroup(IPAddress.Parse(ipAddr));
            return this;
        }

        public string GetMulticast() {
            return m_strMulticastAddr;
        }


        public UdpRx Send(string strIP, int port, string data) {
            return Send(IPAddress.Parse(strIP), port, data);
        }

        public UdpRx Send(string strIP, int port, byte[] data) {
            return Send(IPAddress.Parse(strIP), port, data);
        }

        public UdpRx Send(IPAddress IPAddr, int port, string data) {
            return Send(IPAddr, port, data.GetByte());
        }

        public UdpRx Send(IPAddress IPAddr, int port, byte[] data) {
            return Send(new Msg() {
                content = data,
                endPoint = new IPEndPoint(IPAddr, port)
            });
        }

        public UdpRx Send(IPEndPoint iPEnd, string data) {      
            return Send(iPEnd, data.GetByte()); ;
        }

        public UdpRx Send(IPEndPoint iped, byte[] data) {
            return Send(new Msg() {
                content = data,
                endPoint = iped
            });
        }

        public UdpRx Send(Msg msg, bool bIm = true) {
            if (!m_bRunning) {
                return this;
            }

            if (bIm) {
                m_UdpClient.Send(msg.content, msg.content.Length, msg.endPoint);
            } else {
                m_queMsg.Enqueue(msg);
            }
            return this;
        }

        public UdpRx BroadCast(int port, string data) {
            return BroadCast(port, data.GetByte());
        }

        public UdpRx BroadCast(int port, byte[] data) {
            return Send(new IPEndPoint(IPAddress.Broadcast, port), data);
        }

        public UdpRx Listen(Action<Msg> action) {
            if (!action.IsNull()) {
                OnReceived += action;
            }
            return this;
        }
    }


}


