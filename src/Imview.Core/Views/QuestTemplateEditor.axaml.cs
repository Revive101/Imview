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

using Avalonia.Controls;
using Imview.Core.Controls.Templates;
using Imview.Core.ViewModels;

namespace Imview.Core.Views;

public partial class QuestTemplateEditorView : UserControl {
   
   public QuestTemplateEditorView() {
      InitializeComponent();

      // Wait for DataContext to be set
      DataContextChanged += (s, e) => {
         if (DataContext is QuestTemplateEditorViewModel vm) {
            var editor = this.FindControl<QuestTemplateEditor>("QuestEditor");
            if (editor != null) {
               editor.Template = vm.Template;
            }
         }
      };
   }

}