# PrintPreview.WPF
Adds PrintDialog wraper to your project (with preview window)

Repo contains two projects:
* **PrintPreview.WPF** - Class library.
* **PrintPreview.WPF.Test** - C# Sample Project, examine IPrint class library usage.

### Library usage:
#### C#

###### FlowDocument

```C#
using PrintPreview.WPF;

FlowDocument fd;

// Fill FlowDocument here 

  // prints FlowDocument to default printer with default settings
IPrintDialog.PrintDocument(fd);

// OR

  // show preview Window with printer/page settings
IPrintDialog.PreviewDocument(fd);
```

###### UIElement

```C#
using PrintPreview.WPF;

Grid uie;

// Fill Grid here 

  // prints UIElement to default printer with default settings
IPrintDialog.PrintUIElement(uie);

// OR

  // show preview Window with printer/page settings
IPrintDialog.PreviewUIElement(uie);
```

### Parameters:

* **flowdocument** - Document that will be printed
* **uielement** - UIElement that will be printed


