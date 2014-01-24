using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Sample.Core;
using TinyMessenger;

namespace Sample.Droid {
  [Activity(Label = "Sample.Droid", MainLauncher = true, Icon = "@drawable/icon")]
  public class Activity1 : Activity {
    private CoreClass _core;
    private int _count;
    private TinyMessageSubscriptionToken _token;

    protected override void OnCreate(Bundle bundle) {
      base.OnCreate(bundle);

      // Set our view from the "main" layout resource
      SetContentView(Resource.Layout.Main);

      // Get our button from the layout resource,
      // and attach an event to it
      var button = FindViewById<Button>(Resource.Id.MyButton);
      var button2 = FindViewById<Button>(Resource.Id.MySecondButton);
      var label = FindViewById<TextView>(Resource.Id.MyLabel);

      _token = TinyMessengerHub.Default.Subscribe<StringMessage>(
        stringMessage => {
          if (stringMessage.Message == string.Empty) {
            RunOnUiThread(
              () => {
                label.Text = string.Empty;
                button.Enabled = true;
                button.Text = Resources.GetString(Resource.String.ButtonText);
              });
            return;
          }
          RunOnUiThread(
            () => {
              label.Text = string.Format("Number {0} as {0} second delay", stringMessage.Message);
              button.Text = string.Format("{0} Nums left!", _count--);
            });
        });

      _core = new CoreClass();

      button.Click += (sender, args) => {
        _count = 10;
        button.Text = string.Format("{0} Nums left!", _count--);
        button.Enabled = false;
        _core.GetNewNumbers(10);
      };

      button2.Click += (sender, args) => Console.WriteLine("See I work fine!!!");
    }
  }
}
