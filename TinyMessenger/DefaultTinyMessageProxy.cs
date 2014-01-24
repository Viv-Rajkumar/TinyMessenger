using TinyMessenger.Interfaces;

namespace TinyMessenger {
  /// <summary>
  ///   Default "pass through" proxy.
  ///   Does nothing other than deliver the message.
  /// </summary>
  public sealed class DefaultTinyMessageProxy : ITinyMessageProxy {
// ReSharper disable once InconsistentNaming
    private static readonly DefaultTinyMessageProxy _Instance = new DefaultTinyMessageProxy();

    static DefaultTinyMessageProxy() {}

    private DefaultTinyMessageProxy() {}

    /// <summary>
    ///   Singleton instance of the proxy.
    /// </summary>
    public static DefaultTinyMessageProxy Instance {
      get {
        return _Instance;
      }
    }

    public void Deliver(ITinyMessage message, ITinyMessageSubscription subscription) {
      subscription.Deliver(message);
    }
  }
}
