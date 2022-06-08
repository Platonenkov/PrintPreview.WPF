using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PrintPreview.WPF
{
    public partial class FlowDocumentPreview : Window
    {
        public FlowDocument? fd;
        public string? description;
        public bool singlecolumn;

        private readonly PrintDialog pd = new();

        public FlowDocumentPreview()
        {
            InitializeComponent();
        }

        private void Preview_Loaded(object sender, RoutedEventArgs e)
        {
            stpPrint.DataContext = pd;

            GetPrinters();
            GetOrientations();
        }

        public void GetPrinters()
        {
            var printers = IPrintDialog.GetPrinters();

            cmboPrinter.ItemsSource = printers;

            foreach (var printer in printers.Where(printer => pd.PrintQueue.FullName.Equals(printer.FullName)))
            {
                pd.PrintQueue = printer;
                cmboPrinter.SelectedItem = printer;
            }
        }

        private void GetOrientations() =>
            cmboOrientation.ItemsSource = IPrintDialog.GetPageOrientations();

        private void GetDuplexing()
        {
            var allowedduplex = pd.PrintQueue.GetPrintCapabilities().DuplexingCapability;
            var duplex = new Dictionary<Duplexing, string>();

            if (allowedduplex.Contains(Duplexing.OneSided)) { duplex.Add(Duplexing.OneSided, Properties.Resources.DuplexOneSided); }
            if (allowedduplex.Contains(Duplexing.TwoSidedLongEdge)) { duplex.Add(Duplexing.TwoSidedLongEdge, Properties.Resources.DuplexTwoSidedLongEdge); }
            if (allowedduplex.Contains(Duplexing.TwoSidedShortEdge)) { duplex.Add(Duplexing.TwoSidedShortEdge, Properties.Resources.DuplexTwoSidedShortEdge); }

            cmboDuplexing.ItemsSource = duplex;
            cmboDuplexing.IsEnabled = duplex.Count > 0;

            if (duplex.Count > 0)
            {
                pd.PrintTicket.Duplexing = duplex.Keys.First();
                cmboDuplexing.SelectedItem = duplex.Keys.First();
            }
        }

        private void PreparePreview()
        {
            var previewfd = IPrintDialog.FlowDocumentClone(fd!);
            var area = pd.PrintQueue.GetPrintCapabilities().PageImageableArea;

            if (area is { })
            {
                switch (pd.PrintTicket.PageOrientation)
                {
                    case PageOrientation.Portrait:
                        previewfd.PageWidth = area.ExtentWidth + area.OriginWidth * 2;
                        previewfd.PageHeight = area.ExtentHeight + area.OriginHeight * 2;
                        break;
                    case PageOrientation.Landscape:
                        previewfd.PageWidth = area.ExtentHeight + area.OriginHeight * 2;
                        previewfd.PageHeight = area.ExtentWidth + area.OriginWidth * 2;
                        break;
                }
            }

            if (singlecolumn)
            {
                previewfd.ColumnGap = 0;
                previewfd.ColumnWidth = pd.PrintableAreaWidth;
            }

            fd!.Background = Brushes.White;
            fd.Foreground = Brushes.Black;

            var previewpaginator = ((IDocumentPaginatorSource)previewfd).DocumentPaginator;
            previewpaginator.ComputePageCount();
            rnPageCount.Text = previewpaginator.PageCount.ToString();
            fdpvPreview.Document = previewfd;
        }

        private void PreparePrint()
        {
            var printfd = IPrintDialog.FlowDocumentClone(fd!);
            var area = pd.PrintQueue.GetPrintCapabilities().PageImageableArea;

            if (area is { })
            {
                switch (pd.PrintTicket.PageOrientation)
                {
                    case PageOrientation.Portrait:
                        printfd.PageWidth = area.ExtentWidth + area.OriginWidth * 2;
                        printfd.PageHeight = area.ExtentHeight + area.OriginHeight * 2;
                        break;
                    case PageOrientation.Landscape:
                        printfd.PageWidth = area.ExtentHeight + area.OriginHeight * 2;
                        printfd.PageHeight = area.ExtentWidth + area.OriginWidth * 2;
                        break;
                }
            }

            if (singlecolumn)
            {
                printfd.ColumnGap = 0;
                printfd.ColumnWidth = pd.PrintableAreaWidth;
            }

            var printpaginator = ((IDocumentPaginatorSource)printfd).DocumentPaginator;
            printpaginator = new PageRangeDocumentPaginator(printpaginator, rbPages.IsChecked == true ? tbPages.Text : "");
            pd.PrintDocument(printpaginator, description);
        }

        private void tbPages_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9-]+").IsMatch(e.Text);
        }

        private void cmboPrinter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetDuplexing();
            PreparePreview();
        }

        private void cmboOrientation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PreparePreview();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PreparePrint();

            DialogResult = true;
            Close();
        }
    }
}
