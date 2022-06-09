using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PrintPreview.WPF
{
    public partial class UIElementPreview : Window
    {
        public UIElement? uie;
        public string? description;

        private readonly PrintDialog pd = new();
        public Func<UIElement?, UIElement?>? Clone;
        public PageOrientation? BaseOrientation;

        public UIElementPreview()
        {
            InitializeComponent();
        }

        private void Preview_Loaded(object sender, RoutedEventArgs e)
        {
            stpPrint.DataContext = pd;

            GetPrinters();
            GetOrientations();
            if (BaseOrientation is not null)
                cmboOrientation.SelectedItem = IPrintDialog.GetPageOrientations().FirstOrDefault(f => f.Key == BaseOrientation.Value);
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

        private void GetOrientations() => cmboOrientation.ItemsSource = IPrintDialog.GetPageOrientations();


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
            var previewuie = Clone?.Invoke(uie) ?? IPrintDialog.UIElementClone(uie);
            var container = new Border();

            var area = pd.PrintQueue.GetPrintCapabilities().PageImageableArea;

            if (area is { })
            {
                if (pd.PrintTicket.PageOrientation == PageOrientation.Portrait)
                {
                    container.Width = area.ExtentWidth + area.OriginWidth * 2;
                    container.Height = area.ExtentHeight + area.OriginHeight * 2;
                    container.Padding = new Thickness(area.OriginWidth, area.OriginHeight, area.OriginWidth, area.OriginHeight);
                }
                else if (pd.PrintTicket.PageOrientation == PageOrientation.Landscape)
                {
                    container.Width = area.ExtentHeight + area.OriginHeight * 2;
                    container.Height = area.ExtentWidth + area.OriginWidth * 2;
                    container.Padding = new Thickness(area.OriginWidth, area.OriginHeight, area.OriginWidth, area.OriginHeight);
                }
            }

            container.Child = previewuie;
            container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            container.Arrange(new Rect(container.DesiredSize));
            container.UpdateLayout();
            container.Background = Brushes.White;
            container.Margin = new Thickness(20);

            vbPreview.Child = container;
        }

        private void PreparePrint()
        {
            var printuie = Clone?.Invoke(uie) ?? IPrintDialog.UIElementClone(uie);
            var container = new Border();

            var area = pd.PrintQueue.GetPrintCapabilities().PageImageableArea;

            if (area is { })
            {
                if (pd.PrintTicket.PageOrientation == PageOrientation.Portrait)
                {
                    container.Width = area.ExtentWidth + area.OriginWidth * 2;
                    container.Height = area.ExtentHeight + area.OriginHeight * 2;
                }
                else if (pd.PrintTicket.PageOrientation == PageOrientation.Landscape)
                {
                    container.Width = area.ExtentHeight + area.OriginHeight * 2;
                    container.Height = area.ExtentWidth + area.OriginWidth * 2;
                }
            }

            container.Child = printuie;
            container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            container.Arrange(new Rect(container.DesiredSize));
            container.UpdateLayout();

            pd.PrintVisual(printuie, description);
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
