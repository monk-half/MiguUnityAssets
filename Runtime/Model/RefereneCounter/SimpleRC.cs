namespace UGlue {
    /// <summary>
    /// TODO  未解决同一个对象重复引用的问题，多线程问题
    /// </summary>
    public class SimpleRC{
        public SimpleRC() {
            RefCount = 0;
        }

        public int RefCount { get; private set; }


        public void Retain(object refOwner = null) {
            ++RefCount;
        }

        public void Release(object refOwner = null) {
            --RefCount;
            if (RefCount == 0) {
                OnZeroRef();
            }
        }

        /// <summary>
        /// 无人引用回调
        /// </summary>
        protected virtual void OnZeroRef() {
        }
    }
}
