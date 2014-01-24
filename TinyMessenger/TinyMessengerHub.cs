using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TinyMessenger.Interfaces;

namespace TinyMessenger {
  /// <summary>
  ///   Messenger hub responsible for taking subscriptions/publications and delivering of messages.
  /// </summary>
  public sealed class TinyMessengerHub : ITinyMessengerHub {
    #region Private Types and Interfaces

    private static readonly ITinyMessengerHub DefaultInstance = new TinyMessengerHub();

    private class StrongTinyMessageSubscription<TMessage> : ITinyMessageSubscription
      where TMessage : class, ITinyMessage {
      private readonly Action<TMessage> _deliveryAction;
      private readonly Func<TMessage, bool> _messageFilter;
      private readonly TinyMessageSubscriptionToken _subscriptionToken;

      /// <summary>
      ///   Initializes a new instance of the TinyMessageSubscription class.
      /// </summary>
      /// <param name="subscriptionToken">Subscription Token</param>
      /// <param name="deliveryAction">Delivery action</param>
      /// <param name="messageFilter">Filter function</param>
      public StrongTinyMessageSubscription(
        TinyMessageSubscriptionToken subscriptionToken,
        Action<TMessage> deliveryAction,
        Func<TMessage, bool> messageFilter) {
        if (subscriptionToken == null)
          throw new ArgumentNullException("subscriptionToken");

        if (deliveryAction == null)
          throw new ArgumentNullException("deliveryAction");

        if (messageFilter == null)
          throw new ArgumentNullException("messageFilter");

        _subscriptionToken = subscriptionToken;
        _deliveryAction = deliveryAction;
        _messageFilter = messageFilter;
      }

      public TinyMessageSubscriptionToken SubscriptionToken {
        get {
          return _subscriptionToken;
        }
      }

      public bool ShouldAttemptDelivery(ITinyMessage message) {
        if (message == null)
          return false;

        if (!(typeof(TMessage).GetTypeInfo().IsAssignableFrom(message.GetType().GetTypeInfo())))
          return false;

        return _messageFilter.Invoke(message as TMessage);
      }

      public void Deliver(ITinyMessage message) {
        if (!(message is TMessage))
          throw new ArgumentException("Message is not the correct type");

        _deliveryAction.Invoke(message as TMessage);
      }
    }

    private class WeakTinyMessageSubscription<TMessage> : ITinyMessageSubscription
      where TMessage : class, ITinyMessage {
      private readonly WeakReference _deliveryAction;
      private readonly WeakReference _messageFilter;
      private readonly TinyMessageSubscriptionToken _subscriptionToken;

      /// <summary>
      ///   Initializes a new instance of the WeakTinyMessageSubscription class.
      /// </summary>
      /// <param name="subscriptionToken">Subscription Token</param>
      /// <param name="deliveryAction">Delivery action</param>
      /// <param name="messageFilter">Filter function</param>
      public WeakTinyMessageSubscription(
        TinyMessageSubscriptionToken subscriptionToken,
        Action<TMessage> deliveryAction,
        Func<TMessage, bool> messageFilter) {
        if (subscriptionToken == null)
          throw new ArgumentNullException("subscriptionToken");

        if (deliveryAction == null)
          throw new ArgumentNullException("deliveryAction");

        if (messageFilter == null)
          throw new ArgumentNullException("messageFilter");

        _subscriptionToken = subscriptionToken;
        _deliveryAction = new WeakReference(deliveryAction);
        _messageFilter = new WeakReference(messageFilter);
      }

      public TinyMessageSubscriptionToken SubscriptionToken {
        get {
          return _subscriptionToken;
        }
      }

      public bool ShouldAttemptDelivery(ITinyMessage message) {
        if (message == null)
          return false;

        if (!(typeof(TMessage).GetTypeInfo().IsAssignableFrom(message.GetType().GetTypeInfo())))
          return false;

        if (!_deliveryAction.IsAlive)
          return false;

        if (!_messageFilter.IsAlive)
          return false;

        return ((Func<TMessage, bool>)_messageFilter.Target).Invoke(message as TMessage);
      }

      public void Deliver(ITinyMessage message) {
        if (!(message is TMessage))
          throw new ArgumentException("Message is not the correct type");

        if (!_deliveryAction.IsAlive)
          return;

        ((Action<TMessage>)_deliveryAction.Target).Invoke(message as TMessage);
      }
    }

    #endregion

    #region Subscription dictionary

    private readonly List<SubscriptionItem> _subscriptions = new List<SubscriptionItem>();
    private readonly object _subscriptionsPadlock = new object();

    private class SubscriptionItem {
      public SubscriptionItem(ITinyMessageProxy proxy, ITinyMessageSubscription subscription) {
        Proxy = proxy;
        Subscription = subscription;
      }

      public ITinyMessageProxy Proxy { get; private set; }
      public ITinyMessageSubscription Subscription { get; private set; }
    }

    #endregion

    #region Public API

    /// <summary>
    ///   Gets the Messenger's default static instance
    /// </summary>
    public static ITinyMessengerHub Default {
      get {
        return DefaultInstance;
      }
    }

