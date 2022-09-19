//https://www.advsofteng.com/doc/cdnetdoc/contour.htm

using System;
using ChartDirector;


namespace Leucippus.Models
{
    public class MatrixView
    {        

        public void Create(RazorChartViewer rzv,double[] dataX, double[] dataY, double[] dataZ, string PdbCode)
        {            
            // Create a XYChart object of size 600 x 500 pixels
            XYChart c = new XYChart(600, 500);

            // Add a title to the chart using 15 points Arial Bold Italic font
            c.addTitle(PdbCode, "Arial Bold Italic", 15);

            // Set the plotarea at (75, 40) and of size 400 x 400 pixels. Use semi-transparent black
            // (80000000) dotted lines for both horizontal and vertical grid lines
            c.setPlotArea(75, 40, 400, 400, -1, -1, -1, c.dashLineColor(unchecked((int)0x80000000),
                Chart.DotLine), -1);

            // Set x-axis and y-axis title using 12 points Arial Bold Italic font
            c.xAxis().setTitle("", "Arial Bold Italic", 8);
            c.yAxis().setTitle("", "Arial Bold Italic", 8);

            // Set x-axis and y-axis labels to use Arial Bold font
            c.xAxis().setLabelStyle("Arial Bold");
            c.yAxis().setLabelStyle("Arial Bold");

            // When auto-scaling, use tick spacing of 40 pixels as a guideline
            c.yAxis().setTickDensity(40);
            c.xAxis().setTickDensity(40);

            // Add a contour layer using the given data
            ContourLayer layer = c.addContourLayer(dataX, dataY, dataZ);

            // Move the grid lines in front of the contour layer
            c.getPlotArea().moveGridBefore(layer);

            // Add a color axis (the legend) in which the top left corner is anchored at (505, 40). Set the
            // length to 400 pixels and the labels on the right side.
            ColorAxis cAxis = layer.setColorAxis(505, 40, Chart.TopLeft, 400, Chart.Right);

            // Add a title to the color axis using 12 points Arial Bold Italic font
            cAxis.setTitle("", "Arial Bold Italic", 8);

            // Set color axis labels to use Arial Bold font
            cAxis.setLabelStyle("Arial Bold");

            // Output the chart            
            rzv.Image = c.makeWebImage(Chart.SVG);

            // Include tool tip for the chart
            rzv.ImageMap = c.getHTMLImageMap("", "","title=''");

            // Output Javascript chart model to support contour chart tooltips
            rzv.ChartModel = c.getJsChartModel();
        }

    }
}
