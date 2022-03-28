namespace UGlue.Kit {
    using System;
    using System.Collections.Generic;
    using System.Net;
    using UnityEngine;
    /// <summary>
    /// TODO 缺乏掉线机制
    /// 【完成】同设备不同端口通信
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UdpBus<T> {

        /// <summary>
        /// 接收端口，支持的发送端口集合(考虑同设备多端口通信的问题)
        /// </summary>
        /// <param name="port"></param>
        /// <param name="portSet"></param>
        public UdpBus(int port, int[] portSet, bool hideSelf = false) {
            Init(port, portSet);
            if (!hideSelf) {
                SendHandShake();
            }
        }

        public UdpBus(int port, int basePort, int length, bool hideSelf = false) {
            int[] ports = new int[length];
            for (int i = 0; i < length; i++) {
                ports[i] = basePort + i;
            }

            Init(port, ports);
            if (!hideSelf) {
                SendHandShake();
            }
        }

        private UdpRx m_UdpRx;
        private int[] m_iPortSet;
        private enum CMD {
            FindDevices,
            HandShake,
        }
        private List<IPEndPoint> m_lstIPEndPoint;
        public Action<IPEndPoint, T> OnCommonMsg;
        public Action<IPEndPoint> OnNewDevice;

        private bool m_bOnline = false; //在线状态，被动方接收到握手消息，发送方接收到回复消息。
        public bool OnLine { get { return m_bOnline; } }

        private void Init(int port, int[] portSet) {
            m_lstIPEndPoint = new List<IPEndPoint>();
            m_iPortSet = portSet;
            m_UdpRx = new UdpRx(UdpRx.MulticastAddr[0], port).Listen(OnData);
        }

        public void UnInit() {
            m_UdpRx?.UnInit();
            m_UdpRx = null;
            m_lstIPEndPoint?.Clear();
            OnCommonMsg = null;
            OnNewDevice = null;
        }

        private void OnData(UdpRx.Msg msg) {
            if (IsSelfMsg(msg.endPoint)) {
                //Debug.Log("Filter Self Udp Msg");
                return;
            }

            string content = msg.content.GetString();

            if (content.Equals(CMD.FindDevices.ToString())) {
                Debug.Log("Got FindDevices Msg:" + msg.endPoint + ", " + m_UdpRx.IPEndPoint);
                Debug.Log("Send Reply");
                SendHandShake();
                m_bOnline = true;
            } else if(content.Equals(CMD.HandShake.ToString())){
                foreach (var item in m_lstIPEndPoint) { //过滤已存在的设备，防止重复添加
                    if (item.ToString().Equals(msg.endPoint.ToString())) {
                        Debug.Log("Device Already Exist: " + msg.endPoint);
                        return;
                    }
                }
                Debug.Log("Got HandShake Msg From: " + msg.endPoint);
                m_lstIPEndPoint.Add(msg.endPoint);
                OnNewDevice?.Invoke(msg.endPoint);
                m_bOnline = true;
            }else{
                OnCommonMsg?.Invoke(msg.endPoint, content.ToJsonObj<T>()); //反序列化为T，回调
            }
        }

        private bool IsSelfMsg(IPEndPoint endPoint) {
            foreach (var item in IPTool.GetIPArray()) {
                if (item.Equals(IPTool.ToIPV4(endPoint.Address)) && endPoint.Port.Equals(m_UdpRx.IPEndPoint.Port)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 单播发送
        /// </summary>
        /// <param name="ipend"></param>
        /// <param name="data"></param>
        private void Send(IPEndPoint ipend, byte[] data) {
            m_UdpRx?.Send(ipend, data);
        }

        /// <summary>
        /// 组播发送
        /// </summary>
        /// <param name="data"></param>
        private void Send(byte[] data) {
            m_iPortSet.ForEach(port => { m_UdpRx?.Send(UdpRx.MulticastAddr[0], port, data); });
        }


        /// <summary>
        /// 让组播网络中的设备发现自己(加入组播网络)
        /// </summary>
        private UdpBus<T> SendHandShake() {
            Send(CMD.HandShake.ToString().GetByte()); //在线回复
            return this;
        }

        /// <summary>
        /// 查找组播网络中的设备
        /// </summary>
        /// <returns></returns>
        public UdpBus<T> FindDevices() {
            Send(CMD.FindDevices.ToString().GetByte());
            return this;
        }

        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <returns></returns>
        public List<IPEndPoint> GetDevices() {
            return m_lstIPEndPoint;
        }

        /// <summary>
        /// 组播发送
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public UdpBus<T> Send(T t) {
            Send(t.ToJsonStr().GetByte());
            return this;
        }

        /// <summary>
        /// 单播发送
        /// </summary>
        /// <param name="ipend"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public UdpBus<T> Send(IPEndPoint ipend, T t) {
            Send(ipend, t.ToJsonStr().GetByte());
            return this;
        }

    }
}
