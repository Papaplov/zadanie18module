using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using zadanie18.Models;

namespace zadanie18
{
    public partial class MainWindow : Window
    {
        private TEST1Entities _context;
        private ObservableCollection<Books> _allBooks;
        private List<Genres> _genresList;
        private ListCollectionView _filteredView;

        public List<Genres> Genres => _genresList;

        public MainWindow()
        {
            InitializeComponent();
            _context = new TEST1Entities();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загружаем книги с жанрами
                _context.Books.Include(b => b.Genres).Load();
                _allBooks = _context.Books.Local;

                // Загружаем жанры
                _genresList = _context.Genres.ToList();

                // Заполняем ComboBox фильтра
                GenreComboBox.Items.Clear();
                GenreComboBox.Items.Add("Все");
                foreach (var g in _genresList)
                    GenreComboBox.Items.Add(g.Name);
                GenreComboBox.SelectedIndex = 0;

                // Создаём представление для фильтрации/сортировки
                _filteredView = (ListCollectionView)CollectionViewSource.GetDefaultView(_allBooks);

                // Устанавливаем фильтр
                _filteredView.Filter = FilterPredicate;

                // Применяем сортировку
                ApplySort();

                // Привязываем к DataGrid
                BooksDataGrid.ItemsSource = _filteredView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Предикат фильтрации
        private bool FilterPredicate(object item)
        {
            var book = item as Books;
            if (book == null) return false;

            // Поиск по названию или автору
            string search = SearchTextBox.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                if (!(book.Title?.ToLower().Contains(search) ?? false) &&
                    !(book.Author?.ToLower().Contains(search) ?? false))
                    return false;
            }

            // Фильтр по жанру
            if (GenreComboBox.SelectedItem != null && GenreComboBox.SelectedItem.ToString() != "Все")
            {
                string selectedGenre = GenreComboBox.SelectedItem.ToString();
                if (book.Genres == null || book.Genres.Name != selectedGenre)
                    return false;
            }

            return true;
        }

        // Применение сортировки (исправленная версия)
        private void ApplySort()
        {
            if (_filteredView == null) return;

            if (SortComboBox.SelectedItem is ComboBoxItem sortItem)
            {
                string sortField = sortItem.Tag.ToString();
                _filteredView.SortDescriptions.Clear();

                switch (sortField)
                {
                    case "Title":
                        _filteredView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
                        break;
                    case "Author":
                        _filteredView.SortDescriptions.Add(new SortDescription("Author", ListSortDirection.Ascending));
                        break;
                    case "YearOfPublication":
                        _filteredView.SortDescriptions.Add(new SortDescription("YearOfPublication", ListSortDirection.Ascending));
                        break;
                    case "Price":
                        _filteredView.SortDescriptions.Add(new SortDescription("Price", ListSortDirection.Ascending));
                        break;
                    case "Publisher":
                        _filteredView.SortDescriptions.Add(new SortDescription("Publisher", ListSortDirection.Ascending));
                        break;
                    default:
                        _filteredView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
                        break;
                }
            }
            _filteredView.Refresh();
        }

        // Обновление представления при изменении фильтров
        private void RefreshView()
        {
            if (_filteredView != null)
            {
                _filteredView.Refresh();
            }
        }

        // Обработчики событий фильтрации и сортировки
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => RefreshView();
        private void GenreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshView();
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplySort();

        // Добавление новой книги
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newBook = new Books
                {
                    Title = "Новая книга",
                    Author = "Автор",
                    Publisher = "Издательство",
                    YearOfPublication = DateTime.Now.Year,
                    Price = 0,
                    GenreId = _genresList.FirstOrDefault()?.Id ?? 1
                };
                _context.Books.Add(newBook);
                _context.SaveChanges();
                RefreshView();
                BooksDataGrid.ScrollIntoView(newBook);
                BooksDataGrid.SelectedItem = newBook;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Удаление выбранной книги
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is Books selectedBook)
            {
                if (MessageBox.Show($"Удалить книгу \"{selectedBook.Title}\"?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Books.Remove(selectedBook);
                        _context.SaveChanges();
                        RefreshView();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите книгу для удаления.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Сохранение изменений в базе
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Данные успешно сохранены.", "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                RefreshView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Закрытие окна
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _context?.Dispose();
        }
    }
}