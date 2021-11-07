using System.Globalization;
using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

// Required for Float.TryParse invariant culture

namespace Awrosoft.Utilities.HtmlToQuestPDF.Components.Tags
{
    internal class TableComponent : BaseHTMLComponent
    {
        private delegate (uint, uint) GetPositionDelegate(int rowIndex, uint colSpan, uint rowSpan);

        public TableComponent(HtmlNode node, HTMLComponentsArgs args) : base(node, args)
        {
        }

        private HtmlNodeCollection GetCellNodes()
        {
            node.Id = String.IsNullOrEmpty(node.Id) ? Guid.NewGuid().ToString() : node.Id;
            // Select only direct children th/td within tbody/thead/tfoot or direct children of tr if no tbody/thead/tfoot exists
            // This tries to be slightly more robust against nested tables, though full nested table support is complex.
             return node.SelectNodes(".//tr/th | .//tr/td"); // Simplified selector targeting cells within rows of the current table.
            // The original XPath might be better depending on your specific needs for nested tables, but can be complex.
            // return node.SelectNodes($"(//table[@id=\"{node.Id}\"]//th | //table[@id=\"{node.Id}\"]//td)"); // Original problematic selector
        }

        private List<List<HtmlNode>> GetTableLines()
        {
            // This method relies on finding TRs. Ensure GetCellNodes returns cells in document order.
            var tableRows = node.SelectNodes(".//tr"); // Get all rows in the current table context
            if (tableRows == null) return new List<List<HtmlNode>>(); // No rows found

            var lines = new List<List<HtmlNode>>();

            foreach (var tr in tableRows)
            {
                // Get direct children th/td of the current row
                var cellsInRow = tr.SelectNodes("./th | ./td");
                if (cellsInRow != null)
                {
                    lines.Add(cellsInRow.ToList());
                }
                else
                {
                    lines.Add(new List<HtmlNode>()); // Add empty list for rows with no cells
                }
            }

            return lines;
        }


        protected override void ComposeMany(IContainer container)
        {
            // --- Read Table Attributes ---
            float border = 1; // Default border width if attribute is missing or invalid
            if (float.TryParse(node.GetAttributeValue("border", "1"), NumberStyles.Any, CultureInfo.InvariantCulture, out var borderAttrValue))
            {
                border = borderAttrValue;
            }
             // Clamp border to non-negative
            if (border < 0) border = 0;


            float padding = 5; // Default padding if attribute is missing or invalid
            if (float.TryParse(node.GetAttributeValue("cellpadding", "5"), NumberStyles.Any, CultureInfo.InvariantCulture, out var paddingAttrValue))
            {
                padding = paddingAttrValue;
            }
            // Clamp padding to non-negative
            if (padding < 0) padding = 0;

            // Cellspacing: No direct QuestPDF equivalent. border > 0 makes cells touch (like cellspacing=0).
            // If border=0, cellspacing is visually irrelevant. We parse it in case needed later.
            float cellSpacing = 0;
            if (float.TryParse(node.GetAttributeValue("cellspacing", "0"), NumberStyles.Any, CultureInfo.InvariantCulture, out var cellSpacingAttrValue))
            {
                 cellSpacing = cellSpacingAttrValue;
            }
             if (cellSpacing < 0) cellSpacing = 0;
             // NOTE: 'cellSpacing' value is read but not directly applied in this basic implementation.


            // Direction: Affects text rendering within cells.
            var direction = node.GetAttributeValue("dir", "ltr"); // Default to Left-to-Right
            var isRtl = direction.Trim().Equals("rtl", StringComparison.OrdinalIgnoreCase);

            // TODO: Propagate 'isRtl' to cell content rendering.
            // This might involve modifying 'args' or how 'GetComponent' resolves text components.
            // Example: args.IsRtl = isRtl; (if HTMLComponentsArgs has such a property)
            // Then, components handling text would check args.IsRtl and apply .DirectionRightToLeft()
            var currentArgs = args; // Clone or modify args if necessary to pass directionality
            // Example modification (requires changes to HTMLComponentsArgs):
            // var currentArgs = args.WithDirection(isRtl);


            // --- Build Table Structure ---
            container.Table(table =>
            {
                var lines = GetTableLines();
                if (!lines.Any()) return; // Handle empty tables

                // Calculate max columns considering colspans
                var maxColumns = 0;
                foreach (var line in lines)
                {
                    var currentLineCols = 0;
                    foreach (var cell in line)
                    {
                        currentLineCols += cell.GetAttributeValue("colspan", 1);
                    }
                    if (currentLineCols > maxColumns)
                    {
                        maxColumns = currentLineCols;
                    }
                }
                if (maxColumns == 0) return; // Handle table with no cells

                table.ColumnsDefinition(columns =>
                {
                    for (var i = 0; i < maxColumns; i++)
                    {
                        columns.RelativeColumn();
                    }
                });

                var getNextPosition = GetFuncGettingNextPosition(maxColumns);

                for (var rowIndex = 0; rowIndex < lines.Count; rowIndex++)
                {
                    var line = lines[rowIndex];
                    foreach (var cell in line)
                    {
                        var colSpan = (uint)cell.GetAttributeValue("colspan", 1);
                        var rowSpan = (uint)cell.GetAttributeValue("rowspan", 1);

                         // Ensure spans are at least 1
                        if (colSpan < 1) colSpan = 1;
                        if (rowSpan < 1) rowSpan = 1;


                        (var col, var row) = getNextPosition(rowIndex, colSpan, rowSpan);

                        // Check for potentially invalid positions (though GetFuncGettingNextPosition should handle this)
                         if (col > maxColumns) continue; // Skip cell if calculated column is out of bounds


                        table.Cell()
                        .ColumnSpan(colSpan)
                        .Column(col) // Position is 1-based
                        .Row((uint)rowIndex + 1) // Row index needs to be 1-based for QuestPDF
                        .RowSpan(rowSpan)
                        .Border(border)      // Apply parsed border attribute
                        .Padding(padding)    // Apply parsed cellpadding attribute
                        .Component(cell.GetComponent(currentArgs)); // Pass potentially modified args
                    }
                }
            });
        }

