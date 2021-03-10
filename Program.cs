using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using optimus.delivery;
using System.IO;
using System.Drawing.Printing;
using System.Configuration;
using System.Collections.Generic;
using System.Net.Http.Formatting;

namespace delivery_printer
{
    class Program
    {
        const string configFilename = @"delivery.xml";
        static KeyValueConfigurationCollection settings;
        static int printerColumns;
        static string printerName;
        static string baseURL;
        static HttpClient client = new HttpClient();

        static bool confirm = false;

        static bool cancel = false;


        static async Task apiTask(string base_url)
        {
            try
            {
                HttpClient http = new HttpClient();
                http.BaseAddress = new Uri(base_url);
                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                Console.WriteLine("Enviando requisição para {0}me\nAguardando resposta...", http.BaseAddress);
                HttpResponseMessage response = await http.GetAsync("me");
                if (response.IsSuccessStatusCode)
                {
                    string data = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Sucesso a api respondeu com: {0}", response.StatusCode);
                    confirm = true;
                }
                else Console.WriteLine("Erro a api respondeu com: {0}", response.StatusCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao enviar a resquisição: {0}", ex.Message);
            }
        }

        static void configApi()
        {
            string base_url = "", default_base_url = "http://192.168.0.4:8000/api/";
            while (!confirm)
            {
                Console.WriteLine("Por favor, indique o endereço da api [{0}]", default_base_url);
                base_url = Console.ReadLine();
                if (base_url.Length == 0) base_url = default_base_url;
                apiTask(base_url).GetAwaiter().GetResult();
            }
            Console.WriteLine("Endereço da api {0}", base_url);
            settings.Add("api", base_url);
        }

        static void configPrinter()
        {
            var printers = PrinterSettings.InstalledPrinters;
            string printer = "", default_printer = new PrintDocument().PrinterSettings.PrinterName;
            int index = 0, default_index = 0;
            for (int i = 0; i < printers.Count; i++)
            {
                if (printers[i].Equals(default_printer)) default_index = i;
                Console.WriteLine("[{0}] {1}", i, printers[i]);
            }
            confirm = false;
            while (!confirm)
            {
                Console.WriteLine("Selecione sua impressora. [{0}]", default_index);
                printer = Console.ReadLine();
                index = printer.Length == 0 ? default_index : Convert.ToInt32(printer);
                if (index < printers.Count)
                {
                    Console.WriteLine("A impressora selecionada {0} está correta? [y/n]", printers[index]);
                    var key = Console.ReadKey();
                    if (key.KeyChar == 'y' || key.KeyChar == 'Y') confirm = true;
                    Console.Write("\n");
                }
            }
            settings.Add("printer", printers[index]);

            confirm = false;

            int default_cols = 32, cols = 0;
            while (!confirm)
            {
                Console.WriteLine("Qual o número de colunas da sua impressora? [{0}]", default_cols);
                string str = Console.ReadLine();
                cols = str.Length == 0 ? default_cols : Convert.ToInt32(str);
                if (cols > 0)
                {
                    PrinterTest test = new PrinterTest(cols, printers[index]);
                    Console.WriteLine("Enviamos um teste para sua impressora, o resultado é igual a esse?\n{0}", test.getRaw());
                    test.print();
                    var key = Console.ReadKey();
                    if (key.KeyChar == 'y' || key.KeyChar == 'Y') confirm = true;
                    Console.Write("\n");
                }
            }
            settings.Add("columns", cols.ToString());

        }

        static void consoleCancelHandle(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Aguarde o término das requisições.");
            cancel = true;
        }
        static void Main(string[] args)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(consoleCancelHandle);
            Console.WriteLine(config.FilePath);
            settings = config.AppSettings.Settings;

            if (!File.Exists(config.FilePath))
            {
                configApi();
                configPrinter();

                config.Save();
            }
            printerColumns = Convert.ToInt32(settings["columns"].Value);
            printerName = settings["printer"].Value;
            baseURL = settings["api"].Value;

            Console.WriteLine("Configurações em {0}", config.FilePath);
            Console.WriteLine("API: {0}", baseURL);
            Console.WriteLine("Impressora: {0}", printerName);
            Console.WriteLine("Número de Colunas: {0}", printerColumns);

            intervalTask().GetAwaiter().GetResult();

            Console.WriteLine("Precione enter para encerrar");
            Console.ReadLine();

            //RunAsync().GetAwaiter().GetResult();
        }

        static async Task intervalTask()
        {
            while (!cancel)
            {
                try
                {
                    await findOrder();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EROR: {0}", ex.Message);
                }
                if (cancel) break;
                await Task.Delay(1000 * 10);
            }
        }

        static async Task printOrder(Printer printer)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(String.Format("orders/{0}", printer.id_order));
            if (response.IsSuccessStatusCode)
            {
                Order order = await response.Content.ReadAsAsync<Order>();
                OrderRaw raw = new OrderRaw() { linePointer = '.', cols = printerColumns, printer = printerName };
                raw.print(order);
                await setPrintable(printer);
            }
        }

        static async Task setPrintable(Printer printer)
        {
            printer.printable = false;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.PutAsync<Printer>(String.Format("printers/{0}", printer.id), printer, new JsonMediaTypeFormatter());
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Printer finish {0}", response.StatusCode);
            }
            else
            {
                Console.WriteLine("ERROR: {0}", response.StatusCode);
            }
        }

        static async Task findOrder()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync("printers");
            if (response.IsSuccessStatusCode)
            {
                List<Printer> printers = await response.Content.ReadAsAsync<List<Printer>>();
                foreach (Printer printer in printers)
                {
                    if (printer.printable) await printOrder(printer);
                }
            }

        }
    }
}
