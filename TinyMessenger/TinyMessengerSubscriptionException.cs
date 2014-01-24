using System;

namespace TinyMessenger {
  /// <summary>
  ///   Thrown when an exceptions occurs while subscribing to a message type
  /// </summary>
  public class TinyMessengerSubscriptionException : Exception {
    private const string ErrorText = "Unable to add subscription for {0} : {1}";

    public TinyMessengerSubscriptionException(Type messageType, string reason)
      : base(String.Format(ErrorText, messageType, reason)) {}

    public TinyMessengerSubscriptionException(Type messageType, string reason, Exception innerException)
      : base(String.Format(ErrorText, messageType, reason), innerException) {}
  }
}
