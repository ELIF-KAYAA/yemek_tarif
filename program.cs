using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace YemekTarifleriUygulamasi
{
public class Recipe
{
    public int Id { get; set; } // Tarifin benzersiz kimliği
    public string Name { get; set; } // Tarifin adı
    public string Category { get; set; } // Tarifin kategorisi
    public int PreparationTime { get; set; } // Tarifin hazırlanma süresi (dakika cinsinden)
    public string Instructions { get; set; } // Tarifin talimatları
}

public class MainForm : Form
{
    private DataGridView recipeDataGridView; // Tarifleri göstermek için veri ızgarası
    private Button addButton; // Tarif eklemek için buton
    private Button updateButton; // Tarif güncellemek için buton
    private Button deleteButton; // Tarif silmek için buton
    private Button sortByNameButton; // Tarifleri adıyla sıralamak için buton
    private Button sortByCategoryButton; // Tarifleri kategoriye göre sıralamak için buton
    private Button sortByPreparationTimeButton; // Tarifleri hazırlama süresine göre sıralamak için buton
    private List<Recipe> recipes; // Tariflerin tutulacağı liste
    private SQLiteConnection connection; // Veritabanı bağlantısı
    private PictureBox recipeImage; // Tarif resmini göstermek için resim kutusu
    private Panel imagePanel; // Resim kutusunu barındıracak panel

    // Başlıklar için sabitler
    private const string ID_HEADER = "ID"; // ID başlığı
    private const string NAME_HEADER = "Tarif Adı"; // Tarif adı başlığı
    private const string CATEGORY_HEADER = "Kategori"; // Kategori başlığı
    private const string PREPARATION_TIME_HEADER = "Hazırlama Süresi (dk)"; // Hazırlama süresi başlığı
    private const string INSTRUCTIONS_HEADER = "Talimatlar"; // Talimatlar başlığı

    private TextBox searchTextBox; // Tek bir arama kutusu

    public MainForm()
    {
        InitializeComponent(); // Form bileşenlerini başlat
        recipes = new List<Recipe>(); // Tarif listesi oluştur
        InitializeDatabase(); // Veritabanını başlat
        LoadRecipes(); // Tarifleri yükle
    }
private void InitializeComponent()
{
    this.Text = "Yemek Tarifleri Uygulaması"; // Formun başlığı
    this.Size = new System.Drawing.Size(800, 600); // Formun boyutu

    recipeDataGridView = new DataGridView { Dock = DockStyle.Left, Width = 600 }; // Tarifleri gösterecek veri ızgarası
    recipeDataGridView.DoubleClick += RecipeDataGridView_DoubleClick; // Çift tıklama olayını bağla

    addButton = new Button { Text = "Tarif Ekle", Dock = DockStyle.Top }; // Tarif eklemek için buton
    addButton.Click += AddButton_Click; // Butona tıklama olayını bağla

    updateButton = new Button { Text = "Tarif Güncelle", Dock = DockStyle.Top }; // Tarif güncellemek için buton
    updateButton.Click += UpdateButton_Click; // Butona tıklama olayını bağla

    deleteButton = new Button { Text = "Tarif Sil", Dock = DockStyle.Top }; // Tarif silmek için buton
    deleteButton.Click += DeleteButton_Click; // Butona tıklama olayını bağla

    sortByNameButton = new Button { Text = "İsme Göre Sırala", Dock = DockStyle.Top }; // İsme göre sıralama butonu
    sortByNameButton.Click += SortByNameButton_Click; // Butona tıklama olayını bağla

    sortByCategoryButton = new Button { Text = "Kategorilere Göre Sırala", Dock = DockStyle.Top }; // Kategorilere göre sıralama butonu
    sortByCategoryButton.Click += SortByCategoryButton_Click; // Butona tıklama olayını bağla

    sortByPreparationTimeButton = new Button { Text = "Pişirme Süresine Göre Sırala", Dock = DockStyle.Top }; // Pişirme süresine göre sıralama butonu
    sortByPreparationTimeButton.Click += SortByPreparationTimeButton_Click; // Butona tıklama olayını bağla

    // Tek bir arama kutusu ekliyoruz
    searchTextBox = new TextBox { PlaceholderText = "Tarif veya Kategori Ara", Dock = DockStyle.Top }; // Arama kutusu
    searchTextBox.TextChanged += SearchTextBox_TextChanged; // Arama kutusuna yazıldıkça arama yapması için olay bağlanıyor

    // Kontrolleri forma ekle
    this.Controls.Add(recipeDataGridView); 
    this.Controls.Add(sortByPreparationTimeButton); 
    this.Controls.Add(sortByCategoryButton);
    this.Controls.Add(sortByNameButton);
    this.Controls.Add(deleteButton);
    this.Controls.Add(updateButton);
    this.Controls.Add(addButton);
    this.Controls.Add(searchTextBox); // Arama kutusunu ekle

    imagePanel = new Panel // Resim paneli
    {
        Dock = DockStyle.Fill, // Paneli formun tamamını kaplayacak şekilde ayarla
        Width = 300 // Panelin genişliği
    };

    recipeImage = new PictureBox // Tarif resmini gösterecek resim kutusu
    {
        Dock = DockStyle.Fill, // Resim kutusunu panelin tamamını kaplayacak şekilde ayarla
        SizeMode = PictureBoxSizeMode.StretchImage // Resmin boyutunu uzatarak göster
    };

    imagePanel.Controls.Add(recipeImage); // Resim kutusunu panelin içine ekle
    this.Controls.Add(imagePanel); // Paneli forma ekle

    recipeImage.Image = Image.FromFile("image.jpg"); // Resmi yükle
}

private void InitializeDatabase()
{
    connection = new SQLiteConnection("Data Source=recipes.db;Version=3;"); // Veritabanı bağlantısı oluştur
    connection.Open(); // Bağlantıyı aç

    // Tarifler tablosunu oluştur
    string createTableQuery = "CREATE TABLE IF NOT EXISTS Recipes (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Category TEXT, PreparationTime INTEGER, Instructions TEXT)";
    using (var command = new SQLiteCommand(createTableQuery, connection))
    {
        command.ExecuteNonQuery(); // Sorguyu çalıştır
    }
}

private void LoadRecipes()
{
    recipes.Clear(); // Tarif listesini temizle
    string selectQuery = "SELECT * FROM Recipes"; // Tüm tarifleri seçen sorgu
    using (var command = new SQLiteCommand(selectQuery, connection))
    {
        using (var reader = command.ExecuteReader()) // Sorguyu çalıştır ve sonuçları oku
        {
            while (reader.Read()) // Her bir satırı oku
            {
                // Tarif nesnelerini oluştur ve tarif listesine ekle
                recipes.Add(new Recipe
                {
                    Id = reader.GetInt32(0), // ID
                    Name = reader.GetString(1), // Adı
                    Category = reader.GetString(2), // Kategorisi
                    PreparationTime = reader.GetInt32(3), // Hazırlama süresi
                    Instructions = reader.GetString(4) // Talimatlar
                });
            }
        }
    }

    recipeDataGridView.DataSource = recipes.ToList(); // Tarif listesini veri ızgarasına ata
    // Başlıkları Türkçe olarak güncelle
    recipeDataGridView.Columns[0].HeaderText = "ID"; // ID başlığı
    recipeDataGridView.Columns[1].HeaderText = "Tarif Adı"; // Tarif adı başlığı
    recipeDataGridView.Columns[2].HeaderText = "Kategori"; // Kategori başlığı
    recipeDataGridView.Columns[3].HeaderText = "Hazırlama Süresi (dk)"; // Hazırlama süresi başlığı
    recipeDataGridView.Columns[4].HeaderText = "Talimatlar"; // Talimatlar başlığı
}

private void RecipeDataGridView_DoubleClick(object sender, EventArgs e)
{
    // Seçilen tarifin detaylarını göster
    if (recipeDataGridView.SelectedRows.Count > 0) 
    {
        var selectedRecipe = (Recipe)recipeDataGridView.SelectedRows[0].DataBoundItem; // Seçilen tarif
        MessageBox.Show($"Adı: {selectedRecipe.Name}\nKategori: {selectedRecipe.Category}\nHazırlama Süresi: {selectedRecipe.PreparationTime} dakika\nTalimatlar: {selectedRecipe.Instructions}", "Tarif Detayları");
    }
}

private void AddButton_Click(object sender, EventArgs e)
{
    var addRecipeForm = new AddRecipeForm(this); // Yeni tarif ekleme formunu oluştur
    addRecipeForm.ShowDialog(); // Formu göster
}

public void AddRecipe(Recipe recipe)
{
    recipes.Add(recipe); // Tarifi listeye ekle
    string insertQuery = "INSERT INTO Recipes (Name, Category, PreparationTime, Instructions) VALUES (@Name, @Category, @PreparationTime, @Instructions)"; // Eklemek için sorgu
    using (var command = new SQLiteCommand(insertQuery, connection))
    {
        command.Parameters.AddWithValue("@Name", recipe.Name); // Parametreleri ekle
        command.Parameters.AddWithValue("@Category", recipe.Category);
        command.Parameters.AddWithValue("@PreparationTime", recipe.PreparationTime);
        command.Parameters.AddWithValue("@Instructions", recipe.Instructions);
        command.ExecuteNonQuery(); // Sorguyu çalıştır
    }

    LoadRecipes(); // Tarifleri yeniden yükle
}

private void UpdateButton_Click(object sender, EventArgs e)
{
    // Güncellemek için seçilen tarif var mı kontrol et
    if (recipeDataGridView.SelectedRows.Count > 0)
    {
        var selectedRecipe = (Recipe)recipeDataGridView.SelectedRows[0].DataBoundItem; // Seçilen tarif
        var updateRecipeForm = new UpdateRecipeForm(this, selectedRecipe); // Güncelleme formunu oluştur
        updateRecipeForm.ShowDialog(); // Formu göster
    }
    else
    {
        MessageBox.Show("Güncellemek için bir tarif seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Uyarı mesajı
    }
}

public void UpdateRecipe(Recipe recipe)
{
    // Tarif bilgilerini güncelleme sorgusu
    string updateQuery = "UPDATE Recipes SET Name = @Name, Category = @Category, PreparationTime = @PreparationTime, Instructions = @Instructions WHERE Id = @Id";
    using (var command = new SQLiteCommand(updateQuery, connection))
    {
        command.Parameters.AddWithValue("@Id", recipe.Id); // ID parametresini ekle
        command.Parameters.AddWithValue("@Name", recipe.Name); // Diğer parametreleri ekle
        command.Parameters.AddWithValue("@Category", recipe.Category);
        command.Parameters.AddWithValue("@PreparationTime", recipe.PreparationTime);
        command.Parameters.AddWithValue("@Instructions", recipe.Instructions);
        command.ExecuteNonQuery(); // Sorguyu çalıştır
    }

    LoadRecipes(); // Tarifleri yeniden yükle
}

private void DeleteButton_Click(object sender, EventArgs e)
{
    // Silmek için seçilen tarif var mı kontrol et
    if (recipeDataGridView.SelectedRows.Count > 0)
    {
        var selectedRecipe = (Recipe)recipeDataGridView.SelectedRows[0].DataBoundItem; // Seçilen tarif
        string deleteQuery = "DELETE FROM Recipes WHERE Id = @Id"; // Silme sorgusu
        using (var command = new SQLiteCommand(deleteQuery, connection))
        {
            command.Parameters.AddWithValue("@Id", selectedRecipe.Id); // ID parametresini ekle
            command.ExecuteNonQuery(); // Sorguyu çalıştır
        }

        recipes.Remove(selectedRecipe); // Tarifi listeden kaldır
        LoadRecipes(); // Tarifleri yeniden yükle
    }
    else
    {
        MessageBox.Show("Silmek için bir tarif seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning); // Uyarı mesajı
    }
}


        private void SortByNameButton_Click(object sender, EventArgs e)
        {
            recipeDataGridView.DataSource = recipes.OrderBy(r => r.Name).ToList();
        }

        private void SortByCategoryButton_Click(object sender, EventArgs e)
        {
            recipeDataGridView.DataSource = recipes.OrderBy(r => r.Category).ToList();
        }

        private void SortByPreparationTimeButton_Click(object sender, EventArgs e)
        {
            recipeDataGridView.DataSource = recipes.OrderBy(r => r.PreparationTime).ToList();
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = searchTextBox.Text.Trim().ToLower(); // arama terimi al

            // tarifler isme ve kategoriye göre fiktrele
            var filteredRecipes = recipes.Where(r =>
                string.IsNullOrEmpty(searchTerm) || r.Name.ToLower().Contains(searchTerm) || r.Category.ToLower().Contains(searchTerm)).ToList();

            // DataGridView i güncelle
            recipeDataGridView.DataSource = null; // mevcut veri kaynağını güncelle
            recipeDataGridView.DataSource = recipes.ToList();
            // Başlıkları Türkçe olarak güncelle
            recipeDataGridView.Columns[0].HeaderText = "ID";
            recipeDataGridView.Columns[1].HeaderText = "Tarif Adı";
            recipeDataGridView.Columns[2].HeaderText = "Kategori";
            recipeDataGridView.Columns[3].HeaderText = "Hazırlama Süresi (dk)";
            recipeDataGridView.Columns[4].HeaderText = "Talimatlar";

            recipeDataGridView.DataSource = filteredRecipes; // filtrelenmiş tarifleri bağla
        }


        // Uygulamayı kapatırken veritabanı bağlantısını kapat
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            connection.Close();
            base.OnFormClosing(e);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    // Yeni tarif ekleme formu
    public class AddRecipeForm : Form
    {
        private MainForm mainForm;
        private TextBox nameTextBox;
        private TextBox categoryTextBox;
        private TextBox preparationTimeTextBox;
        private TextBox instructionsTextBox;
        private Button addButton;

        public AddRecipeForm(MainForm mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Yeni Tarif Ekle";
            this.Size = new System.Drawing.Size(400, 300);

            nameTextBox = new TextBox { PlaceholderText = "Tarif Adı", Dock = DockStyle.Top };
            categoryTextBox = new TextBox { PlaceholderText = "Kategori", Dock = DockStyle.Top };
            preparationTimeTextBox = new TextBox { PlaceholderText = "Hazırlama Süresi (dk)", Dock = DockStyle.Top };
            instructionsTextBox = new TextBox { PlaceholderText = "Talimatlar", Dock = DockStyle.Top, Multiline = true, Height = 100 };
            addButton = new Button { Text = "Ekle", Dock = DockStyle.Top };
            addButton.Click += AddButton_Click;

            this.Controls.Add(addButton);
            this.Controls.Add(instructionsTextBox);
            this.Controls.Add(preparationTimeTextBox);
            this.Controls.Add(categoryTextBox);
            this.Controls.Add(nameTextBox);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var newRecipe = new Recipe
            {
                Name = nameTextBox.Text,
                Category = categoryTextBox.Text,
                PreparationTime = int.Parse(preparationTimeTextBox.Text),
                Instructions = instructionsTextBox.Text
            };

            mainForm.AddRecipe(newRecipe);
            this.Close();
        }
    }

    // Tarif güncelleme formu
    public class UpdateRecipeForm : Form
    {
        private MainForm mainForm;
        private Recipe recipe;
        private TextBox nameTextBox;
        private TextBox categoryTextBox;
        private TextBox preparationTimeTextBox;
        private TextBox instructionsTextBox;
        private Button updateButton;

        public UpdateRecipeForm(MainForm mainForm, Recipe recipe)
        {
            this.mainForm = mainForm;
            this.recipe = recipe;
            InitializeComponent(); // arayüzü başlatır
            LoadRecipeDetails(); // tarif detaylarını yükler
        }

        private void InitializeComponent()
        {
            this.Text = "Tarif Güncelle";
            this.Size = new System.Drawing.Size(400, 300);

            nameTextBox = new TextBox { PlaceholderText = "Tarif Adı", Dock = DockStyle.Top };
            categoryTextBox = new TextBox { PlaceholderText = "Kategori", Dock = DockStyle.Top };
            preparationTimeTextBox = new TextBox { PlaceholderText = "Hazırlama Süresi (dk)", Dock = DockStyle.Top };
            instructionsTextBox = new TextBox { PlaceholderText = "Talimatlar", Dock = DockStyle.Top, Multiline = true, Height = 100 };
            updateButton = new Button { Text = "Güncelle", Dock = DockStyle.Top };
            updateButton.Click += UpdateButton_Click;

            this.Controls.Add(updateButton);
            this.Controls.Add(instructionsTextBox);
            this.Controls.Add(preparationTimeTextBox);
            this.Controls.Add(categoryTextBox);
            this.Controls.Add(nameTextBox);
        }

        private void LoadRecipeDetails()
        {
            nameTextBox.Text = recipe.Name;
            categoryTextBox.Text = recipe.Category;
            preparationTimeTextBox.Text = recipe.PreparationTime.ToString();
            instructionsTextBox.Text = recipe.Instructions;
        }

        private void UpdateButton_Click(object sender, EventArgs e)
        {
            recipe.Name = nameTextBox.Text;
            recipe.Category = categoryTextBox.Text;
            recipe.PreparationTime = int.Parse(preparationTimeTextBox.Text);
            recipe.Instructions = instructionsTextBox.Text;

            mainForm.UpdateRecipe(recipe);
            this.Close();
        }
    }
}