        // --- Helper Methods (Updated GetFuncGettingNextPosition and GetTableLines) ---

        // This helper function calculates the correct starting column and row index (1-based)
        // for a cell, considering previous cells and their col/row spans.
         private GetPositionDelegate GetFuncGettingNextPosition(int maxColumns)
        {
            // Tracks occupied cells: List index is row (0-based), bool[] index is column (0-based)
            var occupiedCells = new List<bool[]>();

            return (currentHtmlRowIndex, colSpan, rowSpan) =>
            {
                // Ensure occupiedCells list is large enough for the current row
                while (occupiedCells.Count <= currentHtmlRowIndex)
                {
                    occupiedCells.Add(new bool[maxColumns]);
                }

                // Find the first available column (0-based) in the current row
                var currentCol = 0;
                while (currentCol < maxColumns && occupiedCells[currentHtmlRowIndex][currentCol])
                {
                    currentCol++;
                }

                 // If we went past the max columns, something is wrong (maybe colspan exceeds maxColumns definitions)
                 // Clamp currentCol to avoid errors, although the root cause might be elsewhere
                if (currentCol >= maxColumns)
                {
                    currentCol = maxColumns - 1;
                     // Consider logging a warning here
                }


                // Mark the cells covered by this cell (colSpan x rowSpan) as occupied
                for (var r = 0; r < rowSpan; r++)
                {
                    var targetRow = currentHtmlRowIndex + r;
                     // Ensure occupiedCells list is large enough for spanned rows
                    while (occupiedCells.Count <= targetRow)
                    {
                        occupiedCells.Add(new bool[maxColumns]);
                    }

                    for (var c = 0; c < colSpan; c++)
                    {
                        var targetCol = currentCol + c;
                        if (targetCol < maxColumns) // Ensure we don't write out of bounds
                        {
                            occupiedCells[targetRow][targetCol] = true;
                        }
                    }
                }

                // Return the 1-based column and row for QuestPDF
                return ((uint)currentCol + 1, (uint)currentHtmlRowIndex + 1);
            };
        }


        // GetTr is no longer needed if GetTableLines directly processes <tr> elements.
        // private HtmlNode? GetTr(HtmlNode node) { ... }
    }
}