using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Configuration;

namespace optimus.delivery
{
    public class OrderRaw
    {
        public string printer { get; set; }
        public int cols { get; set; }

        public int blankLines = 3;

        private string blankLine { get { return "".PadLeft(blankLines, '\n') + " \n"; } }

        public StringBuilder raw = new StringBuilder();

        public char linePointer { get; set; }
        private string line { get { return "".PadLeft(cols, linePointer); } }

        private PrintDocument doc;

        public OrderRaw()
        {
        }

        private void printPageEventHandler(object sender, PrintPageEventArgs e)
        {
            Font font = new Font("monospace", 9);
            Graphics graphics = e.Graphics;
            graphics.DrawString(raw.ToString().ToUpper(), font, Brushes.Black, 0, 0, new StringFormat());
        }

        public void print(Order order)
        {
            doc = new PrintDocument();
            if (printer != null) doc.PrinterSettings.PrinterName = printer;
            doc.PrintPage += new PrintPageEventHandler(printPageEventHandler);

            setBanner();
            raw.AppendLine(line);
            setClient(order.client);
            setUser(order.user);
            raw.AppendLine(line);
            setItems(order.items);
            raw.AppendLine(line);
            setTotal(order);
            raw.AppendLine(line);
            setFooter();
            raw.Append(blankLine);
            Console.WriteLine(raw.ToString().ToUpper());
            raw.AppendLine(line);
            doc.Print();
            doc.Dispose();

        }

        private string splitInLine(string data)
        {
            string str = null;
            int pos;
            bool perfect = false;
            StringBuilder sb = new StringBuilder();
            double div = data.Length > cols ? Math.Ceiling((double)data.Length / cols) : 1.0;
            for (int i = 0; i < div; i++)
            {
                str = str != null ? str : data;
                int len = str.Length;
                pos = len <= cols ? len : str.Substring(0, cols).LastIndexOf(" ");
                perfect = len > cols ? str.Substring(cols).IndexOf(' ') == 0 : false;
                pos = perfect ? cols : pos;
                
                sb.AppendLine(str.Substring(0, pos));
                str = pos == len ? str : str.Substring(pos + 1);
                //Calabresa sem cebola e mussarela sem orégano.

            }
            return sb.ToString();
        }

        private void setBanner()
        {
            string banner = "Pizzaria e Esfirraria Lopes";
            double dBaber = (cols - banner.Length) / 2;
            int padBanner = cols - (int)Math.Round(dBaber, MidpointRounding.AwayFromZero);
            raw.AppendLine(banner.PadLeft(padBanner, ' '));
        }
        private void setFooter()
        {
            string banner = "Optimus delivery v.1.0.6";
            double dBaber = (cols - banner.Length) / 2;
            int padBanner = cols - (int)Math.Round(dBaber, MidpointRounding.AwayFromZero);
            raw.AppendLine(banner.PadLeft(padBanner, ' '));
        }


        private void setClient(Client client)
        {
            string col0 = String.Format("Cliente: {0}", client.name);
            string col1 = String.Format("Tel: {0}", client.phone);
            int pad = cols - col0.Length;
            if (pad - col1.Length == 0) raw.AppendFormat("{0} {1}\n", col0, col1);
            else if (pad - col1.Length > 0) raw.AppendFormat("{0}{1}\n", col0, col1.PadLeft(pad, ' '));
            else raw.AppendFormat("{0}\n{1}\n", col0, col1);
            string data = String.Format("{0} N.: {1} {2}", client.address, client.addr_number, client.addr_complement).Trim();
            raw.Append(splitInLine(data));
        }

        private void setUser(User user)
        {
            raw.AppendFormat("Vendedor: {0}\n", user.name);
        }

        private void setItems(List<OrderItem> items)
        {
            foreach (OrderItem item in items)
            {
                setItem(item);
            }
        }

        private void setItem(OrderItem item)
        {
            List<string> names = new List<string>();
            foreach (OrderItemPart part in item.parts)
            {
                names.Add(part.product.name);
            }
            string name = String.Join('/', names);
            string price = String.Format("{0:c}", item.price);
            int pad = cols - name.Length;
            int len = price.Length;

            if (pad - len == 0) raw.AppendFormat("{0} {1}\n", name, price);
            else if (pad - len > 0) raw.AppendFormat("{0}{1}\n", name, price.PadLeft(pad, ' '));
            else raw.AppendFormat("{0}\n{1}\n", name, price.PadLeft(cols, ' '));
        }

        private void setTotal(Order order)
        {
            raw.AppendLine("Observações:");
            raw.AppendLine(order.observation != null ? splitInLine(order.observation) : "");
            raw.AppendLine(line);
            string[] col0 = {
                "F. Pagamento:",
                "V. Total:",
                "V. Pago:",
                "Troco:"
            };

            string[] col1 = {
                order.payment.name,
                String.Format("{0:c}", order.price),
                String.Format("{0:c}", order.pay),
                String.Format("{0:c}", order.repay)
            };

            for (int i = 0; i < col0.Length; i++)
            {
                int pad = cols - col0[i].Length;
                raw.Append(col0[i]);
                raw.AppendLine(col1[i].PadLeft(pad, ' '));
            }
        }
    }
}