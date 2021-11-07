# Awrosoft.Utilities.HtmlToQuestPDF

[![Awrosoft](https://img.shields.io/badge/Developed%20by-Awrosoft-c11707?style=flat-square&logo=data:image/svg+xml;base64,...)](https://awrosoft.com)

**Awrosoft.Utilities.HtmlToQuestPDF** is a robust C# library, forked from [Relorer/HTMLToQPDF](https://github.com/Relorer/HTMLToQPDF), designed to seamlessly generate PDF documents from HTML content using the powerful [QuestPDF](https://www.questpdf.com/) library.

This fork extends the original functionality, enhancing support for common HTML elements and attributes to meet practical document generation needs.

Developed and maintained by [Awrosoft](https://awrosoft.com).

---

## ✨ Overview

QuestPDF excels at programmatic PDF generation but lacks native HTML rendering capabilities. `Awrosoft.Utilities.HtmlToQuestPDF` bridges this gap by offering a lightweight HTML parser that interprets a subset of commonly used HTML tags and inline CSS styles, transforming them into corresponding QuestPDF structures.

---

## 🔧 Key Enhancements

This enhanced version introduces support for:

- **Inline `<span>` Styles:**
  - `color`: Apply custom text color.
  - `font-weight`: Supports `bold` styling.
  - `font-style`: Supports `italic` styling.
  - `text-decoration`: Supports `underline` and `line-through` effects.

- **Table Enhancements:**
  - `border="1"`: Draws borders around table cells.
  - `cellpadding`: Adds internal padding for better cell spacing.

- **Image Attributes:**
  - `width` and `height`: Set image dimensions directly.

- **Table Structure Improvements:**
  - Basic support for `colspan` and `rowspan` attributes in `<td>` and `<th>` elements.

---

## ⚠️ Important Considerations

Please note that QuestPDF does **not** provide full HTML/CSS rendering.

This library serves as a **partial parser**, targeting a **limited subset** of HTML and inline styles suitable for common scenarios like:
- Formatted text (bold, italic, underlined)
- Simple tables with styling
- Inline images

Complex layouts, external stylesheets, JavaScript, and advanced HTML5/CSS3 features are **not supported**.

---

## 📦 Dependencies

- [QuestPDF](https://www.questpdf.com/)
- [HtmlAgilityPack](https://html-agility-pack.net/)

---

## 🚀 Getting Started

Here is a simple example demonstrating how to use the `Html()` extension method inside your QuestPDF document generation logic, including basic error handling:

```csharp
using Awrosoft.Utilities.HtmlToQuestPDF; // Import the namespace
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

// ... within your document generation logic
page.Content().Html(handler =>
{
    try
    {
        if (!string.IsNullOrEmpty(myHtml))
        {
            handler.SetHtml(myHtml);
        }
    }
    catch (Exception ex)
    {
        handler.SetHtml("<p><b>Error rendering content. Please contact support.</b></p>");
    }
});
```

---

## 🤝 Contributing

Contributions, issues, and feature requests are warmly welcome!

Feel free to open an issue or pull request. Check the [Issues Page](https://github.com/Awrosoft/Awrosoft.Utilities.HtmlToQuestPDF/issues) to see current topics.

---

## 📄 License

This project is distributed under the **MIT License**. See the [LICENSE](https://github.com/Awrosoft/Awrosoft.Utilities.HtmlToQuestPDF/blob/main/LICENSE) file for details.