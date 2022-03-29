namespace UGlue.Kit {
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    public class TcpClientRx {
        private TcpClient m_TcpClient;
        private readonly IPEndPoint m_ServerEndPoint;
        private readonly string m_strIp;
        public string IP { get{ return m_strIp; } }
        public TcpClient Client { get { return m_TcpClient; } }
        public TcpClientRx(string strIP, int strPort) {
            m_strIp = strIP;
            m_TcpClient = new TcpClient();
            m_ServerEndPoint = new IPEndPoint(IPAddress.Parse(strIP), strPort);
        }

        public Action<TcpClient> m_actOnConn;
        public Action<TcpClient, Exception> m_actOnError;
        public Action<TcpClient> m_actOnClosed;
        public Action<TcpClient, int, byte[]> m_actOnData;

        public TcpClientRx OnClientConnected(Action<TcpClient> actOnConnIn) {
            if (actOnConnIn != null) {
                m_actOnConn += actOnConnIn;
            }
            return this;
        }

        public TcpClientRx OnClientError(Action<TcpClient, Exception> actOnErrIn) {
            if (actOnErrIn != null) {
                m_actOnError += actOnErrIn;
            }
            return this;
        }

        public TcpClientRx OnClientClosed(Action<TcpClient> actOnClosedIn) {
            if (actOnClosedIn != null) {
                m_actOnClosed += actOnClosedIn;
            }
            return this;
        }

        public TcpClientRx OnGetData(Action<TcpClient, int, byte[]> actOnDataIn) {
            if (actOnDataIn != null) {
                m_actOnData += actOnDataIn;
            }
            return this;
        }


        public TcpClientRx Connect() {
            new Thread(() => {
                try {
                    Connect2Server();
                } catch (Exception e) {
                    Log.I("Tcp client error");
                    m_actOnError?.Invoke(m_TcpClient, e);
                }
            }).Start();
            return this;
        }

        public void Disconnect() {
            m_TcpClient.Close();
        }

        private void Connect2Server() {
            m_TcpClient.Connect(m_ServerEndPoint); //阻塞
            NetworkStream clientStream = m_TcpClient.GetStream();
            Log.I("Tcp client connected");
            m_actOnConn?.Invoke(m_TcpClient);

            byte[] message = new byte[4096];
            int bytesRead;
            while (true) {
                bytesRead = 0;
                try {
                    bytesRead = clientStream.Read(message, 0, 4096);
                } catch {
                    break;
                }
                if (bytesRead == 0) {
                    break;
                }
                Log.I("socket client got data length: " + bytesRead);
                m_actOnData?.Invoke(m_TcpClient, bytesRead, message);
            }

            Log.I("Tcp client closed");
            m_actOnClosed?.Invoke(m_TcpClient);
            clientStream.Close();
            m_TcpClient.Close();
        }

        public bool Send(TcpClient clientIn, int lenIn, byte[] dataIn) {
            try {
                clientIn.GetStream().Write(dataIn, 0, lenIn);
                clientIn.GetStream().Flush();//refresh stream
            } catch (Exception e) {
                Log.W("send Tcp message error:" + e);
                return false;
            }
            return true;
        }
    }
}
