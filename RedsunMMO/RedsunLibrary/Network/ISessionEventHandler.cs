namespace RedsunLibrary.Network
{
    public interface ISessionEventHandler
    {
        bool onConnected();
        bool onConnectFailed();
        void onDisconnected();
        void onReceived();
        void onInvaliedReceived();
    }
}
