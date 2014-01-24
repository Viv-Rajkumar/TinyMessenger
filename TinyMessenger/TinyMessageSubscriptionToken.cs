using System;
using System.Reflection;
using TinyMessenger.Interfaces;

namespace TinyMessenger {
  /// <summary>
  ///   Represents an active subscription to a message
  /// </summary>
  public sealed class TinyMessageSubscriptionToken : IDisposable {
    private readonly WeakReference _hub;
    private readonly Type _messageType;

    /// <summary>
    ///   Initializes a new instance of the TinyMessageSubscriptionToken class.
    /// </summary>
    public TinyMessageSubscriptionToken(ITinyMessengerHub hub, Type messageType) {
      if (hub == null)
        throw new ArgumentNullException("hub");

      if (!typeof(ITinyMessage).GetTypeInfo().IsAssignableFrom(messageType.GetTypeInfo()))
        throw new ArgumentOutOfRangeException("messageType");

      _hub = new WeakReference(hub);
      _messageType = messageType;
    }

    public void Dispose() {
      if (_hub.IsAlive) {
        var hub = _hub.Target as ITinyMessengerHub;

        if (hub != null) {
          var unsubscribeMethod = typeof(ITinyMessengerHub).GetRuntimeMethod(
            "Unsubscribe",
            new[] {typeof(TinyMessageSubscriptionToken)});
          unsubscribeMethod = unsubscribeMethod.MakeGenericMethod(_messageType);
          unsubscribeMethod.Invoke(hub, new object[] {this});
        }
      }

// ReSharper disable once GCSuppressFinalizeForTypeWithoutDestructor
      GC.SuppressFinalize(this);
    }
  }
}
