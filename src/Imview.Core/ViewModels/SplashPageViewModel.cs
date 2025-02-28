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
                mainViewModel.GetQuestsFromPacketCapture),
            new SplashSectionViewModel(
                "Files",
                "Unpack KIWADs",
                "Unpack KIWADs & Deserialize",
                "",
                () => mainViewModel.UnpackKiwad(false),
                () => mainViewModel.UnpackKiwad(true),
                null)
        ];
}

public class SplashSectionViewModel(
    string title,
    string createButtonText,
    string loadButtonText,
    string getQuestsFromPacketCaptureButtonText,
    System.Action? createAction,
    System.Action? loadAction,
    System.Action? getQuestsFromPacketCaptureAction) {

    public string Title { get; } = title;
    public string CreateButtonText { get; } = createButtonText;
    public string LoadButtonText { get; } = loadButtonText;
    public string GetQuestsFromPacketCaptureButtonText { get; } = getQuestsFromPacketCaptureButtonText;
    public ICommand? CreateCommand { get; }
        = createAction != null
            ? ReactiveCommand.Create(createAction)
            : null;
    public ICommand? LoadCommand { get; }
        = loadAction != null
            ? ReactiveCommand.Create(loadAction)
            : null;
    public ICommand? GetQuestsFromPacketCaptureCommand { get; }
        = getQuestsFromPacketCaptureAction != null
            ? ReactiveCommand.Create(getQuestsFromPacketCaptureAction)
            : null;
    public bool HasThirdButton
        => !string.IsNullOrEmpty(GetQuestsFromPacketCaptureButtonText)
        && GetQuestsFromPacketCaptureCommand != null;

}