    /// <summary>
    ///   Subscribe to a message type with the given destination and delivery action.
    ///   All references are held with strong references
    ///   All messages of this type will be delivered.
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="deliveryAction">Action to invoke when message is delivered</param>
    /// <returns>TinyMessageSubscription used to unsubscribing</returns>
    public TinyMessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction)
      where TMessage : class, ITinyMessage {
      return AddSubscriptionInternal(deliveryAction, m => true, true, DefaultTinyMessageProxy.Instance);
    }

    /// <summary>
    ///   Subscribe to a message type with the given destination and delivery action.
    ///   Messages will be delivered via the specified proxy.
    ///   All references (apart from the proxy) are held with strong references
    ///   All messages of this type will be delivered.
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="deliveryAction">Action to invoke when message is delivered</param>
    /// <param name="proxy">Proxy to use when delivering the messages</param>
    /// <returns>TinyMessageSubscription used to unsubscribing</returns>
    public TinyMessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, ITinyMessageProxy proxy)
      where TMessage : class, ITinyMessage {
      return AddSubscriptionInternal(deliveryAction, m => true, true, proxy);
    }

    /// <summary>
    ///   Subscribe to a message type with the given destination and delivery action.
    ///   All messages of this type will be delivered.
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="deliveryAction">Action to invoke when message is delivered</param>
    /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
    /// <returns>TinyMessageSubscription used to unsubscribing</returns>
    public TinyMessageSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences)
      where TMessage : class, ITinyMessage {
      return AddSubscriptionInternal(deliveryAction, m => true, useStrongReferences, DefaultTinyMessageProxy.Instance);
    }

    /// <summary>
    ///   Subscribe to a message type with the given destination and delivery action.
    ///   Messages will be delivered via the specified proxy.
    ///   All messages of this type will be delivered.
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="deliveryAction">Action to invoke when message is delivered</param>
    /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
    /// <param name="proxy">Proxy to use when delivering the messages</param>
    /// <returns>TinyMessageSubscription used to unsubscribing</returns>
    public TinyMessageSubscriptionToken Subscribe<TMessage>(
      Action<TMessage> deliveryAction,
      bool useStrongReferences,
      ITinyMessageProxy proxy) where TMessage : class, ITinyMessage {
      return AddSubscriptionInternal(deliveryAction, m => true, useStrongReferences, proxy);
    }

    /// <summary>
    ///   Subscribe to a message type with the given destination and delivery action with the given filter.
    ///   All references are held with WeakReferences
    ///   Only messages that "pass" the filter will be delivered.
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="deliveryAction">Action to invoke when message is delivered</param>
    /// <param name="messageFilter">Message Filter</param>
    /// <returns>TinyMessageSubscription used to unsubscribing</returns>
    public TinyMessageSubscriptionToken Subscribe<TMessage>(
      Action<TMessage> deliveryAction,
      Func<TMessage, bool> messageFilter) where TMessage : class, ITinyMessage {
      return AddSubscriptionInternal(deliveryAction, messageFilter, true, DefaultTinyMessageProxy.Instance);
    }

    /// <summary>
    ///   Subscribe to a message type with the given destination and delivery action with the given filter.
    ///   Messages will be delivered via the specified proxy.
    ///   All references (apart from the proxy) are held with WeakReferences
    ///   Only messages that "pass" the filter will be delivered.
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="deliveryAction">Action to invoke when message is delivered</param>
    /// <param name="messageFilter">Message Filter</param>
    /// <param name="proxy">Proxy to use when delivering the messages</param>
    /// <returns>TinyMessageSubscription used to unsubscribing</returns>
    public TinyMessageSubscriptionToken Subscribe<TMessage>(
      Action<TMessage> deliveryAction,
      Func<TMessage, bool> messageFilter,
      ITinyMessageProxy proxy) where TMessage : class, ITinyMessage {
      return AddSubscriptionInternal(deliveryAction, messageFilter, true, proxy);
    }

    /// <summary>
    ///   Subscribe to a message type with the given destination and delivery action with the given filter.
    ///   All references are held with WeakReferences
    ///   Only messages that "pass" the filter will be delivered.
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="deliveryAction">Action to invoke when message is delivered</param>
    /// <param name="messageFilter">Message Filter</param>
    /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
    /// <returns>TinyMessageSubscription used to unsubscribing</returns>
    public TinyMessageSubscriptionToken Subscribe<TMessage>(
      Action<TMessage> deliveryAction,
      Func<TMessage, bool> messageFilter,
      bool useStrongReferences) where TMessage : class, ITinyMessage {
      return AddSubscriptionInternal(
        deliveryAction,
        messageFilter,
        useStrongReferences,
        DefaultTinyMessageProxy.Instance);
    }

    /// <summary>
    ///   Subscribe to a message type with the given destination and delivery action with the given filter.
    ///   Messages will be delivered via the specified proxy.
    ///   All references are held with WeakReferences
    ///   Only messages that "pass" the filter will be delivered.
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="deliveryAction">Action to invoke when message is delivered</param>
    /// <param name="messageFilter">Message Filter</param>
    /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
    /// <param name="proxy">Proxy to use when delivering the messages</param>
    /// <returns>TinyMessageSubscription used to unsubscribing</returns>
    public TinyMessageSubscriptionToken Subscribe<TMessage>(
      Action<TMessage> deliveryAction,
      Func<TMessage, bool> messageFilter,
      bool useStrongReferences,
      ITinyMessageProxy proxy)
      where TMessage : class, ITinyMessage {
      return AddSubscriptionInternal(deliveryAction, messageFilter, useStrongReferences, proxy);
    }

    /// <summary>
    ///   Unsubscribe from a particular message type.
    ///   Does not throw an exception if the subscription is not found.
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="subscriptionToken">Subscription token received from Subscribe</param>
    public void Unsubscribe<TMessage>(TinyMessageSubscriptionToken subscriptionToken)
      where TMessage : class, ITinyMessage {
      RemoveSubscriptionInternal<TMessage>(subscriptionToken);
    }

    /// <summary>
    ///   Publish a message to any subscribers
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="message">Message to deliver</param>
    public void Publish<TMessage>(TMessage message) where TMessage : class, ITinyMessage {
      PublishInternal(message);
    }

    /// <summary>
    ///   Publish a message to any subscribers asynchronously
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="message">Message to deliver</param>
    public void PublishAsync<TMessage>(TMessage message) where TMessage : class, ITinyMessage {
      PublishAsyncInternal(message, null);
    }

    /// <summary>
    ///   Publish a message to any subscribers asynchronously
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <param name="message">Message to deliver</param>
    /// <param name="callback">AsyncCallback called on completion</param>
    public void PublishAsync<TMessage>(TMessage message, AsyncCallback callback) where TMessage : class, ITinyMessage {
      PublishAsyncInternal(message, callback);
    }

    #endregion

    #region Internal Methods

    private TinyMessageSubscriptionToken AddSubscriptionInternal<TMessage>(
      Action<TMessage> deliveryAction,
      Func<TMessage, bool> messageFilter,
      bool strongReference,
      ITinyMessageProxy proxy)
      where TMessage : class, ITinyMessage {
      if (deliveryAction == null)
        throw new ArgumentNullException("deliveryAction");

      if (messageFilter == null)
        throw new ArgumentNullException("messageFilter");

      if (proxy == null)
        throw new ArgumentNullException("proxy");

      lock (_subscriptionsPadlock) {
        var subscriptionToken = new TinyMessageSubscriptionToken(this, typeof(TMessage));

        ITinyMessageSubscription subscription;
        if (strongReference)
          subscription = new StrongTinyMessageSubscription<TMessage>(subscriptionToken, deliveryAction, messageFilter);
        else
          subscription = new WeakTinyMessageSubscription<TMessage>(subscriptionToken, deliveryAction, messageFilter);

        _subscriptions.Add(new SubscriptionItem(proxy, subscription));

        return subscriptionToken;
      }
    }

