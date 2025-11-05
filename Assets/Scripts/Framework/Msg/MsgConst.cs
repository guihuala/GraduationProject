namespace GuiFramework
{
    public class MsgConst
    {
        #region 输入相关 10开头
        
        public const int ON_INTERACTION_PRESS = 10001; //输入 - 交互键按下
        public const int ON_DASH_PRESS = 10002; //输入 - 冲刺键按下
        public const int ON_USE_PRESS = 10003; //输入 - 使用键按下
        public const int ON_USE_LONG_PRESS = 10004; //输入 - 使用键长按
        public const int ON_GAMEPAUSE_PRESS = 10005; //输入 - 暂停键按下
        
        public const int ON_UI_INTERACTION_PRESS = 10101; //输入 - 交互键按下

        #endregion
        
        
        
        #region 系统相关 99开头
        public const int ON_CONTROL_MAP_CHG = 99001;//系统 - 输入映射方式切换 Game/UI
        public const int ON_CONTROL_TYPE_CHG = 99002;//系统 - 输入方式切换 键盘/手柄
        public const int ON_LANGUAGE_CHG = 99003;//系统 - 语言切换
        #endregion
    }
}