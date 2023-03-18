using ICSharpCode.AvalonEdit.Highlighting;
using System.Windows.Controls;

namespace Pixel.Automation.HttpRequest.Editor
{
    /// <summary>
    /// Interaction logic for HttpResponseBodyView.xaml
    /// </summary>
    public partial class HttpResponseBodyView : UserControl
    {      
        public HttpResponseBodyView()
        {
            InitializeComponent();
            ResponseDataViewer.DocumentChanged += ResponseDataViewer_DocumentChanged;
        }

        private void ResponseDataViewer_DocumentChanged(object sender, EventArgs e)
        {
            string highlightSyntax = (this.DataContext as HttpResponseBodyViewModel).ResponseType;
            var typeConverter = new HighlightingDefinitionTypeConverter();
            if (highlightSyntax != null)
            {
                var syntaxHighlighter = (IHighlightingDefinition)typeConverter.ConvertFrom(highlightSyntax);
                ResponseDataViewer.SyntaxHighlighting = syntaxHighlighter;
                return;
            }
            ResponseDataViewer.SyntaxHighlighting = null;
        }
    }
}
