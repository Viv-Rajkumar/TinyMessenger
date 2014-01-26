// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace Sample.iOS
{
	[Register ("Sample_iOSViewController")]
	partial class Sample_iOSViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIActivityIndicatorView ActivityIndicator { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton FetchNumbersButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UILabel OutputLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (OutputLabel != null) {
				OutputLabel.Dispose ();
				OutputLabel = null;
			}

			if (ActivityIndicator != null) {
				ActivityIndicator.Dispose ();
				ActivityIndicator = null;
			}

			if (FetchNumbersButton != null) {
				FetchNumbersButton.Dispose ();
				FetchNumbersButton = null;
			}
		}
	}
}
