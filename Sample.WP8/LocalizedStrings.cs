﻿using Sample.WP8.Resources;

namespace Sample.WP8 {
  /// <summary>
  ///   Provides access to string resources.
  /// </summary>
  public class LocalizedStrings {
    private static readonly AppResources _localizedResources = new AppResources();

    public AppResources LocalizedResources {
      get {
        return _localizedResources;
      }
    }
  }
}
