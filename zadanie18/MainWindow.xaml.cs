using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using zadanie18.Models;

namespace zadanie18
{
    public partial class MainWindow : Window
    {
        private TEST1Entities _context;
        private List<dynamic> _allBooks;

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            _context = new TEST1Entities();
            var books = _context.Books.Include("Genres").ToList();
            _allBooks = books.Select(b => new
            {
                b.Id,
                b.Title,
                b.Author,
                b.Publisher,
                b.YearOfPublication,
                b.Price,
                GenreName = b.Genres?.Name ?? "Без жанра"
            }).ToList<dynamic>();

            // Заполняем ComboBox жанров
            var genres = _context.Genres.ToList();
            GenreComboBox.Items.Clear();
            GenreComboBox.Items.Add("Все");
            foreach (var g in genres)
                GenreComboBox.Items.Add(g.Name);
            GenreComboBox.SelectedIndex = 0;

            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            if (_allBooks == null) return;

            // Поиск
            string search = SearchTextBox?.Text?.Trim().ToLower();
            var filtered = _allBooks.AsEnumerable();
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(b =>
                    b.Title.ToLower().Contains(search) ||
                    b.Author.ToLower().Contains(search));
            }

            // Фильтр по жанру (с проверкой на null)
            if (GenreComboBox.SelectedItem != null && GenreComboBox.SelectedItem.ToString() != "Все")
            {
                string selectedGenre = GenreComboBox.SelectedItem.ToString();
                filtered = filtered.Where(b => b.GenreName == selectedGenre);
            }

            // Сортировка
            if (SortComboBox.SelectedItem is ComboBoxItem selectedSort)
            {
                string field = selectedSort.Tag.ToString();
                switch (field)
                {
                    case "Title": filtered = filtered.OrderBy(b => b.Title); break;
                    case "Author": filtered = filtered.OrderBy(b => b.Author); break;
                    case "YearOfPublication": filtered = filtered.OrderBy(b => b.YearOfPublication); break;
                    case "Price": filtered = filtered.OrderBy(b => b.Price); break;
                    case "Publisher": filtered = filtered.OrderBy(b => b.Publisher); break;
                    default: filtered = filtered.OrderBy(b => b.Title); break;
                }
            }

            BooksDataGrid.ItemsSource = filtered.ToList();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFiltersAndSort();
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFiltersAndSort();
        private void GenreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFiltersAndSort();
    }
}