// ReSharper disable once UnusedTypeParameter
    private void RemoveSubscriptionInternal<TMessage>(TinyMessageSubscriptionToken subscriptionToken)
      where TMessage : class, ITinyMessage {
      if (subscriptionToken == null)
        throw new ArgumentNullException("subscriptionToken");

      lock (_subscriptionsPadlock) {
        var currentlySubscribed = (from sub in _subscriptions
          where ReferenceEquals(sub.Subscription.SubscriptionToken, subscriptionToken)
          select sub).ToList();

        // currentlySubscribed.ForEach(sub => _Subscriptions.Remove(sub));
        foreach (var sub in currentlySubscribed) {
          _subscriptions.Remove(sub);
        }
      }
    }

    private void PublishInternal<TMessage>(TMessage message)
      where TMessage : class, ITinyMessage {
      if (message == null)
        throw new ArgumentNullException("message");

      List<SubscriptionItem> currentlySubscribed;
      lock (_subscriptionsPadlock) {
        currentlySubscribed = (from sub in _subscriptions
          where sub.Subscription.ShouldAttemptDelivery(message)
          select sub).ToList();
      }

      foreach (var sub in currentlySubscribed) {
        try {
          sub.Proxy.Deliver(message, sub.Subscription);
        } catch (Exception ex) {
          Debug.WriteLine(ex.Message);
          // Ignore any errors and carry on
          // TODO - add to a list of erroring subs and remove them?
        }
      }

      /*currentlySubscribed.ForEach(sub =>
            {
                try
                {
                    sub.Proxy.Deliver(message, sub.Subscription);
                }
                catch (Exception)
                {
                    // Ignore any errors and carry on
                    // TODO - add to a list of erroring subs and remove them?
                }
            });*/
    }

    private void PublishAsyncInternal<TMessage>(TMessage message, AsyncCallback callback)
      where TMessage : class, ITinyMessage {
      Action publishAction = () => PublishInternal(message);

      publishAction.BeginInvoke(callback, null);
    }

    #endregion
  }
}
