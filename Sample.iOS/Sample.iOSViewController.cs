using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Sample.Core;
using TinyMessenger;

namespace Sample.iOS
{
	public partial class Sample_iOSViewController : UIViewController
	{
		private CoreClass _core;
		private int _count;
		private TinyMessageSubscriptionToken _token;

		public Sample_iOSViewController (IntPtr handle) : base (handle)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		#region View lifecycle

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			OutputLabel.Text = string.Empty;
			ActivityIndicator.HidesWhenStopped = true;
			ActivityIndicator.StopAnimating ();

			_core = new CoreClass();

			_token = TinyMessengerHub.Default.Subscribe<StringMessage>(
				stringMessage => {
					if (stringMessage.Message == string.Empty) {
						InvokeOnMainThread(
							() => {
								OutputLabel.Text = string.Empty;
								FetchNumbersButton.Enabled = true;
								FetchNumbersButton.SetTitle("Fetch Numbers from PCL", UIControlState.Normal);
								ActivityIndicator.StartAnimating();
							});
						return;
					}
					InvokeOnMainThread(
						() => {
							OutputLabel.Text = string.Format("Number as {0} second delay", stringMessage.Message);
							FetchNumbersButton.SetTitle(string.Format("{0} Nums left!", _count--), UIControlState.Normal);
						});
				});

			FetchNumbersButton.TouchUpInside += (sender, args) => {
				_count = 10;
				FetchNumbersButton.SetTitle(string.Format("{0} Nums left!", _count--), UIControlState.Normal);
				FetchNumbersButton.Enabled = false;
				ActivityIndicator.StartAnimating();
				_core.GetNewNumbers(10);
			};


			
			// Perform any additional setup after loading the view, typically from a nib.
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
		}

		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
		}

		#endregion
	}
}

