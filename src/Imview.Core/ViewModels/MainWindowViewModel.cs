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
using System.Windows.Input;
using Imview.Core.ViewModels;
using ReactiveUI;
using Avalonia.Notification;
using Imview.Core.Services;
using Avalonia.Controls;

namespace Imview.Core.ViewModels;

public class MainWindowViewModel : ViewModelBase {

    public INotificationMessageManager Manager { get; } = new NotificationMessageManager();
    public ICommand CreateQuestCommand { get; }
    public ICommand LoadQuestCommand { get; }

    private ViewModelBase _currentViewModel;
    private Window? _mainWindow;

    public MainWindowViewModel() {
        _currentViewModel = new SplashPageViewModel(this);
        CreateQuestCommand = ReactiveCommand.Create(CreateNewQuest);
        LoadQuestCommand = ReactiveCommand.Create(LoadQuest);

        MessageService.Initialize(Manager);
    }

    public void Initialize(Window window) {
        _mainWindow = window;
    }

    public ViewModelBase CurrentViewModel {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public void CreateNewQuest() {
        CurrentViewModel = new QuestTemplateEditorViewModel(this);
    }

    public async void LoadQuest() {
        try {
            if (_mainWindow == null) {
                MessageService.Error("Main window is not initialized.")
                    .WithDuration(TimeSpan.FromSeconds(5))
                    .Send();
                    
                return;
            }

            var template = await TemplateSerializer.LoadTemplateAsync(_mainWindow);
            if (template != null) {
                CurrentViewModel = new QuestTemplateEditorViewModel(this, template);
                MessageService.Info("Quest template loaded successfully!")
                    .WithDuration(TimeSpan.FromSeconds(3))
                    .Send();
            }
        }
        catch (Exception ex) {
            MessageService.Error($"Failed to load quest template: {ex.Message}")
                .WithDuration(TimeSpan.FromSeconds(5))
                .Send();
        }
    }

    public void GetQuestsFromPacketCapture() {
        // Placeholder for future implementation
        MessageService.Info("This feature is not yet implemented.")
            .WithDuration(TimeSpan.FromSeconds(3))
            .Send();
    }

    public void ReturnToSplash() {
        CurrentViewModel = new SplashPageViewModel(this);
    }

}