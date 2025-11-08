namespace GuiFramework
{
    public enum AudioCategory
    {
        BackgroundMusic,    // 背景音乐
        Ambient,           // 环境音效
        UI,                // 界面音效
        Character,         // 角色音效
        Environment,       // 环境交互音效
        SpecialEffect      // 特殊效果音效
    }
    
    public enum AudioPriority
    {
        Low = 0,
        Normal = 128,
        High = 256
    }
}