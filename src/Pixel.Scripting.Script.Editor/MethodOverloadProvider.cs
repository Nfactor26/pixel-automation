using ICSharpCode.AvalonEdit.CodeCompletion;
using Pixel.Scripting.Editor.Core.Models.Signatures;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Pixel.Scripting.Script.Editor
{
    internal sealed class MethodOverloadProvider : IOverloadProvider, INotifyPropertyChanged
    {
        private readonly SignatureHelpResponse signatureHelp;     
        public MethodOverloadProvider(SignatureHelpResponse signatureHelp)
        {
            this.signatureHelp = signatureHelp;          
        }

        private int selectedIndex = 1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = value;
                Refresh(value);
                OnPropertyChanged();
            }
        }

        public int Count => signatureHelp.Signatures.Count();

        private string currentIndexText;
        public string CurrentIndexText
        {
            get => currentIndexText;
            set
            {
                currentIndexText = value;
                OnPropertyChanged();
            }
        }

        private object currentHeader;
        public object CurrentHeader
        {
            get => currentHeader;
            set
            {
                currentHeader = value;
                OnPropertyChanged();
            }
        }

        private object currentContent;
        public object CurrentContent
        {
            get => currentContent;
            set
            {
                currentContent = value;
                OnPropertyChanged();
            }
        }

        private void Refresh(int index)
        {
            var currentSignature = signatureHelp.Signatures.ElementAt(index);
            if(currentSignature.SymbolDisplayParts != null)
            {
                CurrentHeader = new WrapPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Children =
                    {
                        currentSignature.SymbolDisplayParts.ToTextBlock()
                    }
                };
            }
          
            var contentPanel = new StackPanel();
            contentPanel.Children.Add(new TextBlock() { Text = currentSignature.Documentation });
            foreach (var param in currentSignature.Parameters)
            {
                var wrapPanel = new WrapPanel() { Orientation = Orientation.Horizontal };
                wrapPanel.Children.Add(new TextBlock() { Text = $"{param.Label} : {param.Documentation}"});
                contentPanel.Children.Add(wrapPanel);
            }
            CurrentContent = contentPanel;


        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
