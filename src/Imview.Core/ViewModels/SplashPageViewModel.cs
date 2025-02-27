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

using ReactiveUI;
using System.Windows.Input;
using Imview.Core.Controls.Templates;
using System.Collections.ObjectModel;

namespace Imview.Core.ViewModels;

public class SplashPageViewModel : ViewModelBase {

    public ObservableCollection<SplashSectionViewModel> Sections { get; }

    public SplashPageViewModel(MainWindowViewModel mainViewModel) 
        => Sections = [
            new SplashSectionViewModel(
                "Quests",
                "Create Quest",
                "Load Quest",
                "Get Quests From Packet Capture",
                mainViewModel.CreateNewQuest,
                mainViewModel.LoadQuest,
                mainViewModel.GetQuestsFromPacketCapture)
        ];
}

public class SplashSectionViewModel {

    public string Title { get; }
    public string CreateButtonText { get; }
    public string LoadButtonText { get; }
    public string GetQuestsFromPacketCaptureButtonText { get; }
    public ICommand CreateCommand { get; }
    public ICommand LoadCommand { get; }
    public ICommand GetQuestsFromPacketCaptureCommand { get; }

    public SplashSectionViewModel(
        string title, 
        string createButtonText, 
        string loadButtonText,
        string getQuestsFromPacketCaptureButtonText,
        System.Action createAction, 
        System.Action loadAction,
        System.Action getQuestsFromPacketCaptureAction) {
        Title = title;
        CreateButtonText = createButtonText;
        LoadButtonText = loadButtonText;
        GetQuestsFromPacketCaptureButtonText = getQuestsFromPacketCaptureButtonText;
        CreateCommand = ReactiveCommand.Create(createAction);
        LoadCommand = ReactiveCommand.Create(loadAction);
        GetQuestsFromPacketCaptureCommand = ReactiveCommand.Create(getQuestsFromPacketCaptureAction);
    }

}