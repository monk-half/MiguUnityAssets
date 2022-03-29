namespace UGlue {
    public class EventRemoteMsg {
        public enum MsgType { Register, Unregister, Event , HandShake}
        public MsgType m_iMsgType;
        public string m_strEvent;
        public object[] m_arrParams;
        public EventRemoteMsg(MsgType typeIn, string strEventIn, params object[] paramsIn) {
            m_iMsgType = typeIn;
            m_strEvent = strEventIn;
            m_arrParams = paramsIn;
        }
    }
}