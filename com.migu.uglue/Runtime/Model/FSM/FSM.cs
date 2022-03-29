namespace UGlue
{
    using System.Collections.Generic;

	public class FSM{

		public delegate void FSMCallfunc(params object[] param);
        private List<FSMTranslation> transList = new List<FSMTranslation>();
        private string mCurState;

        private struct FSMTranslation{
			public string fromState;
			public string translation;
			public string toState;
			public FSMCallfunc transCallback; // 回调函数

			public FSMTranslation(string fromStateIn, string transIn, string toStateIn, FSMCallfunc callback){
				fromState = fromStateIn;
				toState = toStateIn;
				translation = transIn;
                transCallback = callback;
			}
		}

        /// <summary>
        /// 增加状态机关系链
        /// </summary>
        /// <param name="fromState">起始状态</param>
        /// <param name="strAction">事件</param>
        /// <param name="toState">结束状态</param>
        /// <param name="callfunc">状态切换回调</param>
        /// <returns></returns>
        public FSM AddTranslation(string fromState, string strAction, string toState, FSMCallfunc callfunc){
            transList.Add(new FSMTranslation(fromState, strAction, toState, callfunc));
            return this;
		}

        /// <summary>
        /// 开启状态机，赋予初始状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
		public FSM Start(string state){
			mCurState = state;
            return this;
		}

        /// <summary>
        /// 获取当前状态
        /// </summary>
        /// <returns></returns>
        public string GetCurrState() {
            return mCurState;
        }

        /// <summary>
        /// 事件驱动状态机转换
        /// </summary>
        /// <param name="strAction"> 事件名称</param>
        /// <param name="param">事件参数</param>
        public void HandleEvent(string strAction, params object[] param){
            if (mCurState != null) {
                foreach (FSMTranslation fsmt in transList) {
                    if (fsmt.fromState.Equals(mCurState) && fsmt.translation.Equals(strAction)) {
                        mCurState = fsmt.toState;
                        fsmt.transCallback(param);
                        break;
                    }
                }
            }
		}

        /// <summary>
        /// 清除所有状态转换关系
        /// </summary>
		public void Clear(){
			transList.Clear();
		}
	}
}