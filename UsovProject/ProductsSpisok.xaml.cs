using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace UsovProject
{
    public partial class ProductsSpisok : Window
    {
        private Entity _context = new Entity();
        private ObservableCollection<ProductView> _items = new ObservableCollection<ProductView>();
        private ICollectionView _view;

        private User _user;
        private Edit _editor;

        private string _searchText = "";
        private string _discountFilter = "Все";

        private bool _isGuest;
        private bool _isClient;
        private bool _isAdmin;
        private bool _isManager;

        public ProductsSpisok(User user)
        {
            InitializeComponent();

            _user = user;

            DetectRole();
            SetUserInfo();
            SetPermissions();

            _view = CollectionViewSource.GetDefaultView(_items);
            _view.Filter = Filter;
             
            dgProducts.ItemsSource = _view;

            cbDiscount.SelectedIndex = 0;

            LoadData();
        }

        private void DetectRole()
        {
            _isGuest = (_user == null);

            if (_user != null && _user.Role != null)
            {
                if (_user.Role.RoleName == "Авторизированный клиент")
                    _isClient = true;

                if (_user.Role.RoleName == "Администратор")
                    _isAdmin = true;

                if (_user.Role.RoleName == "Менеджер")
                    _isManager = true;
            }
        }

        private void SetUserInfo()
        {
            if (_user == null)
            {
                tbUser.Text = "Гость";
                tbRole.Text = "Неавторизованный пользователь";
            }
            else
            {
                tbUser.Text = _user.Familia + " " + _user.Imya + " " + _user.Otchestvo;
                tbRole.Text = _user.Role.RoleName;
            }
        }

        private void SetPermissions()
        {
            bool canFilter = (_isAdmin || _isManager);

            tbSearch.Visibility = canFilter ? Visibility.Visible : Visibility.Collapsed;
           
            cbDiscount.Visibility = canFilter ? Visibility.Visible : Visibility.Collapsed;
             

            btnAdd.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
            btnDelete.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadData()
        {
            _items.Clear();

            foreach (var p in _context.Tovar.ToList())
            {
                decimal cost = p.Cost;
                int discount = p.Discount ?? 0;
                int stock = p.KolvoNaSklade ?? 0;

                decimal finalPrice = cost - (cost * discount / 100);

                Brush rowBrush = Brushes.Transparent;

                if (stock == 0)
                    rowBrush = Brushes.LightGray;
                else if (discount > 15)
                    rowBrush = (Brush)new BrushConverter().ConvertFrom("#008080");

                _items.Add(new ProductView
                {
                    Id = p.IDTovar,
                    TovarName = p.TovarName,
                    Description = p.Description,
                    CategoryName = p.TovarCategory != null ? p.TovarCategory.TovarCategoryName : "",
                    Manufacturer = p.Proizvoditeli != null ? p.Proizvoditeli.NameProizvoditelia : "",
                    Supplier = p.Postavshik != null ? p.Postavshik.PostavshikName : "",
                    Cost = cost,
                    FinalPrice = finalPrice,
                    Discount = discount,
                    Unit = p.EdIzmer != null ? p.EdIzmer.Edzm : "",
                    Stock = stock,
        
                    RowBrush = rowBrush
                });
            }

            if (_view != null)
                _view.Refresh();
        }

        private bool Filter(object obj)
        {
            ProductView p = obj as ProductView;

            if (p == null)
                return false;

            bool searchOk = true;

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                searchOk =
                    (p.TovarName != null && p.TovarName.ToLower().Contains(_searchText)) ||
                    (p.Description != null && p.Description.ToLower().Contains(_searchText)) ||
                    (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(_searchText)) ||
                    (p.Supplier != null && p.Supplier.ToLower().Contains(_searchText));
            }

            bool discountOk = true;

            if (_discountFilter == "0-11.99")
                discountOk = p.Discount < 12;
            else if (_discountFilter == "12-18.99")
                discountOk = p.Discount >= 12 && p.Discount < 19;
            else if (_discountFilter == "19+")
                discountOk = p.Discount >= 19;

            return searchOk && discountOk;
        }

        private void ApplyFilter()
        {
            if (_view != null)
                _view.Refresh();
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = tbSearch.Text.Trim().ToLower();
            ApplyFilter();
        }

        private void cbDiscount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = cbDiscount.SelectedItem as ComboBoxItem;

            if (item == null)
                return;

            _discountFilter = item.Content != null ? item.Content.ToString() : "Все";
            ApplyFilter();
        }

      
       
        private void dgProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProductView selected = dgProducts.SelectedItem as ProductView;

            if (selected == null)
                return;

            if (!_isAdmin)
            {
                MessageBox.Show("Редактирование доступно только администратору");
                return;
            }

            var entity = _context.Tovar.FirstOrDefault(x => x.IDTovar == selected.Id);

            if (entity == null)
                return;

            _editor = new Edit(entity);

            if (_editor.ShowDialog() == true)
            {
                _context.SaveChanges();
                LoadData();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Tovar t = new Tovar();
            Edit w = new Edit(t);

            if (w.ShowDialog() == true)
            {
                _context.SaveChanges();
                LoadData();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            ProductView selected = dgProducts.SelectedItem as ProductView;

            if (selected == null)
                return;

            var entity = _context.Tovar.FirstOrDefault(x => x.IDTovar == selected.Id);

            if (entity == null)
                return;

            if (_context.Zakaz.Any(x => x.IDTovar == entity.IDTovar))
            {
                MessageBox.Show("Нельзя удалить товар, он есть в заказах");
                return;
            }

            if (MessageBox.Show("Удалить товар?", "Удаление", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            _context.Tovar.Remove(entity);
            _context.SaveChanges();

            LoadData();
        }

        

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }

        public class ProductView
        {
            public int Id { get; set; }
            public string CategoryName { get; set; }
            public string TovarName { get; set; }
            public string Description { get; set; }
            public string Manufacturer { get; set; }
            public string Supplier { get; set; }
            public decimal Cost { get; set; }
            public decimal FinalPrice { get; set; }
            public int Discount { get; set; }
            public string Unit { get; set; }
            public int Stock { get; set; }
            public string ImagePath { get; set; }
            public Brush RowBrush { get; set; }
        }
    }
}