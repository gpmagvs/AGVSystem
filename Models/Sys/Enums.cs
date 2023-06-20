namespace AGVSystem.Models.Sys
{
    /// <summary>
    /// 系統運轉狀態
    /// </summary>
    public enum RUN_MODE
    {
        MAINTAIN,
        RUN
    }

    /// <summary>
    /// HOST連線狀態
    /// </summary>
    public enum HOST_CONN_MODE
    {
        OFFLINE,
        ONLINE
    }

    /// <summary>
    /// HOST 模式
    /// </summary>
    public enum HOST_OPER_MODE
    {
        LOCAL,
        REMOTE
    }

}
