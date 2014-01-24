using System;
using TinyMessenger.Interfaces;

namespace TinyMessenger {
  /// <summary>
  ///   Base class for messages that provides weak refrence storage of the sender
  /// </summary>
  public abstract class TinyMessageBase : ITinyMessage {
    /// <summary>
    ///   Store a WeakReference to the sender just in case anyone is daft enough to
    ///   keep the message around and prevent the sender from being collected.
    /// </summary>
    private readonly WeakReference _sender;

    /// <summary>
    ///   Initializes a new instance of the MessageBase class.
    /// </summary>
    /// <param name="sender">Message sender (usually "this")</param>
    protected TinyMessageBase(object sender) {
      if (sender == null)
        throw new ArgumentNullException("sender");

      _sender = new WeakReference(sender);
    }

    public object Sender {
      get {
        return (_sender == null) ? null : _sender.Target;
      }
    }
  }
}
