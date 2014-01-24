using System;
using System.Threading.Tasks;
using TinyMessenger;

namespace Sample.Core {
  public class CoreClass {
    private static readonly Random Rnd = new Random();

    public void GetNewNumbers(int requiredNumbers) {
      Task.Run(
        async () => {
          var number = 0;
          for (var i = 0; i < requiredNumbers; ++i) {
            var temp = 0;
            do {
              temp = Rnd.Next(1, 5);
            } while (temp == number);
            number = temp;
            await Task.Delay(number * 1000);
            TinyMessengerHub.Default.Publish(new StringMessage(this, number.ToString()));
          }
          TinyMessengerHub.Default.Publish(new StringMessage(this, string.Empty));
        });
    }
  }
}
