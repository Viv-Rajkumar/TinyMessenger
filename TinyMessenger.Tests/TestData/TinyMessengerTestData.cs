using TinyMessenger.Interfaces;

namespace TinyMessenger.Tests.TestData {
  public class TestMessage : TinyMessageBase {
    public TestMessage(object sender)
      : base(sender) {}
  }

  public class DerivedMessage<TThings> : TestMessage {
    public DerivedMessage(object sender)
      : base(sender) {}

    public TThings Things { get; set; }
  }

  public interface ITestMessageInterface : ITinyMessage {}

  public class InterfaceDerivedMessage<TThings> : ITestMessageInterface {
    public InterfaceDerivedMessage(object sender) {
      Sender = sender;
    }

    public TThings Things { get; set; }
    public object Sender { get; private set; }
  }

  public class TestProxy : ITinyMessageProxy {
    public ITinyMessage Message { get; private set; }

    public void Deliver(ITinyMessage message, ITinyMessageSubscription subscription) {
      Message = message;
      subscription.Deliver(message);
    }
  }
}
