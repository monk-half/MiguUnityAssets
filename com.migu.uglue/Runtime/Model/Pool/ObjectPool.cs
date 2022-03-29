namespace UGlue {
    using System;
    using UGlue.Kit;

    public class ObjectPool<T> where T : class{
        public SafeStack<T> m_stkObj = new SafeStack<T>();
        public int Size = 0;

        public ObjectPool(int size) {
            Size = size;
        }

        public ObjectPool<T> Create(Func<T> func) {
            while (m_stkObj.Count < Size) {
                m_stkObj.Push(func());
            }
            return this;
        }

        public bool Push(T t) {
            if (m_stkObj.Count >= Size) {
                return false;
            }
            foreach (var item in m_stkObj) {//防止重复添加
                if (item == t) {//地址相同，不是值相同
                    Log.I("object already exist");
                    return false;
                }
            }
            m_stkObj.Push(t);
            return true;
        }

        public T Pop() {
            if (m_stkObj.Count <= 0) {
                return null;
            }
            return m_stkObj.Pop();
        } 
    }
}
