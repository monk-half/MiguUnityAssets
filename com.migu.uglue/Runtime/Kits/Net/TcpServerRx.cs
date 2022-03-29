namespace UGlue.Kit {
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class TcpServerRx {
        TcpListener m_TcpListener;

        //Tcp server actions
        public Action m_actOnServerStart = null;
        public Action<Exception> m_actOnServerErr = null;
        public Action m_actOnServerStop = null;

        //Tcp client actions
        public Action<TcpClient> m_actOnClientConnect = null;
        public Action<TcpClient, int, byte[]> m_actOnClientData = null;
        public Action<TcpClient> m_actOnClientDead = null;


        public TcpServerRx(IPEndPoint localEP) {
            m_TcpListener = new TcpListener(localEP);
        }
        public TcpServerRx(IPAddress localaddr, int port) {
            m_TcpListener = new TcpListener(localaddr, port);
        }
        public TcpServerRx(int port) {
            m_TcpListener = new TcpListener(IPAddress.Any, port); // 监听本机端口
        }

        public TcpServerRx OnServerErr(Action<Exception> actErr) {
            if (actErr != null) {
                m_actOnServerErr += actErr;
            }
            return this;
        }

        public TcpServerRx OnServerStart(Action actStart) {
            if (actStart != null) {
                m_actOnServerStart += actStart;
            }
            return this;
        }

        public TcpServerRx OnServerStop(Action actStop) {
            if (actStop != null) {
                m_actOnServerStop += actStop;
            }
            return this;
        }

        public TcpServerRx OnClientConnect(Action<TcpClient> actConn) {
            if (actConn != null) {
                m_actOnClientConnect += actConn;
            }
            return this;
        }

        public TcpServerRx OnClientClose(Action<TcpClient> actClose) {
            if (actClose != null) {
                m_actOnClientDead += actClose;
            }
            return this;
        }

        public TcpServerRx OnClientData(Action<TcpClient, int, byte[]> actData) {
            if (actData != null) {
                m_actOnClientData += actData;
            }
            return this;
        }

        public TcpServerRx Start() {
            new Thread(StartServer).Start();
            return this;
        }

        private void StartServer() {
            try {
                m_TcpListener.Start();
                m_actOnServerStart?.Invoke();
                Log.I("Tcp server start");
            } catch (Exception e) {
                m_actOnServerErr?.Invoke(e); //invoke while not null
            }

            while (true) {
                try {
                    TcpClient client = m_TcpListener.AcceptTcpClient();
                    new Thread(new ParameterizedThreadStart(HandleClientMsg)).Start(client);
                } catch (Exception e) {
                    Log.W("Tcp server error: " + e);
                    break;
                }
            }

            m_TcpListener.Stop();
            m_actOnServerStop?.Invoke();
            Log.I("Tcp server closed");
        }


        public bool Send(TcpClient clientIn, int lenIn, byte[] dataIn) {
            try {
                clientIn.GetStream().Write(dataIn, 0, lenIn);
                clientIn.GetStream().Flush();//refresh stream
            } catch (Exception e){
                Log.W("send Tcp message error:" + e);
                return false;
            }
            return true;
        }

        public void Stop() {
            Log.I("Tcp server closed");
            m_TcpListener.Stop();
            m_actOnServerStop?.Invoke();
        }


        private void HandleClientMsg(object client) {
            TcpClient tcpClient = (TcpClient)client;
            Log.I("TcpServer start a Tcp client stream");
            m_actOnClientConnect?.Invoke(tcpClient);
            NetworkStream clientStream = tcpClient.GetStream();

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
                Log.I("Tcp Server got message");
                m_actOnClientData?.Invoke(tcpClient, bytesRead, message);             
            }

            Log.W("TcpServer stop a Tcp client");
            m_actOnClientDead?.Invoke(tcpClient);    
            clientStream.Close();
            tcpClient.Close();
        }


    }
}
