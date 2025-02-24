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

    public SplashPageViewModel() 
        => Sections = [
            new SplashSectionViewModel(
                "Quests",
                "Create Quest",
                ReactiveCommand.Create(CreateNewQuest))
        ];

    private void CreateNewQuest() {
        // todo
    }

}

public class SplashSectionViewModel {

    public string Title { get; }
    public string ButtonText { get; }
    public ICommand Command { get; }

    public SplashSectionViewModel(string title, string buttonText, ICommand command) {
        Title = title;
        ButtonText = buttonText;
        Command = command;
    }

}