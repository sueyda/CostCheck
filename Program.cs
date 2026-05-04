using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace costcheckV1
{
    public class Islem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Aciklama { get; set; }
        public decimal Miktar { get; set; }
        public bool GelirMi { get; set; } 
        public DateTime Tarih { get; set; } = DateTime.Now;
        public string Kategori { get; set; } = "other";
    }

    public class IslemGorunum
    {
        public string Id { get; set; }
        public string Tarih { get; set; }
        public string Kategori { get; set; }
        public string Tur { get; set; }
        public string Aciklama { get; set; }
        public decimal Miktar { get; set; }
    }

    public class KategoriItem
    {
        public string Kod { get; set; }
        public string Label { get; set; }
    }

    public static class Kategoriler
    {
        public static readonly string[] Kodlar = { "salary", "groceries", "bills", "entertainment", "pets", "education", "health", "transport", "other" };

        public static string GetLabel(string kod, bool en)
        {
            switch (kod)
            {
                case "salary": return en ? "Salary / Income" : "Maaş / Gelir";
                case "groceries": return en ? "Groceries / Market" : "Mutfak / Market";
                case "bills": return en ? "Bills / Rent" : "Faturalar / Kira";
                case "entertainment": return en ? "Entertainment" : "Oyun & Eğlence";
                case "pets": return en ? "Pets" : "Kedi & Evcil Hayvan";
                case "education": return en ? "Education" : "Eğitim";
                case "health": return en ? "Health" : "Sağlık";
                case "transport": return en ? "Transport" : "Ulaşım";
                case "other": return en ? "Other" : "Diğer";
                default: return kod;
            }
        }
    }

    public class Ayarlar
    {
        public string Dil { get; set; } = "tr";
        public string ParaBirimi { get; set; } = "TL";
    }

    // YENİ: DÜZENLEME FORMU
    public class IslemDuzenleForm : Form
    {
        public Islem IslemVerisi { get; private set; }
        
        private ComboBox cmbKategori;
        private TextBox txtAciklama;
        private TextBox txtMiktar;
        private CheckBox chkGelirMi;
        private Button btnKaydet;
        private Button btnIptal;

        public IslemDuzenleForm(Islem islem, Ayarlar ayarlar)
        {
            this.IslemVerisi = new Islem {
                Id = islem.Id,
                Aciklama = islem.Aciklama,
                Miktar = islem.Miktar,
                GelirMi = islem.GelirMi,
                Tarih = islem.Tarih,
                Kategori = islem.Kategori
            };

            bool en = ayarlar.Dil == "en";
            string pb = ayarlar.ParaBirimi ?? "TL";

            this.Text = en ? "Edit Transaction" : "İşlemi Düzenle";
            this.Size = new Size(400, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10F);

            Label lblKategori = new Label { Text = en ? "Category:" : "Kategori:", Location = new Point(25, 25), AutoSize = true };
            cmbKategori = new ComboBox { Location = new Point(130, 22), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };
            
            var katList = Kategoriler.Kodlar.Select(k => new KategoriItem { Kod = k, Label = Kategoriler.GetLabel(k, en) }).ToList();
            cmbKategori.DataSource = katList;
            cmbKategori.DisplayMember = "Label";
            cmbKategori.ValueMember = "Kod";

            foreach (KategoriItem item in cmbKategori.Items)
            {
                if (item.Kod == IslemVerisi.Kategori) { cmbKategori.SelectedItem = item; break; }
            }

            Label lblAciklama = new Label { Text = en ? "Description:" : "Açıklama:", Location = new Point(25, 75), AutoSize = true };
            txtAciklama = new TextBox { Location = new Point(130, 72), Width = 220 };
            txtAciklama.Text = IslemVerisi.Aciklama;

            Label lblMiktar = new Label { Text = en ? $"Amount ({pb}):" : $"Miktar ({pb}):", Location = new Point(25, 125), AutoSize = true };
            txtMiktar = new TextBox { Location = new Point(130, 122), Width = 220 };
            txtMiktar.Text = IslemVerisi.Miktar.ToString("0.##");

            Label lblTur = new Label { Text = en ? "Type:" : "Tür:", Location = new Point(25, 175), AutoSize = true };
            chkGelirMi = new CheckBox { Text = en ? "This is an Income" : "Bu bir Gelir", Location = new Point(130, 172), Width = 220 };
            chkGelirMi.Checked = IslemVerisi.GelirMi;

            btnKaydet = new Button { Text = en ? "Save" : "Kaydet", Location = new Point(130, 220), Width = 105, Height = 35, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.Click += BtnKaydet_Click;

            btnIptal = new Button { Text = en ? "Cancel" : "İptal", Location = new Point(245, 220), Width = 105, Height = 35, BackColor = Color.FromArgb(149, 165, 166), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnIptal.FlatAppearance.BorderSize = 0;
            btnIptal.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblKategori, cmbKategori, lblAciklama, txtAciklama, lblMiktar, txtMiktar, lblTur, chkGelirMi, btnKaydet, btnIptal });
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAciklama.Text) || !decimal.TryParse(txtMiktar.Text, out decimal miktar) || miktar <= 0)
            {
                MessageBox.Show(this.Text.Contains("Edit") ? "Invalid input! Please check the amount." : "Geçersiz giriş! Lütfen miktarı kontrol edin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            IslemVerisi.Aciklama = txtAciklama.Text.Trim();
            IslemVerisi.Miktar = miktar;
            IslemVerisi.GelirMi = chkGelirMi.Checked;
            if (cmbKategori.SelectedItem is KategoriItem item) IslemVerisi.Kategori = item.Kod;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    public class AnaPencere : Form
    {
        private List<Islem> _islemler;
        private string _guncelDosyaYolu;
        private bool _kaydedilmemisDegisiklikVar;
        private Ayarlar _ayarlar;
        private readonly string _ayarlarDosyasi;
        private bool _filtreAktif = false;

        // UI Elemanları
        private TabControl tabControl;
        private TabPage tabIslemler;
        private TabPage tabAnaliz;

        private Panel pnlTop;
        private Panel pnlBottom;
        private Panel pnlFiltre;
        private Panel pnlOrta;
        private Panel pnlGrafik;

        private ComboBox cmbKategori;
        private TextBox txtAciklama;
        private TextBox txtMiktar;
        private DataGridView dgvGecmis;
        private Label lblGelir;
        private Label lblGider;
        private Label lblKalan;
        private Label lblMesaj;

        private DateTimePicker dtpBaslangic;
        private DateTimePicker dtpBitis;
        private Button btnFiltrele;
        private Button btnTumunuGoster;
        private Button btnCsvAktar;

        // Menü Elemanları
        private ToolStripMenuItem menuDosya;
        private ToolStripMenuItem menuYeni;
        private ToolStripMenuItem menuAc;
        private ToolStripMenuItem menuKaydet;
        private ToolStripMenuItem menuFarkliKaydet;
        private ToolStripMenuItem menuCikis;
        private ToolStripMenuItem menuAyarlar;
        
        private ToolStripMenuItem menuDil;
        private ToolStripMenuItem menuTr;
        private ToolStripMenuItem menuEn;
        
        private ToolStripMenuItem menuParaBirimi;
        private ToolStripMenuItem menuTl;
        private ToolStripMenuItem menuUsd;
        private ToolStripMenuItem menuEur;

        private Label lblKategori;
        private Label lblBaslik;
        private Label lblMiktar;
        private Button btnGelir;
        private Button btnGider;
        
        private Button btnSil;
        private Button btnDuzenle; // YENİ
        
        private Label lblListeBaslik;
        private ToolStripMenuItem deleteItem;
        private ToolStripMenuItem editItem; // YENİ

        public AnaPencere()
        {
            _islemler = new List<Islem>();
            _guncelDosyaYolu = null;
            _kaydedilmemisDegisiklikVar = false;

            string saveFolder = Path.Combine(Application.StartupPath, "Saves");
            if (!Directory.Exists(saveFolder)) 
            {
                Directory.CreateDirectory(saveFolder);
            }

            _ayarlarDosyasi = Path.Combine(saveFolder, "ayarlar.json");
            try
            {
                if (File.Exists(_ayarlarDosyasi))
                    _ayarlar = JsonSerializer.Deserialize<Ayarlar>(File.ReadAllText(_ayarlarDosyasi)) ?? new Ayarlar();
                else
                    _ayarlar = new Ayarlar();
            }
            catch { _ayarlar = new Ayarlar(); }

            if (string.IsNullOrEmpty(_ayarlar.ParaBirimi)) 
                _ayarlar.ParaBirimi = "TL";

            ArayuzuOlustur();
            DiliGuncelle();
            SetCustomIcon();
        }

        private void SetCustomIcon()
        {
            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_kaydedilmemisDegisiklikVar)
            {
                bool en = _ayarlar.Dil == "en";
                var msg = en ? "You have unsaved changes. Do you want to save before exiting?" : "Kaydedilmemiş değişiklikleriniz var. Çıkmadan önce kaydetmek ister misiniz?";
                var title = en ? "Warning" : "Uyarı";

                var cevap = MessageBox.Show(msg, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (cevap == DialogResult.Yes)
                {
                    DosyaKaydet(false);
                    if (_kaydedilmemisDegisiklikVar) e.Cancel = true; 
                }
                else if (cevap == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
            base.OnFormClosing(e);
        }

        private void ArayuzuOlustur()
        {
            this.Size = new Size(950, 800);
            this.MinimumSize = new Size(850, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 246, 250);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.FormBorderStyle = FormBorderStyle.Sizable; 
            this.MaximizeBox = true;

            // --- MENÜ ---
            MenuStrip menuStrip = new MenuStrip { Dock = DockStyle.Top, BackColor = Color.White, Font = new Font("Segoe UI", 9.5F) };
            
            menuDosya = new ToolStripMenuItem("Dosya");
            menuYeni = new ToolStripMenuItem("Yeni", null, (s, e) => YeniDosya()) { ShortcutKeys = Keys.Control | Keys.N };
            menuAc = new ToolStripMenuItem("Aç...", null, (s, e) => DosyaAc()) { ShortcutKeys = Keys.Control | Keys.O };
            menuKaydet = new ToolStripMenuItem("Kaydet", null, (s, e) => DosyaKaydet(false)) { ShortcutKeys = Keys.Control | Keys.S };
            menuFarkliKaydet = new ToolStripMenuItem("Farklı Kaydet...", null, (s, e) => DosyaKaydet(true)) { ShortcutKeys = Keys.Control | Keys.Shift | Keys.S };
            menuCikis = new ToolStripMenuItem("Çıkış", null, (s, e) => this.Close()) { ShortcutKeys = Keys.Alt | Keys.F4 };
            menuDosya.DropDownItems.AddRange(new ToolStripItem[] { menuYeni, menuAc, new ToolStripSeparator(), menuKaydet, menuFarkliKaydet, new ToolStripSeparator(), menuCikis });
            
            menuAyarlar = new ToolStripMenuItem("Ayarlar");
            menuDil = new ToolStripMenuItem("Dil (Language)");
            menuTr = new ToolStripMenuItem("Türkçe", null, (s, e) => DiliDegistir("tr")) { CheckOnClick = true };
            menuEn = new ToolStripMenuItem("English", null, (s, e) => DiliDegistir("en")) { CheckOnClick = true };
            menuDil.DropDownItems.AddRange(new ToolStripItem[] { menuTr, menuEn });
            
            menuParaBirimi = new ToolStripMenuItem("Para Birimi (Currency)");
            menuTl = new ToolStripMenuItem("TL (₺)", null, (s, e) => ParaBirimiDegistir("TL")) { CheckOnClick = true };
            menuUsd = new ToolStripMenuItem("USD ($)", null, (s, e) => ParaBirimiDegistir("$")) { CheckOnClick = true };
            menuEur = new ToolStripMenuItem("EUR (€)", null, (s, e) => ParaBirimiDegistir("€")) { CheckOnClick = true };
            menuParaBirimi.DropDownItems.AddRange(new ToolStripItem[] { menuTl, menuUsd, menuEur });

            menuAyarlar.DropDownItems.AddRange(new ToolStripItem[] { menuDil, menuParaBirimi });
            menuStrip.Items.AddRange(new ToolStripItem[] { menuDosya, menuAyarlar });
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;

            // --- TAB CONTROL ---
            tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10.5F) };
            this.Controls.Add(tabControl);
            tabControl.BringToFront();

            tabIslemler = new TabPage("Kayıt & Liste") { BackColor = Color.FromArgb(245, 246, 250) };
            tabAnaliz = new TabPage("Analiz & Raporlar") { BackColor = Color.White };
            tabControl.TabPages.Add(tabIslemler);
            tabControl.TabPages.Add(tabAnaliz);

            // ================= TAB 1: İŞLEMLER =================
            
            // --- ALT PANEL ---
            pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 120, BackColor = Color.White };
            tabIslemler.Controls.Add(pnlBottom);
            pnlBottom.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlBottom.ClientRectangle, Color.Transparent, 0, ButtonBorderStyle.None, Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid, Color.Transparent, 0, ButtonBorderStyle.None, Color.Transparent, 0, ButtonBorderStyle.None);

            lblGelir = new Label { Location = new Point(25, 25), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(46, 204, 113), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            lblGider = new Label { Location = new Point(25, 65), AutoSize = true, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.FromArgb(231, 76, 60), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            lblKalan = new Label { Location = new Point(pnlBottom.Width - 425, 25), Size = new Size(400, 30), AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 18, FontStyle.Bold), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            lblMesaj = new Label { Location = new Point(pnlBottom.Width - 425, 65), Size = new Size(400, 25), AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 10, FontStyle.Italic), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };
            pnlBottom.Controls.AddRange(new Control[] { lblGelir, lblGider, lblKalan, lblMesaj });

            // --- FİLTRE PANEL ---
            pnlFiltre = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.FromArgb(240, 242, 245) };
            tabIslemler.Controls.Add(pnlFiltre);
            pnlFiltre.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlFiltre.ClientRectangle, Color.Transparent, 0, ButtonBorderStyle.None, Color.Transparent, 0, ButtonBorderStyle.None, Color.Transparent, 0, ButtonBorderStyle.None, Color.FromArgb(210, 210, 210), 1, ButtonBorderStyle.Solid);

            dtpBaslangic = new DateTimePicker { Location = new Point(25, 15), Width = 130, Format = DateTimePickerFormat.Short };
            Label lblTire = new Label { Text = "-", Location = new Point(160, 18), AutoSize = true };
            dtpBitis = new DateTimePicker { Location = new Point(180, 15), Width = 130, Format = DateTimePickerFormat.Short };
            
            btnFiltrele = new Button { Location = new Point(330, 13), Width = 100, Height = 30, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnFiltrele.FlatAppearance.BorderSize = 0;
            btnFiltrele.Click += (s, e) => { _filtreAktif = true; EkraniGuncelle(); };

            btnTumunuGoster = new Button { Location = new Point(440, 13), Width = 130, Height = 30, BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnTumunuGoster.FlatAppearance.BorderSize = 0;
            btnTumunuGoster.Click += (s, e) => { _filtreAktif = false; EkraniGuncelle(); };

            btnCsvAktar = new Button { Location = new Point(pnlFiltre.Width - 175, 13), Width = 150, Height = 30, BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnCsvAktar.FlatAppearance.BorderSize = 0;
            btnCsvAktar.Click += (s, e) => CsvDisaAktar();

            pnlFiltre.Controls.AddRange(new Control[] { dtpBaslangic, lblTire, dtpBitis, btnFiltrele, btnTumunuGoster, btnCsvAktar });

            // --- ÜST PANEL ---
            pnlTop = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = Color.White };
            tabIslemler.Controls.Add(pnlTop);
            pnlTop.BringToFront(); 
            pnlTop.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlTop.ClientRectangle, Color.Transparent, 0, ButtonBorderStyle.None, Color.Transparent, 0, ButtonBorderStyle.None, Color.Transparent, 0, ButtonBorderStyle.None, Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid);

            lblKategori = new Label { Location = new Point(25, 25), AutoSize = true, ForeColor = Color.FromArgb(80, 80, 80) };
            cmbKategori = new ComboBox { Location = new Point(130, 22), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
            
            lblBaslik = new Label { Location = new Point(330, 25), AutoSize = true, ForeColor = Color.FromArgb(80, 80, 80) };
            txtAciklama = new TextBox { Location = new Point(410, 22), Width = pnlTop.Width - 435, Font = new Font("Segoe UI", 11), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };

            lblMiktar = new Label { Location = new Point(25, 75), AutoSize = true, ForeColor = Color.FromArgb(80, 80, 80) };
            txtMiktar = new TextBox { Location = new Point(130, 72), Width = 180, Font = new Font("Segoe UI", 11) };

            btnGelir = new Button { Location = new Point(pnlTop.Width - 350, 70), Width = 150, Height = 35, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnGelir.FlatAppearance.BorderSize = 0;
            btnGelir.Click += (s, e) => IslemEkle(true);

            btnGider = new Button { Location = new Point(pnlTop.Width - 180, 70), Width = 150, Height = 35, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Font = new Font("Segoe UI", 10, FontStyle.Bold), Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnGider.FlatAppearance.BorderSize = 0;
            btnGider.Click += (s, e) => IslemEkle(false);

            pnlTop.Controls.AddRange(new Control[] { lblKategori, cmbKategori, lblBaslik, txtAciklama, lblMiktar, txtMiktar, btnGelir, btnGider });

            // --- ORTA PANEL ---
            pnlOrta = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            tabIslemler.Controls.Add(pnlOrta);
            pnlOrta.BringToFront(); 

            lblListeBaslik = new Label { Location = new Point(25, 15), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(50, 50, 50) };
            pnlOrta.Controls.Add(lblListeBaslik);

            // Yeni Düzenle Butonu
            btnDuzenle = new Button { 
                Location = new Point(pnlOrta.Width - 335, 10), 
                Width = 150, Height = 32, 
                BackColor = Color.White, 
                ForeColor = Color.FromArgb(52, 152, 219), 
                FlatStyle = FlatStyle.Flat, 
                Cursor = Cursors.Hand, 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), 
                Anchor = AnchorStyles.Top | AnchorStyles.Right 
            };
            btnDuzenle.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btnDuzenle.Click += (s, e) => DuzenleSeciliIslem();
            pnlOrta.Controls.Add(btnDuzenle);

            btnSil = new Button { 
                Location = new Point(pnlOrta.Width - 175, 10), 
                Width = 150, Height = 32, 
                BackColor = Color.White, 
                ForeColor = Color.FromArgb(231, 76, 60), 
                FlatStyle = FlatStyle.Flat, 
                Cursor = Cursors.Hand, 
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold), 
                Anchor = AnchorStyles.Top | AnchorStyles.Right 
            };
            btnSil.FlatAppearance.BorderColor = Color.FromArgb(220, 220, 220);
            btnSil.Click += (s, e) => SilSeciliIslem();
            pnlOrta.Controls.Add(btnSil);

            dgvGecmis = new DataGridView
            {
                Location = new Point(25, 50),
                Size = new Size(pnlOrta.Width - 50, pnlOrta.Height - 65),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(235, 235, 235)
            };
            
            dgvGecmis.EnableHeadersVisualStyles = false;
            dgvGecmis.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(245, 246, 250);
            dgvGecmis.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(100, 100, 100);
            dgvGecmis.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvGecmis.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 246, 250);
            dgvGecmis.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvGecmis.ColumnHeadersHeight = 40;
            dgvGecmis.RowTemplate.Height = 35;
            dgvGecmis.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 238, 255);
            dgvGecmis.DefaultCellStyle.SelectionForeColor = Color.Black;

            var menu = new ContextMenuStrip();
            editItem = new ToolStripMenuItem();
            editItem.Click += (s, e) => DuzenleSeciliIslem();
            
            deleteItem = new ToolStripMenuItem();
            deleteItem.Click += (s, e) => SilSeciliIslem();
            
            menu.Items.Add(editItem);
            menu.Items.Add(deleteItem);
            dgvGecmis.ContextMenuStrip = menu;

            dgvGecmis.KeyDown += (s, e) => 
            {
                if (e.KeyCode == Keys.Delete) SilSeciliIslem();
            };

            // Çift tıklayarak düzenleme yapma
            dgvGecmis.CellDoubleClick += (s, e) => 
            {
                if (e.RowIndex >= 0) DuzenleSeciliIslem();
            };

            pnlOrta.Controls.Add(dgvGecmis);

            // ================= TAB 2: ANALİZ & GRAFİK =================
            pnlGrafik = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlGrafik.Paint += PnlGrafik_Paint;
            tabAnaliz.Controls.Add(pnlGrafik);
        }

        private void DiliDegistir(string dilKodu)
        {
            _ayarlar.Dil = dilKodu;
            try { File.WriteAllText(_ayarlarDosyasi, JsonSerializer.Serialize(_ayarlar)); } catch { }
            DiliGuncelle();
        }

        private void ParaBirimiDegistir(string birim)
        {
            _ayarlar.ParaBirimi = birim;
            try { File.WriteAllText(_ayarlarDosyasi, JsonSerializer.Serialize(_ayarlar)); } catch { }
            DiliGuncelle();
        }

        private void DiliGuncelle()
        {
            bool en = _ayarlar.Dil == "en";
            string pb = _ayarlar.ParaBirimi ?? "TL";

            menuTr.Checked = !en;
            menuEn.Checked = en;
            menuTl.Checked = (pb == "TL");
            menuUsd.Checked = (pb == "$");
            menuEur.Checked = (pb == "€");

            menuDosya.Text = en ? "File" : "Dosya";
            menuYeni.Text = en ? "New (Empty Budget)" : "Yeni (Boş Bütçe)";
            menuAc.Text = en ? "Open..." : "Aç...";
            menuKaydet.Text = en ? "Save" : "Kaydet";
            menuFarkliKaydet.Text = en ? "Save As..." : "Farklı Kaydet...";
            menuCikis.Text = en ? "Exit" : "Çıkış";
            menuAyarlar.Text = en ? "Settings" : "Ayarlar";
            menuDil.Text = en ? "Language (Dil)" : "Dil (Language)";
            menuParaBirimi.Text = en ? "Currency (Para Birimi)" : "Para Birimi (Currency)";

            tabIslemler.Text = en ? "Records & List" : "Kayıt & Liste";
            tabAnaliz.Text = en ? "Analysis & Reports" : "Analiz & Raporlar";

            lblKategori.Text = en ? "Category:" : "Kategori:";
            lblBaslik.Text = en ? "Description:" : "Açıklama:";
            lblMiktar.Text = en ? $"Amount ({pb}):" : $"Miktar ({pb}):";
            btnGelir.Text = en ? "➕ Add Income" : "➕ Gelir Ekle";
            btnGider.Text = en ? "➖ Add Expense" : "➖ Gider Ekle";

            btnFiltrele.Text = en ? "Filter" : "Filtrele";
            btnTumunuGoster.Text = en ? "Show All" : "Tümünü Göster";
            btnCsvAktar.Text = en ? "📥 Export CSV" : "📥 CSV İndir";

            lblListeBaslik.Text = en ? "Recent Transactions" : "Son İşlemler";
            if (btnSil != null) btnSil.Text = en ? "🗑️ Delete Selected" : "🗑️ Seçileni Sil";
            if (btnDuzenle != null) btnDuzenle.Text = en ? "✏️ Edit Selected" : "✏️ Seçileni Düzenle";
            
            if (editItem != null) editItem.Text = en ? "✏️ Edit Selected" : "✏️ Seçili İşlemi Düzenle";
            if (deleteItem != null) deleteItem.Text = en ? "🗑️ Delete Selected" : "🗑️ Seçili İşlemi Sil";

            // Kategori Listesini Güncelle
            string secili = "other";
            if (cmbKategori.SelectedItem is KategoriItem seciliItem) secili = seciliItem.Kod;

            var katList = Kategoriler.Kodlar.Select(k => new KategoriItem { Kod = k, Label = Kategoriler.GetLabel(k, en) }).ToList();
            cmbKategori.DataSource = katList;
            cmbKategori.DisplayMember = "Label";
            cmbKategori.ValueMember = "Kod";

            foreach (KategoriItem item in cmbKategori.Items)
            {
                if (item.Kod == secili) { cmbKategori.SelectedItem = item; break; }
            }

            BasligiGuncelle();
            EkraniGuncelle();
        }

        private void BasligiGuncelle()
        {
            bool en = _ayarlar.Dil == "en";
            string yeniButceTxt = en ? "Unsaved New Budget" : "Kaydedilmemiş Yeni Bütçe";
            string dosyaAdi = string.IsNullOrEmpty(_guncelDosyaYolu) ? yeniButceTxt : Path.GetFileName(_guncelDosyaYolu);
            string yildiz = _kaydedilmemisDegisiklikVar ? "*" : "";
            string appName = "Cost Check";
            this.Text = $"{appName} - [{dosyaAdi}]{yildiz}";
        }

        private void YeniDosya()
        {
            bool en = _ayarlar.Dil == "en";
            if (_kaydedilmemisDegisiklikVar)
            {
                var msg = en ? "You have unsaved changes. Do you want to save before creating a new budget?" : "Kaydedilmemiş değişiklikleriniz var. Yeni bir bütçe açmadan önce kaydetmek ister misiniz?";
                var title = en ? "Warning" : "Uyarı";

                var cevap = MessageBox.Show(msg, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (cevap == DialogResult.Yes)
                {
                    DosyaKaydet(false);
                    if (_kaydedilmemisDegisiklikVar) return; 
                }
                else if (cevap == DialogResult.Cancel)
                {
                    return;
                }
            }

            _islemler.Clear();
            _guncelDosyaYolu = null;
            _kaydedilmemisDegisiklikVar = false;
            _filtreAktif = false;
            EkraniGuncelle();
            BasligiGuncelle();
        }

        private void DosyaAc()
        {
            bool en = _ayarlar.Dil == "en";
            if (_kaydedilmemisDegisiklikVar)
            {
                var msg = en ? "You have unsaved changes. Do you want to save before opening a file?" : "Kaydedilmemiş değişiklikleriniz var. Başka bir dosya açmadan önce kaydetmek ister misiniz?";
                var title = en ? "Warning" : "Uyarı";

                var cevap = MessageBox.Show(msg, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (cevap == DialogResult.Yes)
                {
                    DosyaKaydet(false);
                    if (_kaydedilmemisDegisiklikVar) return;
                }
                else if (cevap == DialogResult.Cancel)
                {
                    return;
                }
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = en ? "JSON Files (*.json)|*.json|All Files (*.*)|*.*" : "JSON Dosyaları (*.json)|*.json|Tüm Dosyalar (*.*)|*.*";
                ofd.Title = en ? "Open Budget File" : "Bütçe Dosyası Aç";
                
                string saveFolder = Path.Combine(Application.StartupPath, "Saves");
                if (Directory.Exists(saveFolder))
                {
                    ofd.InitialDirectory = saveFolder;
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _guncelDosyaYolu = ofd.FileName;
                    VerileriYukle();
                    _kaydedilmemisDegisiklikVar = false;
                    _filtreAktif = false;
                    EkraniGuncelle();
                    BasligiGuncelle();
                }
            }
        }

        private void DosyaKaydet(bool farkliKaydet)
        {
            bool en = _ayarlar.Dil == "en";
            string saveFolder = Path.Combine(Application.StartupPath, "Saves");
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            bool mesajGoster = farkliKaydet;

            if (farkliKaydet || string.IsNullOrEmpty(_guncelDosyaYolu))
            {
                mesajGoster = true;
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = en ? "JSON Files (*.json)|*.json|All Files (*.*)|*.*" : "JSON Dosyaları (*.json)|*.json|Tüm Dosyalar (*.*)|*.*";
                    sfd.Title = farkliKaydet ? (en ? "Save Budget As" : "Bütçeyi Farklı Kaydet") : (en ? "Save Budget" : "Bütçeyi Kaydet");
                    sfd.InitialDirectory = saveFolder;
                    sfd.FileName = "Butce_" + DateTime.Now.ToString("yyyyMMdd") + ".json";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        _guncelDosyaYolu = sfd.FileName;
                    }
                    else
                    {
                        return; // Kullanıcı iptal etti
                    }
                }
            }

            try
            {
                string jsonString = JsonSerializer.Serialize(_islemler, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_guncelDosyaYolu, jsonString);
                
                _kaydedilmemisDegisiklikVar = false; 
                BasligiGuncelle();
                
                if (mesajGoster)
                {
                    MessageBox.Show(en ? "Data successfully saved." : "Veriler başarıyla kaydedildi.", en ? "Information" : "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(en ? $"Save failed:\n{ex.Message}" : $"Kayıt işlemi başarısız oldu:\n{ex.Message}", en ? "Error" : "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CsvDisaAktar()
        {
            bool en = _ayarlar.Dil == "en";
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV Dosyası (*.csv)|*.csv";
                sfd.FileName = "Bilanco_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var liste = GetFiltrelenmisIslemler().OrderByDescending(x => x.Tarih).ToList();
                        List<string> lines = new List<string>();
                        
                        string header = en ? "Date,Category,Type,Description,Amount" : "Tarih,Kategori,Tür,Açıklama,Miktar";
                        lines.Add(header);
                        
                        foreach (var i in liste)
                        {
                            string t = i.Tarih.ToString("yyyy-MM-dd HH:mm");
                            string k = Kategoriler.GetLabel(i.Kategori, en);
                            string tur = i.GelirMi ? (en ? "Income" : "Gelir") : (en ? "Expense" : "Gider");
                            string a = i.Aciklama.Replace(",", " ").Replace("\"", "");
                            string m = i.Miktar.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
                            lines.Add($"{t},{k},{tur},{a},{m}");
                        }
                        
                        File.WriteAllLines(sfd.FileName, lines, System.Text.Encoding.UTF8);
                        MessageBox.Show(en ? "Export successful!" : "Dışa aktarma başarılı!", en ? "Success" : "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(en ? "Error exporting CSV: " + ex.Message : "CSV oluşturulurken hata: " + ex.Message, en ? "Error" : "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SilSeciliIslem()
        {
            bool en = _ayarlar.Dil == "en";
            if (dgvGecmis.SelectedRows.Count > 0)
            {
                string id = dgvGecmis.SelectedRows[0].Cells["Id"].Value.ToString();
                var item = _islemler.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    var msg = en ? $"Are you sure you want to delete the transaction '{item.Aciklama}'?" : $"'{item.Aciklama}' işlemini silmek istediğinize emin misiniz?";
                    var title = en ? "Delete Transaction" : "İşlem Sil";

                    var cevap = MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (cevap == DialogResult.Yes)
                    {
                        _islemler.Remove(item);
                        _kaydedilmemisDegisiklikVar = true;
                        BasligiGuncelle();
                        EkraniGuncelle();
                    }
                }
            }
        }

        private void DuzenleSeciliIslem()
        {
            if (dgvGecmis.SelectedRows.Count > 0)
            {
                string id = dgvGecmis.SelectedRows[0].Cells["Id"].Value.ToString();
                var index = _islemler.FindIndex(i => i.Id == id);
                if (index != -1)
                {
                    var islem = _islemler[index];
                    using (var form = new IslemDuzenleForm(islem, _ayarlar))
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            _islemler[index] = form.IslemVerisi;
                            _kaydedilmemisDegisiklikVar = true;
                            BasligiGuncelle();
                            EkraniGuncelle();
                        }
                    }
                }
            }
        }

        private void IslemEkle(bool gelirMi)
        {
            bool en = _ayarlar.Dil == "en";
            string aciklama = txtAciklama.Text.Trim();
            if (string.IsNullOrEmpty(aciklama) || !decimal.TryParse(txtMiktar.Text, out decimal miktar) || miktar <= 0)
            {
                var msg = en ? "Please enter a valid description and a numerical amount greater than zero!" : "Lütfen geçerli bir açıklama ve sıfırdan büyük sayısal bir miktar girin!";
                var title = en ? "Missing Info" : "Eksik Bilgi";
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string kategori = "other";
            if (cmbKategori.SelectedItem is KategoriItem item) kategori = item.Kod;

            _islemler.Add(new Islem { Aciklama = aciklama, Miktar = miktar, GelirMi = gelirMi, Kategori = kategori });
            
            _kaydedilmemisDegisiklikVar = true;
            BasligiGuncelle();
            
            txtAciklama.Clear();
            txtMiktar.Clear();
            txtAciklama.Focus();
            EkraniGuncelle();
        }

        private List<Islem> GetFiltrelenmisIslemler()
        {
            if (!_filtreAktif) return _islemler;
            DateTime baslangic = dtpBaslangic.Value.Date;
            DateTime bitis = dtpBitis.Value.Date.AddDays(1).AddTicks(-1);
            return _islemler.Where(i => i.Tarih >= baslangic && i.Tarih <= bitis).ToList();
        }

        private void EkraniGuncelle()
        {
            bool en = _ayarlar.Dil == "en";
            string pb = _ayarlar.ParaBirimi ?? "TL";

            var filtrelenmisListe = GetFiltrelenmisIslemler();

            var bindingList = filtrelenmisListe.OrderByDescending(x => x.Tarih).Select(i => new IslemGorunum {
                Id = i.Id,
                Tarih = i.Tarih.ToString("dd.MM.yyyy HH:mm"),
                Kategori = Kategoriler.GetLabel(i.Kategori, en),
                Tur = i.GelirMi ? (en ? "Income" : "Gelir") : (en ? "Expense" : "Gider"),
                Aciklama = i.Aciklama,
                Miktar = i.Miktar
            }).ToList();

            dgvGecmis.DataSource = null;
            dgvGecmis.Columns.Clear();
            dgvGecmis.DataSource = bindingList;

            if (dgvGecmis.Columns.Count > 0)
            {
                if (dgvGecmis.Columns["Id"] != null) 
                    dgvGecmis.Columns["Id"].Visible = false;
                
                if (dgvGecmis.Columns["Tarih"] != null)
                {
                    dgvGecmis.Columns["Tarih"].HeaderText = en ? "Date" : "Tarih";
                    dgvGecmis.Columns["Tarih"].FillWeight = 20;
                }
                
                if (dgvGecmis.Columns["Kategori"] != null)
                {
                    dgvGecmis.Columns["Kategori"].HeaderText = en ? "Category" : "Kategori";
                    dgvGecmis.Columns["Kategori"].FillWeight = 20;
                }

                if (dgvGecmis.Columns["Tur"] != null)
                {
                    dgvGecmis.Columns["Tur"].HeaderText = en ? "Type" : "Tür";
                    dgvGecmis.Columns["Tur"].FillWeight = 15;
                }
                
                if (dgvGecmis.Columns["Aciklama"] != null)
                {
                    dgvGecmis.Columns["Aciklama"].HeaderText = en ? "Description" : "Açıklama";
                    dgvGecmis.Columns["Aciklama"].FillWeight = 30;
                }
                
                if (dgvGecmis.Columns["Miktar"] != null)
                {
                    dgvGecmis.Columns["Miktar"].HeaderText = en ? $"Amount ({pb})" : $"Tutar ({pb})";
                    dgvGecmis.Columns["Miktar"].FillWeight = 15;
                    dgvGecmis.Columns["Miktar"].DefaultCellStyle.Format = "N2";
                    dgvGecmis.Columns["Miktar"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            foreach (DataGridViewRow row in dgvGecmis.Rows)
            {
                string turVal = row.Cells["Tur"].Value?.ToString();
                if (turVal == "Gelir" || turVal == "Income")
                {
                    row.Cells["Tur"].Style.ForeColor = Color.FromArgb(46, 204, 113);
                    row.Cells["Tur"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
                else
                {
                    row.Cells["Tur"].Style.ForeColor = Color.FromArgb(231, 76, 60);
                    row.Cells["Tur"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                }
            }

            decimal toplamGelir = filtrelenmisListe.Where(i => i.GelirMi).Sum(i => i.Miktar);
            decimal toplamGider = filtrelenmisListe.Where(i => !i.GelirMi).Sum(i => i.Miktar);
            decimal kalan = toplamGelir - toplamGider;

            lblGelir.Text = en ? $"Total Income: {toplamGelir:N2} {pb}" : $"Toplam Gelir: {toplamGelir:N2} {pb}";
            lblGider.Text = en ? $"Total Expense: {toplamGider:N2} {pb}" : $"Toplam Gider: {toplamGider:N2} {pb}";
            lblKalan.Text = en ? $"Balance: {kalan:N2} {pb}" : $"Bakiye: {kalan:N2} {pb}";

            if (kalan > 0)
            {
                lblKalan.ForeColor = Color.FromArgb(46, 204, 113);
                lblMesaj.Text = en ? "✅ Financial status is positive, keep saving." : "✅ Finansal durumunuz pozitif, tasarruf etmeye devam edin.";
                lblMesaj.ForeColor = Color.FromArgb(46, 204, 113);
            }
            else if (kalan == 0)
            {
                lblKalan.ForeColor = Color.DarkOrange;
                lblMesaj.Text = en ? "⚖️ Your budget is at breakeven. Avoid extra expenses." : "⚖️ Bütçeniz başa baş noktasında. Ekstra masraflardan kaçının.";
                lblMesaj.ForeColor = Color.DarkOrange;
            }
            else
            {
                lblKalan.ForeColor = Color.FromArgb(231, 76, 60);
                lblMesaj.Text = en ? $"🚨 WARNING! You are short by {Math.Abs(kalan):N2} {pb}. Take action!" : $"🚨 DİKKAT! {Math.Abs(kalan):N2} {pb} açıktasınız. Acil önlem alın!";
                lblMesaj.ForeColor = Color.FromArgb(231, 76, 60);
            }

            if (pnlGrafik != null) pnlGrafik.Invalidate();
        }

        private void PnlGrafik_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            var giderler = GetFiltrelenmisIslemler().Where(i => !i.GelirMi).ToList();
            bool en = _ayarlar.Dil == "en";
            string pb = _ayarlar.ParaBirimi ?? "TL";

            if (!giderler.Any())
            {
                string msg = en ? "No expense data to display for the selected period." : "Seçili dönem için gösterilecek gider verisi bulunamadı.";
                var size = g.MeasureString(msg, new Font("Segoe UI", 12));
                g.DrawString(msg, new Font("Segoe UI", 12), Brushes.Gray, (pnlGrafik.Width - size.Width) / 2, (pnlGrafik.Height - size.Height) / 2);
                return;
            }

            var gruplar = giderler.GroupBy(i => i.Kategori)
                                  .Select(grup => new { Kategori = grup.Key, Toplam = grup.Sum(x => x.Miktar) })
                                  .OrderByDescending(x => x.Toplam)
                                  .ToList();

            decimal genelToplam = gruplar.Sum(x => x.Toplam);
            
            int chartSize = Math.Min(pnlGrafik.Width - 350, pnlGrafik.Height - 100);
            if (chartSize < 150) chartSize = 150;
            
            Rectangle rect = new Rectangle(50, 50, chartSize, chartSize);
            float startAngle = 0;
            
            Color[] renkler = { Color.FromArgb(231, 76, 60), Color.FromArgb(52, 152, 219), Color.FromArgb(241, 196, 15), Color.FromArgb(155, 89, 182), Color.FromArgb(46, 204, 113), Color.FromArgb(230, 126, 34), Color.FromArgb(52, 73, 94), Color.FromArgb(149, 165, 166), Color.FromArgb(211, 84, 0) };

            int colorIdx = 0;
            int legendY = 50;
            int legendX = rect.Right + 50;

            foreach (var grup in gruplar)
            {
                float sweepAngle = (float)((grup.Toplam / genelToplam) * 360m);
                Color c = renkler[colorIdx % renkler.Length];
                
                using (Brush b = new SolidBrush(c))
                {
                    g.FillPie(b, rect, startAngle, sweepAngle);
                }

                // Legend
                using (Brush b = new SolidBrush(c))
                {
                    g.FillRectangle(b, legendX, legendY, 20, 20);
                }
                
                string lbl = $"{Kategoriler.GetLabel(grup.Kategori, en)}: {grup.Toplam:N2} {pb} ({(grup.Toplam / genelToplam):P1})";
                g.DrawString(lbl, new Font("Segoe UI", 11, FontStyle.Bold), Brushes.DarkSlateGray, legendX + 30, legendY);
                
                legendY += 35;
                startAngle += sweepAngle;
                colorIdx++;
            }
            
            // Donut Hole
            using (Brush b = new SolidBrush(Color.White))
            {
                int holeSize = rect.Width / 2;
                g.FillEllipse(b, rect.X + holeSize / 2, rect.Y + holeSize / 2, holeSize, holeSize);
            }

            // Center Text
            string totalText = en ? "Total Expenses" : "Toplam Gider";
            string totalVal = $"{genelToplam:N2} {pb}";
            
            SizeF s1 = g.MeasureString(totalText, new Font("Segoe UI", 10));
            SizeF s2 = g.MeasureString(totalVal, new Font("Segoe UI", 12, FontStyle.Bold));

            g.DrawString(totalText, new Font("Segoe UI", 10), Brushes.Gray, rect.X + rect.Width / 2 - s1.Width / 2, rect.Y + rect.Height / 2 - s1.Height);
            g.DrawString(totalVal, new Font("Segoe UI", 12, FontStyle.Bold), Brushes.Black, rect.X + rect.Width / 2 - s2.Width / 2, rect.Y + rect.Height / 2);
        }

        private void VerileriYukle()
        {
            if (string.IsNullOrEmpty(_guncelDosyaYolu)) return;

            try
            {
                if (File.Exists(_guncelDosyaYolu))
                {
                    string jsonString = File.ReadAllText(_guncelDosyaYolu);
                    _islemler = JsonSerializer.Deserialize<List<Islem>>(jsonString) ?? new List<Islem>();
                    
                    foreach (var islem in _islemler)
                    {
                        if (string.IsNullOrEmpty(islem.Id)) islem.Id = Guid.NewGuid().ToString();
                        if (islem.Tarih == default) islem.Tarih = DateTime.Now;
                        if (string.IsNullOrEmpty(islem.Kategori)) islem.Kategori = "other";
                    }
                }
            }
            catch (Exception)
            {
                _islemler = new List<Islem>();
            }
        }
    }
    
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AnaPencere());
        }
    }
}