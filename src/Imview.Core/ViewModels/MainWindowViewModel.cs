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

namespace Imview.Core.ViewModels;

public class MainWindowViewModel : ViewModelBase {

    public INotificationMessageManager Manager { get; } = new NotificationMessageManager();

    private ViewModelBase _currentViewModel;

    public MainWindowViewModel() {
        _currentViewModel = new SplashPageViewModel(this);
        CreateQuestCommand = ReactiveCommand.Create(CreateNewQuest);

        MessageService.Initialize(Manager);
    }

    public ViewModelBase CurrentViewModel {
        get => _currentViewModel;
        set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
    }

    public ICommand CreateQuestCommand { get; }

    public void CreateNewQuest() {
        CurrentViewModel = new QuestTemplateEditorViewModel(this);
    }

    public void ReturnToSplash() {
        CurrentViewModel = new SplashPageViewModel(this);
    }

}