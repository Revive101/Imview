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

using System;
using Avalonia.Notification;
using System.Collections.Generic;
using Imview.Core.Common.Constants;

namespace Imview.Core.Services;

public class MessageBuilder {

    private readonly INotificationMessageManager _manager;
    private readonly string _message;
    private readonly string _accentColor;
    private readonly string _badge;
    private TimeSpan? _duration;
    private readonly List<(string buttonText, Action<object> buttonAction)> _buttons = [];

    internal MessageBuilder(
        INotificationMessageManager manager,
        string message,
        string accentColor,
        string badge) {
        _manager = manager;
        _message = message;
        _accentColor = accentColor;
        _badge = badge;
    }

    public MessageBuilder WithDuration(TimeSpan duration) {
        _duration = duration;

        return this;
    }

    public MessageBuilder WithButton(string buttonText, Action<object> buttonAction) {
        _buttons.Add((buttonText, buttonAction ?? (_ => { })));

        return this;
    }

    public void Send() {
        var messageBuilder = _manager
            .CreateMessage()
            .Accent(_accentColor)
            .Animates(true)
            .Background(EditorConstants.MESSAGE_BACKGROUND_COLOR)
            .HasBadge(_badge)
            .HasMessage(_message)
            .Dismiss().WithDelay(
                _duration 
                ?? TimeSpan.FromSeconds(EditorConstants.MESSAGE_DELAY_DEFAULT_IN_SECONDS));

        // Add buttons.
        foreach (var (buttonText, buttonAction) in _buttons) {
            messageBuilder.Dismiss().WithButton(buttonText, buttonAction);
        }

        messageBuilder.Queue();
    }

}

public static class MessageService {

    private static INotificationMessageManager? s_manager;

    /// <summary>
    /// Initialize the message service with a notification manager.
    /// </summary>
    public static void Initialize(INotificationMessageManager manager) 
        => s_manager = manager ?? throw new ArgumentNullException(nameof(manager));

    private static MessageBuilder CreateMessageBuilder(string message, string accentColor, string badge) {
        if (s_manager == null) {
            throw new InvalidOperationException("MessageService has not been initialized. Call Initialize() first.");
        }

        return new MessageBuilder(s_manager, message, accentColor, badge);
    }

    public static MessageBuilder Info(string message)   
        => CreateMessageBuilder(message, EditorConstants.MESSAGE_INFO_ACCENT, EditorConstants.MESSAGE_INFO_BADGE);

    public static MessageBuilder Warn(string message)   
        => CreateMessageBuilder(message, EditorConstants.MESSAGE_WARN_ACCENT, EditorConstants.MESSAGE_WARN_BADGE);

    public static MessageBuilder Error(string message)  
        => CreateMessageBuilder(message, EditorConstants.MESSAGE_ERROR_ACCENT, EditorConstants.MESSAGE_ERROR_BADGE);

    public static MessageBuilder Fatal(string message)  
        => CreateMessageBuilder(message, EditorConstants.MESSAGE_FATAL_ACCENT, EditorConstants.MESSAGE_FATAL_BADGE);

}