using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

namespace optimus.delivery {
    public class PrinterTest {
        private PrintDocument document;
        private int cols;
        private string line;
        private char pointer = '.';
        

        public PrinterTest(int cols, string printer) {
            this.cols = cols;
            document = new PrintDocument();
            document.PrinterSettings.PrinterName = printer;
            line = "".PadLeft(cols+1, pointer);
        }

        public string getRaw() {
            return String.Format("{0}\n{1}", "".PadLeft(cols, pointer), pointer);
        }

        public void print() {
            document.Print();
        }

        private void printPageEventHandler(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("monospace", 9);
            Graphics graphics = e.Graphics;
            graphics.DrawString(line, font, Brushes.Black, 0, 0, new StringFormat());
            //graphics.DrawString(" ", font, Brushes.Black, 10, 0, new StringFormat());
        }
    }
}