namespace InvoiceRecognitionClient;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private Button btnUploadInvoice;
    private ListView lvInvoices;
    private Panel pnlReview;
    private Label lblNotification;
    private OpenFileDialog openFileDialogPdf;
    private TableLayoutPanel tableReview;
    private PictureBox picturePreview;
    private Label lblPdfPreview;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        btnUploadInvoice = new Button();
        lvInvoices = new ListView();
        pnlReview = new Panel();
        tableReview = new TableLayoutPanel();
        picturePreview = new PictureBox();
        lblPdfPreview = new Label();
        lblNotification = new Label();
        openFileDialogPdf = new OpenFileDialog();
        pnlReview.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picturePreview).BeginInit();
        SuspendLayout();
        // 
        // btnUploadInvoice
        // 
        btnUploadInvoice.Location = new Point(20, 20);
        btnUploadInvoice.Name = "btnUploadInvoice";
        btnUploadInvoice.Size = new Size(180, 40);
        btnUploadInvoice.TabIndex = 0;
        btnUploadInvoice.Text = "Cargar Factura(s) con IA";
        btnUploadInvoice.Click += btnUploadInvoice_Click;
        // 
        // lvInvoices
        // 
        lvInvoices.Location = new Point(20, 80);
        lvInvoices.Size = new Size(500, 340);
        lvInvoices.View = View.Details;
        lvInvoices.FullRowSelect = true;
        lvInvoices.GridLines = true;
        lvInvoices.MultiSelect = false;
        if (lvInvoices.Columns.Count == 0)
        {
            lvInvoices.Columns.Add("Archivo", 200, HorizontalAlignment.Left);
            lvInvoices.Columns.Add("Estado", 120, HorizontalAlignment.Left);
            lvInvoices.Columns.Add("Mensaje", 350, HorizontalAlignment.Left);
        }
        lvInvoices.SelectedIndexChanged += lvInvoices_SelectedIndexChanged;
        // 
        // pnlReview
        // 
        pnlReview.BorderStyle = BorderStyle.FixedSingle;
        pnlReview.Controls.Add(tableReview);
        pnlReview.Controls.Add(picturePreview);
        pnlReview.Controls.Add(lblPdfPreview);
        pnlReview.Location = new Point(540, 80);
        pnlReview.Name = "pnlReview";
        pnlReview.Size = new Size(650, 470);
        pnlReview.TabIndex = 2;
        pnlReview.Visible = false;
        pnlReview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        // 
        // tableReview
        // 
        tableReview.AutoScroll = true;
        tableReview.AutoSize = false;
        tableReview.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        tableReview.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
        tableReview.ColumnCount = 2;
        tableReview.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        tableReview.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
        tableReview.Location = new Point(0, 0);
        tableReview.Name = "tableReview";
        tableReview.RowCount = 5;
        tableReview.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableReview.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableReview.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableReview.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableReview.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        tableReview.Size = new Size(650, 350);
        tableReview.TabIndex = 0;
        tableReview.Dock = DockStyle.Top;
        // 
        // picturePreview
        // 
        picturePreview.Location = new Point(10, 360);
        picturePreview.Name = "picturePreview";
        picturePreview.Size = new Size(300, 100);
        picturePreview.SizeMode = PictureBoxSizeMode.Zoom;
        picturePreview.TabIndex = 1;
        picturePreview.TabStop = false;
        picturePreview.Visible = false;
        // 
        // lblPdfPreview
        // 
        lblPdfPreview.Location = new Point(10, 360);
        lblPdfPreview.Name = "lblPdfPreview";
        lblPdfPreview.Size = new Size(300, 30);
        lblPdfPreview.TabIndex = 2;
        lblPdfPreview.Text = "Vista previa no disponible";
        lblPdfPreview.TextAlign = ContentAlignment.MiddleCenter;
        lblPdfPreview.Visible = false;
        // 
        // lblNotification
        // 
        lblNotification.Location = new Point(0, 0);
        lblNotification.Size = new Size(ClientSize.Width, 32);
        lblNotification.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        lblNotification.ForeColor = Color.DarkGreen;
        lblNotification.BackColor = Color.LightYellow;
        lblNotification.TextAlign = ContentAlignment.MiddleCenter;
        lblNotification.Visible = false;
        lblNotification.BringToFront();
        // 
        // openFileDialogPdf
        // 
        openFileDialogPdf.Filter = "Facturas (*.pdf;*.jpg;*.png)|*.pdf;*.jpg;*.png";
        openFileDialogPdf.Multiselect = true;
        openFileDialogPdf.Title = "Seleccionar factura(s)";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1216, 546);
        Controls.Add(btnUploadInvoice);
        Controls.Add(lvInvoices);
        Controls.Add(pnlReview);
        Controls.Add(lblNotification);
        Name = "Form1";
        Text = "Reconocimiento de Facturas con IA";
        Controls.Add(lblNotification);
        pnlReview.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picturePreview).EndInit();
        ResumeLayout(false);
    }

    #endregion
}
