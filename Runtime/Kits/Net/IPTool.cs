namespace UGlue.Kit {
    using System.Net;

    public static class IPTool {

        /// <summary>
        /// 获取ipv4的ip地址，注意，如果有多个网卡则会返回多个ip地址，选择合适的网卡才能通信
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static IPAddress GetIP(int idx = 0) {
#if NET_35
            return GetIPArr()[idx];
#else
            return GetIPArr()[idx].MapToIPv4();
#endif
        }

        public static IPAddress[] GetIPArray() {
            IPAddress[] addr = GetIPArr();
            for (int i = 0; i < addr.Length; i++) {
#if NET_35
                addr[i] = addr[i];
#else
                addr[i] = addr[i].MapToIPv4();
#endif
            }
            return addr;
        }

        public static IPAddress GetIPV6(int idx = 0) {
#if NET_35
            return GetIPArr()[idx];
#else
            return GetIPArr()[idx].MapToIPv6();
#endif
        }

        public static IPAddress ToIPV4(IPAddress ipaddr) {
#if NET_35
            return ipaddr;
#else
            return ipaddr.MapToIPv4();
#endif
        }

        public static int GetIPLength() {
            return GetIPArr().Length;
        }

        private static IPAddress[] GetIPArr() {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList;
        }

    }

}

