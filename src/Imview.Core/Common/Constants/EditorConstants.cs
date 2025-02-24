/*
BSD 3-Clause License

Copyright (c) 2024, Jooty

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.
*/

namespace Imview.Core.Common.Constants;

public static class EditorConstants {

   public const int DEFAULT_WINDOW_WIDTH = 500;
   public const int DEFAULT_WINDOW_HEIGHT = 600;
   public const int DEFAULT_CONTROL_SPACING = 5;
   public const int DEFAULT_MARGIN = 20;
   public const int DEFAULT_LIST_HEIGHT = 150;
   public const int DEFAULT_SELECTOR_WIDTH = 100;

   public const int SMALL_WINDOW_WIDTH = 300;
   public const int SMALL_WINDOW_HEIGHT = 400;

   public const int MESSAGE_DELAY_DEFAULT_IN_SECONDS = 5;
   public const string MESSAGE_BACKGROUND_COLOR = "#333";

   public const string MESSAGE_INFO_ACCENT = "#1751C3";
   public const string MESSAGE_WARN_ACCENT = "#FFA500";
   public const string MESSAGE_ERROR_ACCENT = "#FF0000";
   public const string MESSAGE_FATAL_ACCENT = "#8B0000";

   public const string MESSAGE_INFO_BADGE = "Info";
   public const string MESSAGE_WARN_BADGE = "Warning";
   public const string MESSAGE_ERROR_BADGE = "Error";
   public const string MESSAGE_FATAL_BADGE = "Fatal";

   public static readonly Avalonia.Thickness DEFAULT_MARGIN_THICKNESS = new(DEFAULT_MARGIN);
   public static readonly Avalonia.Thickness DEFAULT_GROUP_PADDING = new(10);
   public static readonly Avalonia.Thickness DEFAULT_BUTTON_MARGIN = new(0, 10, 0, 0);
   public static readonly Avalonia.CornerRadius DEFAULT_CORNER_RADIUS = new(5);
   public static readonly Avalonia.Media.IBrush DEFAULT_WINDOW_BACKGROUND = 
      new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E1E1E"));

}