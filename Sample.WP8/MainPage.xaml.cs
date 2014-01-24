using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Sample.Core;
using TinyMessenger;

namespace Sample.WP8 {
  public partial class MainPage : PhoneApplicationPage {
    // Constructor
    private CoreClass _core;
    private int _count;
    private TinyMessageSubscriptionToken _token;

    public MainPage() {
      InitializeComponent();

      Loaded += (s, e) => {
        _core = new CoreClass();

        _token = TinyMessengerHub.Default.Subscribe<StringMessage>(
          stringMessage => {
            if (stringMessage.Message == string.Empty) {
              Dispatcher.BeginInvoke(RefreshPage);
              return;
            }
            Dispatcher.BeginInvoke(() => outputLabel.Text = stringMessage.Message);
            --_count;
            Dispatcher.BeginInvoke(() => durationLabel.Text = _count.ToString());
          });

        RefreshPage();
      };
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
      button.IsEnabled = false;
      _count = Convert.ToInt32(slider.Value);
      durationLabel.Text = _count.ToString();
      _core.GetNewNumbers(_count);
    }

    private void RefreshPage() {
      _count = 0;
      outputLabel.Text = string.Empty;
      durationLabel.Text = string.Empty;
      button.IsEnabled = true;
    }
  }
}
