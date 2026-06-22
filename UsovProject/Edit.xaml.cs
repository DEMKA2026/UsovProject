using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UsovProject
{
    /// <summary>
    /// Логика взаимодействия для Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        private Entity _db;
        private Tovar _product;
        public Edit(Tovar product)
        {
            InitializeComponent();

            _db = new Entity();
            _product = product;

            LoadCombos();
            LoadData();
        }

        private void LoadCombos()
        {
            cbCategory.ItemsSource = _db.TovarCategory.ToList();
            cbCategory.DisplayMemberPath = "TovarCategoryName";
            cbCategory.SelectedValuePath = "IDTovarCategory";

            cbManufacturer.ItemsSource = _db.Proizvoditeli.ToList();
            cbManufacturer.DisplayMemberPath = "NameProizvoditelia";
            cbManufacturer.SelectedValuePath = "IDProizvoditelia";

            cbSupplier.ItemsSource = _db.Postavshik.ToList();
            cbSupplier.DisplayMemberPath = "PostavshikName";
            cbSupplier.SelectedValuePath = "IDPostavshik";

            cbUnit.ItemsSource = _db.EdIzmer.ToList();
            cbUnit.DisplayMemberPath = "Edzm";
            cbUnit.SelectedValuePath = "IDEdIzm";
        }

        private void LoadData()
        {
            tbId.Text = _product.IDTovar.ToString();
            tbName.Text = _product.TovarName;
            tbArticle.Text = _product.Article;
            tbDescription.Text = _product.Description;
            tbCost.Text = _product.Cost.ToString();
            tbStock.Text = _product.KolvoNaSklade.ToString();
            tbDiscount.Text = _product.Discount.ToString();

            cbCategory.SelectedValue = _product.IDTovarCategory;
            cbManufacturer.SelectedValue = _product.IDProizvoditelia;
            cbSupplier.SelectedValue = _product.IDPostavshik;
            cbUnit.SelectedValue = _product.IDEdIzm;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
                return;

            try
            {
                ApplyChanges();
                _db.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException != null
                    ? ex.InnerException.Message
                    : ex.Message);
            }
        }

        private bool Validate()
        {
            decimal price;
            int stock;
            int discount;

            if (!decimal.TryParse(tbCost.Text, out price) ||
                !int.TryParse(tbStock.Text, out stock) ||
                !int.TryParse(tbDiscount.Text, out discount))
            {
                MessageBox.Show("Цена, количество и скидка должны быть числами");
                return false;
            }

            if (price < 0 || stock < 0 || discount < 0)
            {
                MessageBox.Show("Значения не могут быть отрицательными");
                return false;
            }

            if (cbCategory.SelectedValue == null ||
                cbManufacturer.SelectedValue == null ||
                cbSupplier.SelectedValue == null ||
                cbUnit.SelectedValue == null)
            {
                MessageBox.Show("Не все списки заполнены");
                return false;
            }

            if (string.IsNullOrWhiteSpace(tbArticle.Text) || tbArticle.Text.Length != 6)
            {
                MessageBox.Show("Артикул должен содержать 6 символов");
                return false;
            }

            return true;
        }

        private void ApplyChanges()
        {
            if (_product.IDTovar == 0)
            {
                int newId = 1;

                if (_db.Tovar.Any())
                    newId = _db.Tovar.Max(x => x.IDTovar) + 1;

                _product.IDTovar = newId;
                _db.Tovar.Add(_product);
            }

            _product.TovarName = tbName.Text;
            _product.Description = tbDescription.Text;
            _product.Article = tbArticle.Text;

            _product.Cost = Convert.ToDecimal(tbCost.Text);
            _product.KolvoNaSklade = Convert.ToInt32(tbStock.Text);
            _product.Discount = Convert.ToInt32(tbDiscount.Text);

            _product.IDTovarCategory = (int)cbCategory.SelectedValue;
            _product.IDProizvoditelia = (int)cbManufacturer.SelectedValue;
            _product.IDPostavshik = (int)cbSupplier.SelectedValue;
            _product.IDEdIzm = (int)cbUnit.SelectedValue;
        }
    }
}
