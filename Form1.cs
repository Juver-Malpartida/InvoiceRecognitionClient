using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace InvoiceRecognitionClient;

public partial class Form1 : Form
{
    private HttpClient httpClient = new HttpClient();
    private static readonly string ApiBaseUrl;
    private Dictionary<string, InvoiceStatus> invoiceTasks = new();
    private System.Windows.Forms.Timer pollingTimer = new();

    static Form1()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        ApiBaseUrl = config["ApiBaseUrl"] ?? "http://localhost:8000/api/v1/invoices";
    }

    public Form1()
    {
        InitializeComponent();
        pollingTimer.Interval = 3000; // 3 segundos
        pollingTimer.Tick += PollingTimer_Tick;
    }

    private async void btnUploadInvoice_Click(object sender, EventArgs e)
    {
        if (openFileDialogPdf.ShowDialog() == DialogResult.OK)
        {
            var filePaths = openFileDialogPdf.FileNames;
            int numFiles = filePaths.Length;
            ShowNotification($"Lote de {numFiles} factura(s) enviado a procesar.", false);
            string userId = Environment.UserName;
            var result = await UploadInvoicesAsync(filePaths, userId);
            if (result != null && result.Count == numFiles)
            {
                for (int i = 0; i < numFiles; i++)
                {
                    string fileName = Path.GetFileName(filePaths[i]);
                    var item = lvInvoices.Items.Add(fileName);
                    item.SubItems.Add("Procesando...");
                    item.SubItems.Add("");
                    string taskId = result[i];
                    invoiceTasks[taskId] = new InvoiceStatus { FileName = fileName, Status = "Procesando...", ListViewItem = item };
                }
                pollingTimer.Start();
            }
            else
            {
                ShowNotification($"Error al procesar el lote de facturas.", true);
            }
        }
    }

    private async Task<List<string>?> UploadInvoicesAsync(string[] filePaths, string userId)
    {
        using var form = new MultipartFormDataContent();
        var streams = new List<Stream>();
        try
        {
            foreach (var filePath in filePaths)
            {
                var fileStream = File.OpenRead(filePath);
                streams.Add(fileStream);
                string ext = Path.GetExtension(filePath).ToLowerInvariant();
                string mimeType = ext switch
                {
                    ".pdf" => "application/pdf",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => "application/octet-stream"
                };
                form.Add(new StreamContent(fileStream) { Headers = { ContentType = new MediaTypeHeaderValue(mimeType) } }, "files", Path.GetFileName(filePath));
            }
            form.Add(new StringContent(userId), "user_identifier");
            var response = await httpClient.PostAsync($"{ApiBaseUrl}/extract", form);
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("task_ids", out var taskIdsProp) && taskIdsProp.ValueKind == JsonValueKind.Array)
                {
                    var ids = new List<string>();
                    foreach (var el in taskIdsProp.EnumerateArray())
                        ids.Add(el.GetString() ?? "");
                    return ids;
                }
            }
        }
        catch
        {
            // Ignorar, se maneja en el caller
        }
        finally
        {
            foreach (var s in streams)
                s.Dispose();
        }
        return null;
    }

    private async void PollingTimer_Tick(object? sender, EventArgs e)
    {
        foreach (var kvp in invoiceTasks.ToList())
        {
            var taskId = kvp.Key;
            var status = kvp.Value;
            var response = await httpClient.GetAsync($"{ApiBaseUrl}/status/{taskId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var state = doc.RootElement.GetProperty("status").GetString();
                status.ListViewItem.SubItems[1].Text = state switch
                {
                    "PENDING" or "PROCESSING" => "Procesando...",
                    "COMPLETED" => "Listo para Revisión",
                    "FAILED" => "Error",
                    _ => state
                };
                if (state == "COMPLETED")
                {
                    status.ListViewItem.SubItems[2].Text = ""; // Limpiar mensaje si estaba en error
                    ShowNotification($"Los datos de la factura {status.FileName} están listos para su revisión.", false);
                    status.JsonData = doc.RootElement.GetProperty("data").ToString();
                    pollingTimer.Stop();
                }
                else if (state == "FAILED")
                {
                    var errorMsg = doc.RootElement.TryGetProperty("error_message", out var err) ? err.GetString() : "Error desconocido";
                    status.ListViewItem.SubItems[2].Text = errorMsg;
                    ShowNotification($"Error al procesar la factura {status.FileName}: {errorMsg}", true);
                }
            }
        }
    }

    // Evento correctamente enlazado
    private void lvInvoices_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lvInvoices.SelectedItems.Count == 1)
        {
            var item = lvInvoices.SelectedItems[0];
            var taskId = invoiceTasks.FirstOrDefault(x => x.Value.ListViewItem == item).Key;
            if (taskId != null && invoiceTasks[taskId].JsonData != null)
            {
                ShowReviewPanel(invoiceTasks[taskId].JsonData, invoiceTasks[taskId].FileName);
            }
        }
    }

    private void ShowReviewPanel(string jsonData, string fileName)
    {
        // Limpiar panel y tabla
        tableReview.Controls.Clear();
        tableReview.RowCount = 0;
        tableReview.RowStyles.Clear();
        tableReview.ColumnStyles.Clear();
        tableReview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
        tableReview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
        // Mostrar datos extraídos en la tabla
        var data = JsonSerializer.Deserialize<InvoiceData>(jsonData);
        int row = 0;
        void AddRow(string label, string value)
        {
            tableReview.RowCount++;
            tableReview.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var lbl = new Label { Text = label, Anchor = AnchorStyles.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
            var txt = new TextBox { Text = value, Width = 350, Anchor = AnchorStyles.Left, ReadOnly = false };
            tableReview.Controls.Add(lbl, 0, row);
            tableReview.Controls.Add(txt, 1, row);
            row++;
        }
        if (data != null)
        {
            AddRow("ID Factura", data.invoice_id ?? "");
            AddRow("Emisor", data.issuer_name ?? "");
            AddRow("NIF Emisor", data.issuer_tax_id ?? "");
            AddRow("Receptor", data.recipient_name ?? "");
            AddRow("NIF Receptor", data.recipient_tax_id ?? "");
            AddRow("Fecha Emisión", data.issue_date ?? "");
            AddRow("Fecha Vencimiento", data.due_date ?? "");
            AddRow("Total", data.total_amount?.ToString() ?? "");
            AddRow("Impuesto", data.tax_amount?.ToString() ?? "");
            AddRow("Moneda", data.currency ?? "");
            // Mostrar ítems de línea
            if (data.line_items != null && data.line_items.Count > 0)
            {
                var lblItems = new Label { Text = "Ítems:", Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
                tableReview.Controls.Add(lblItems, 0, row);
                tableReview.SetColumnSpan(lblItems, 2);
                row++;
                foreach (var item in data.line_items)
                {
                    var desc = $"{item.description} | Cant: {item.quantity} | Unit: {item.unit_price} | Total: {item.total_price}";
                    var lblItem = new Label { Text = desc, Anchor = AnchorStyles.Left, AutoSize = true };
                    tableReview.Controls.Add(lblItem, 0, row);
                    tableReview.SetColumnSpan(lblItem, 2);
                    row++;
                }
            }
        }
        // Vista previa
        picturePreview.Visible = false;
        lblPdfPreview.Visible = false;
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        string filePath = null;
        foreach (ListViewItem item in lvInvoices.Items)
        {
            if (item.Text == fileName)
            {
                filePath = openFileDialogPdf.FileNames.FirstOrDefault(f => Path.GetFileName(f) == fileName);
                break;
            }
        }
        if (!string.IsNullOrEmpty(filePath))
        {
            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
            {
                try
                {
                    picturePreview.Image = Image.FromFile(filePath);
                    picturePreview.Visible = true;
                }
                catch { picturePreview.Visible = false; }
            }
            else if (ext == ".pdf")
            {
                lblPdfPreview.Visible = true;
            }
        }
        // Botones
        var prevBtns = pnlReview.Controls.OfType<Button>().ToList();
        foreach (var btn in prevBtns) pnlReview.Controls.Remove(btn);
        var btnConfirm = new Button { Text = "Confirmar y Guardar", Width = 150, Location = new Point(10, pnlReview.Height - 50) };
        var btnCancel = new Button { Text = "Cancelar", Width = 80, Location = new Point(170, pnlReview.Height - 50) };
        btnConfirm.Anchor = btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnConfirm.Click += (s, e) => { pnlReview.Visible = false; ShowNotification($"Factura {fileName} confirmada.", false); };
        btnCancel.Click += (s, e) => { pnlReview.Visible = false; };
        pnlReview.Controls.Add(btnConfirm);
        pnlReview.Controls.Add(btnCancel);
        pnlReview.Visible = true;
    }

    private void ShowNotification(string message, bool isError)
    {
        lblNotification.Text = message;
        lblNotification.ForeColor = isError ? Color.DarkRed : Color.DarkGreen;
        lblNotification.Visible = true;
        var t = new System.Windows.Forms.Timer { Interval = 4000 };
        t.Tick += (s, e) => { lblNotification.Visible = false; t.Stop(); };
        t.Start();
    }

    private class InvoiceStatus
    {
        public string FileName { get; set; } = "";
        public string Status { get; set; } = "";
        public ListViewItem ListViewItem { get; set; }
        public string? JsonData { get; set; }
    }

    private class InvoiceData
    {
        public string? invoice_id { get; set; }
        public string? issuer_name { get; set; }
        public string? issuer_tax_id { get; set; }
        public string? recipient_name { get; set; }
        public string? recipient_tax_id { get; set; }
        public string? issue_date { get; set; }
        public string? due_date { get; set; }
        public decimal? total_amount { get; set; }
        public decimal? tax_amount { get; set; }
        public string? currency { get; set; }
        public List<LineItem>? line_items { get; set; }
    }

    private class LineItem
    {
        public string? description { get; set; }
        public decimal? quantity { get; set; }
        public decimal? unit_price { get; set; }
        public decimal? total_price { get; set; }
    }
}
