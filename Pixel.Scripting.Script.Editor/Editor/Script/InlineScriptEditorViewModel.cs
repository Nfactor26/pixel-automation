﻿using Pixel.Scripting.Editor.Core;
using Pixel.Scripting.Editor.Core.Contracts;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pixel.Scripting.Script.Editor.Script
{
    public class InlineScriptEditorViewModel : IInlineScriptEditor, INotifyPropertyChanged
    {
        private string targetDocument;      
        private readonly IEditorService editorService;

        public CodeTextEditor Editor { get; private set; }

        public InlineScriptEditorViewModel(IEditorService editorService)
        {
            this.editorService = editorService;
            this.Editor = new CodeTextEditor(editorService)
            {
                ShowLineNumbers = false,
                Margin = new Thickness(2),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                WordWrap = true
            };
            this.Editor.LostFocus += OnLostFocus;
            this.Editor.GotFocus += OnFocus;
            this.editorService.WorkspaceChanged += OnWorkspaceChanged;
        }

        private void OnWorkspaceChanged(object sender, WorkspaceChangedEventArgs e)
        {
            this.Editor.LostFocus -= OnLostFocus;
            this.Editor.GotFocus -= OnFocus;
            (this.Editor as IDisposable)?.Dispose();

            this.Editor = new CodeTextEditor(editorService)
            {
                ShowLineNumbers = false,
                Margin = new Thickness(2),
                FontSize = 12,
                FontFamily = new FontFamily("Consolas"),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                WordWrap = true
            };
            OpenDocument(this.targetDocument);
            this.Editor.LostFocus += OnLostFocus;
            this.Editor.GotFocus += OnFocus;
            OnPropertyChanged(nameof(Editor));          
        }

        private void OnFocus(object sender, RoutedEventArgs e)
        {
            Activate();
            //this.Editor.InvalidateVisual();
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            Deactivate();
        }

        public virtual void OpenDocument(string targetDocument, string initialContent = "")
        {          
            this.targetDocument = targetDocument;
            this.editorService.CreateFileIfNotExists(targetDocument, initialContent);
            this.Editor.Text = this.editorService.GetFileContentFromDisk(targetDocument);
            this.Editor.OpenDocument(targetDocument);
            //Activate();
        }

        public void SetContent(string targetDocument, string documentContent)
        {
            this.editorService.SetContent(targetDocument, documentContent);
            this.Editor.Text = documentContent;
        }

        public virtual void CloseDocument(bool save = true)
        {
            if(this.targetDocument != null)
            {
                if (save)
                {
                    this.editorService.SaveDocument(this.targetDocument);
                }
                this.editorService.TryCloseDocument(this.targetDocument);
            }           
        }

        public virtual void Activate()
        {
            if (!this.editorService.HasDocument(this.targetDocument))
                this.editorService.AddDocument(this.targetDocument, this.Editor.Text);
            this.editorService.TryOpenDocument(this.targetDocument);
         
        }

        public virtual void Deactivate()
        {
            CloseDocument(true);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            CloseDocument(false);
            this.editorService.WorkspaceChanged -= OnWorkspaceChanged;
            (this.Editor as IDisposable)?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);           
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
