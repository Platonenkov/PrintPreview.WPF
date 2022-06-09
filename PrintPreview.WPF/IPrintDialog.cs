using System;
using System.Collections.Generic;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;

namespace PrintPreview.WPF
{
    public static class IPrintDialog
    {
        public static IEnumerable<PrintQueue> GetPrinters()
        {
            //var printers = new List<PrintQueue>();
            //var printServer = new LocalPrintServer();

            //printers.AddRange(printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Connections }));
            //printers.AddRange(printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local }));
            var pr = new LocalPrintServer().GetPrintQueues(
                new[] { EnumeratedPrintQueueTypes.Connections, EnumeratedPrintQueueTypes.Local });
            return pr;
        }

        public static Dictionary<PageOrientation, string> GetPageOrientations() =>
            new()
            {
                { PageOrientation.Portrait, Properties.Resources.OrientationPortrait },
                { PageOrientation.Landscape, Properties.Resources.OrientationLandscape }
            };

        public static FlowDocument FlowDocumentClone(FlowDocument fd)
        {
            var str = XamlWriter.Save(fd);
            var stringReader = new StringReader(str);
            var xmlReader = XmlReader.Create(stringReader);
            return (FlowDocument)XamlReader.Load(xmlReader);
        }

        public static UIElement UIElementClone(UIElement? uie)
        {
            var str = XamlWriter.Save(uie!);
            var stringReader = new StringReader(str);
            var xmlReader = XmlReader.Create(stringReader);
            return (UIElement)XamlReader.Load(xmlReader);
        }

        public static bool PreviewDocument(FlowDocument? flowdocument, string description = "", bool singlecolumn = true, Window? owner = null, PageOrientation? StartOrientation = null)
        {
            if (flowdocument is null) { return false; }

            var w = new FlowDocumentPreview
            {
                fd = FlowDocumentClone(flowdocument),
                description = description,
                singlecolumn = singlecolumn,
                BaseOrientation = StartOrientation
            };

            if (owner is not null) { w.Owner = owner; w.Icon = owner.Icon; }

            var dialogResult = w.ShowDialog();
            return dialogResult switch
            {
                null => false,
                true => true,
                _ => false
            };
        }

        public static bool PrintDocument(FlowDocument? flowdocument, string description = "", bool singlecolumn = true)
        {
            if (flowdocument is null) { return false; }

            var fd = FlowDocumentClone(flowdocument);
            var pd = new PrintDialog();

            var area = pd.PrintQueue.GetPrintCapabilities().PageImageableArea;

            if (area is { })
            {
                switch (pd.PrintTicket.PageOrientation)
                {
                    case PageOrientation.Portrait:
                        fd.PageWidth = area.ExtentWidth + area.OriginWidth * 2;
                        fd.PageHeight = area.ExtentHeight + area.OriginHeight * 2;
                        break;
                    case PageOrientation.Landscape:
                        fd.PageWidth = area.ExtentHeight + area.OriginHeight * 2;
                        fd.PageHeight = area.ExtentWidth + area.OriginWidth * 2;
                        break;
                }
            }

            if (singlecolumn)
            {
                fd.ColumnGap = 0;
                fd.ColumnWidth = pd.PrintableAreaWidth;
            }

            var paginator = ((IDocumentPaginatorSource)fd).DocumentPaginator;
            pd.PrintDocument(paginator, description);
            return true;
        }

        public static bool PreviewUIElement(UIElement? uielement, string description = "", Window? owner = null, Func<UIElement?, UIElement?>? Clone = null, PageOrientation? StartOrientation = null)
        {
            if (uielement is null) { return false; }

            var w = new UIElementPreview
            {
                uie = Clone?.Invoke(uielement) ?? UIElementClone(uielement),
                description = description,
                Clone = Clone,
                BaseOrientation = StartOrientation
            };

            if (owner is not null) { w.Owner = owner; w.Icon = owner.Icon; }

            var dialog_result = w.ShowDialog();
            return dialog_result switch
            {
                null => false,
                true => true,
                _ => false
            };
        }

        public static bool PrintUIElement(UIElement? uielement, string description = "", Func<UIElement?, UIElement?>? Clone = null)
        {
            if (uielement is null) { return false; }

            var uie = Clone?.Invoke(uielement) ?? UIElementClone(uielement);
            var pd = new PrintDialog();
            var container = new Border();

            var area = pd.PrintQueue.GetPrintCapabilities().PageImageableArea;

            if (area is { })
            {
                switch (pd.PrintTicket.PageOrientation)
                {
                    case PageOrientation.Portrait:
                        container.Width = area.ExtentWidth + area.OriginWidth * 2;
                        container.Height = area.ExtentHeight + area.OriginHeight * 2;
                        break;
                    case PageOrientation.Landscape:
                        container.Width = area.ExtentHeight + area.OriginHeight * 2;
                        container.Height = area.ExtentWidth + area.OriginWidth * 2;
                        break;
                }
            }

            container.Child = uie;
            container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            container.Arrange(new Rect(container.DesiredSize));
            container.UpdateLayout();

            pd.PrintVisual(uie, description);
            return true;
        }

    }
}
