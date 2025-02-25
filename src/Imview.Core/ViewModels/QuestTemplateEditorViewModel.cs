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

using System.Windows.Input;
using ReactiveUI;
using Imcodec.ObjectProperty.TypeCache;

namespace Imview.Core.ViewModels;

public class QuestTemplateEditorViewModel : ViewModelBase {

    public ICommand BackToSplashCommand { get; }
    public QuestTemplate Template { get; }

    private readonly MainWindowViewModel _mainViewModel;

    public QuestTemplateEditorViewModel(MainWindowViewModel mainViewModel, QuestTemplate? template = null) {
        _mainViewModel = mainViewModel;
        Template = template ?? new QuestTemplate();
        BackToSplashCommand = ReactiveCommand.Create(BackToSplash);
    }

    private void BackToSplash() {
        _mainViewModel.ReturnToSplash();
    }
    
}