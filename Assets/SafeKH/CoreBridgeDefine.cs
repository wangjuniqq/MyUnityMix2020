public class CBDefine
{

    private static ICoreBridge m_coreBridge;

    private static bool m_isInit = false;

    public static void InitCoreBridge<T>() where T : ICoreBridge, new()
    {
        if (m_isInit) return;
        m_coreBridge = new T();
        m_isInit = true;
    }

    public static ICoreBridge GetCoreBridge()
    {
        return m_coreBridge;
    }
}