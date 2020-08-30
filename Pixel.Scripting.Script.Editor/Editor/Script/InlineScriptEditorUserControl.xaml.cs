﻿using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Component = Pixel.Automation.Core.Component;
using Pixel.Automation.Core;
using Pixel.Automation.Core.Components.TestCase;
using System.Collections.Generic;

namespace Pixel.Scripting.Script.Editor.Script
{
    /// <summary>
    /// Interaction logic for InlineScriptEditorUserControl.xaml
    /// </summary>
    public partial class InlineScriptEditorUserControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ScriptFileProperty = DependencyProperty.Register("ScriptFile", typeof(string), typeof(InlineScriptEditorUserControl),
                                                                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public string ScriptFile
        {
            get { return (string)GetValue(ScriptFileProperty); }
            set { SetValue(ScriptFileProperty, value); }
        }

        public static readonly DependencyProperty OwnerComponentProperty = DependencyProperty.Register("OwnerComponent", typeof(Component), typeof(InlineScriptEditorUserControl),
                                                                                          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));
        public Component OwnerComponent
        {
            get { return (Component)GetValue(OwnerComponentProperty); }
            set { SetValue(OwnerComponentProperty, value); }
        }
     
        public IInlineScriptEditor Editor { get; private set; }

        public InlineScriptEditorUserControl()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (OwnerComponent != null && !string.IsNullOrEmpty(ScriptFile) && this.Editor == null)
            {             

                var editorFactory = OwnerComponent.EntityManager.GetServiceOfType<IScriptEditorFactory>();
                this.Editor = editorFactory.CreateInlineScriptEditor();          
                if(OwnerComponent.TryGetAnsecstorOfType<TestCaseEntity>(out TestCaseEntity testCaseEntity))
                {
                    //Test cases have a initialization script file which contains all declared variables. In order to get intellisense support for those variable, we need a reference to that project
                    editorFactory.AddProject(OwnerComponent.Id, new string[] { testCaseEntity.Tag}, OwnerComponent.EntityManager.Arguments.GetType());                   
                }
                else
                {
                    editorFactory.AddProject(OwnerComponent.Id, Array.Empty<string>(), OwnerComponent.EntityManager.Arguments.GetType());                   
                }
                editorFactory.AddDocument(ScriptFile, OwnerComponent.Id, string.Empty);
                this.Editor.OpenDocument(ScriptFile, OwnerComponent.Id, string.Empty);
                OnPropertyChanged(nameof(this.Editor));
            }      
        }

        #region INotifyPropertyChanged

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected virtual void OnPropertyChanged([CallerMemberName]string name = "")
        {
            this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion INotifyPropertyChanged
    }
}
