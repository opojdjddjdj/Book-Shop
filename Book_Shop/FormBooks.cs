using System.Drawing.Drawing2D;
using Npgsql;

namespace Book_Shop
{
    public partial class FormBooks : Form
    {
        private const string ConnectionString =
            "Host=localhost;Port=5432;Database=Book_Shop;Username=postgres;Password=1111";
        private const string PlaceholderName = "book_placeholder";
        private const string LogoName = "Читай-город";
        private const int CoverMaxWidth = 120;
        private const int CoverMaxHeight = 100;
        private const int LongLoanDays = 30;
        private static readonly Color ListBackColor = ColorTranslator.FromHtml("#F0F8FF");
        private static readonly Color NoCopiesColor = ColorTranslator.FromHtml("#FFCCCC");
        private static readonly Color FewCopiesColor = ColorTranslator.FromHtml("#FFF3CD");
        private static readonly Color LongLoanColor = ColorTranslator.FromHtml("#D4EDDA");

        private const string SelectBooksSql = @"
            SELECT b.isbn,
                   b.title,
                   a.author_name,
                   g.genre_name,
                   p.publisher_name,
                   b.publication_year,
                   b.pages,
                   b.total_copies,
                   b.available_copies,
                   b.description,
                   b.cover_image,
                   EXISTS (SELECT 1
                           FROM book_loans bl
                           WHERE bl.id_book = b.id
                             AND bl.date_returned IS NULL
                             AND bl.date_issued < CURRENT_DATE - @days) AS has_long_loan
            FROM books b
            JOIN authors a ON a.id = b.id_author
            JOIN genres g ON g.id = b.id_genre
            JOIN publishers p ON p.id = b.id_publisher
            ORDER BY b.id";

        private sealed record BookRow(
            string Isbn,
            string Title,
            string Author,
            string Genre,
            string Publisher,
            int PublicationYear,
            int Pages,
            int TotalCopies,
            int AvailableCopies,
            string Description,
            string? CoverImage,
            bool HasLongLoan);

        public FormBooks()
        {
            InitializeComponent();
            CreateColumns();
            picLogo.Image = GetResourceImage(LogoName);
            LoadBooks();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            dgvBooks.CurrentCell = null;
            dgvBooks.ClearSelection();
        }

        private void CreateColumns()
        {
            var colCover = new DataGridViewImageColumn();
            colCover.Name = "colCover";
            colCover.HeaderText = "Обложка";
            colCover.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colCover.Width = CoverMaxWidth + 20;
            dgvBooks.Columns.Add(colCover);

            AddTextColumn("colIsbn", "ISBN", 130, DataGridViewContentAlignment.MiddleLeft);
            AddTextColumn("colTitle", "Название", 200, DataGridViewContentAlignment.MiddleLeft);
            AddTextColumn("colAuthor", "Автор", 150, DataGridViewContentAlignment.MiddleLeft);
            AddTextColumn("colGenre", "Жанр", 110, DataGridViewContentAlignment.MiddleLeft);
            AddTextColumn("colPublisher", "Издательство", 130, DataGridViewContentAlignment.MiddleLeft);
            AddTextColumn("colYear", "Год издания", 90, DataGridViewContentAlignment.MiddleCenter);
            AddTextColumn("colPages", "Страниц", 80, DataGridViewContentAlignment.MiddleCenter);
            AddTextColumn("colTotal", "Всего экземпляров", 110, DataGridViewContentAlignment.MiddleCenter);
            AddTextColumn("colAvailable", "Доступно экземпляров", 120, DataGridViewContentAlignment.MiddleCenter);
            var colDescription = AddTextColumn("colDescription", "Аннотация", 250, DataGridViewContentAlignment.MiddleLeft);
            colDescription.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDescription.MinimumWidth = 250;
        }

        private DataGridViewTextBoxColumn AddTextColumn(string name, string headerText, int width, DataGridViewContentAlignment alignment)
        {
            var column = new DataGridViewTextBoxColumn();
            column.Name = name;
            column.HeaderText = headerText;
            column.Width = width;
            column.DefaultCellStyle.Alignment = alignment;
            dgvBooks.Columns.Add(column);
            return column;
        }

        private void LoadBooks()
        {
            List<BookRow> books = ReadBooksFromDatabase();
            dgvBooks.SuspendLayout();
                dgvBooks.Rows.Clear();
                foreach (BookRow book in books)
                {
                    int rowIndex = dgvBooks.Rows.Add(
                        LoadCover(book.CoverImage),
                        book.Isbn,
                        book.Title,
                        book.Author,
                        book.Genre,
                        book.Publisher,
                        book.PublicationYear,
                        book.Pages,
                        book.TotalCopies,
                        book.AvailableCopies,
                        book.Description);
                    dgvBooks.Rows[rowIndex].DefaultCellStyle.BackColor = GetRowColor(book);
                }
                dgvBooks.ResumeLayout();
                dgvBooks.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
                lblCount.Text = $"Книг в каталоге: {books.Count}";
        }

        private static List<BookRow> ReadBooksFromDatabase()
        {
            var books = new List<BookRow>();
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();
            using var command = new NpgsqlCommand(SelectBooksSql, connection);
            command.Parameters.AddWithValue("days", LongLoanDays);
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                books.Add(new BookRow(
                    Isbn: reader.GetString(0),
                    Title: reader.GetString(1),
                    Author: reader.GetString(2),
                    Genre: reader.GetString(3),
                    Publisher: reader.GetString(4),
                    PublicationYear: reader.GetInt32(5),
                    Pages: reader.GetInt32(6),
                    TotalCopies: reader.GetInt32(7),
                    AvailableCopies: reader.GetInt32(8),
                    Description: reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    CoverImage: reader.IsDBNull(10) ? null : reader.GetString(10),
                    HasLongLoan: reader.GetBoolean(11)));
            }
            return books;
        }
        private static Color GetRowColor(BookRow book)
        {
            if (book.AvailableCopies == 0)
            {
                return NoCopiesColor;
            }

            if (book.AvailableCopies <= 2)
            {
                return FewCopiesColor;
            }

            if (book.HasLongLoan)
            {
                return LongLoanColor;
            }
            return ListBackColor;
        }
        private static Image LoadCover(string? coverImage)
        {
            Image? source = null;

            if (!string.IsNullOrWhiteSpace(coverImage))
            {
                source = GetResourceImage(Path.GetFileNameWithoutExtension(coverImage))
                      ?? GetResourceImage(Path.GetFileName(coverImage));
            }

            source ??= GetResourceImage(PlaceholderName);

            return source == null ? new Bitmap(1, 1) : CreateThumbnail(source);
        }
        private static Image? GetResourceImage(string name)
        {
            try
            {
                var resourceManager = Resource1.ResourceManager;

                resourceManager.IgnoreCase = true;
                return resourceManager.GetObject(name) as Image;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private static Image CreateThumbnail(Image source)
        {
            double scale = Math.Min((double)CoverMaxWidth / source.Width, (double)CoverMaxHeight / source.Height);
            if (scale > 1)
            {
                scale = 1;
            }
            int width = Math.Max(1, (int)(source.Width * scale));
            int height = Math.Max(1, (int)(source.Height * scale));
            var thumbnail = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(thumbnail);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(source, 0, 0, width, height);
            return thumbnail;
        }
        private void FormBooks_Load(object sender, EventArgs e)
        {

        }
    }
}
