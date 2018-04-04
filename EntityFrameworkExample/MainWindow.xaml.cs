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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EntityFrameworkExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DBMethods dBMethods = new DBMethods();
        List<Item> Items = new List<Item>();
        List<Order> OrdersToAdd = new List<Order>(); //contains orders not yet added to db.
        //List<Order> Orders = new List<Order>();

        public MainWindow()
        {
            InitializeComponent();

            FillComboBox();
            FillDataGrid();
            // --- example on how to create a and save an item
            //Item item = new Item()
            //{
            //    Name = "Magnum",
            //    UnitPrice = 16.40m,
            //    Description = "Den klassiske flødeis"
            //};

            //context.ItemSet.Add(item);
            //context.SaveChanges();
            // ---- end
        }

        public void FillDataGrid()
        {
            //if (Orders.Any()) Orders.Clear();
            //Orders = DBMethods.getAllOrders(context);

            dgOrders.ItemsSource = OrdersToAdd;
            //dgOrders.ItemsSource = dBMethods.OrdersToAdd;
        }

        public void FillComboBox()
        {
            if (Items.Any()) Items.Clear();
            Items = dBMethods.getAllItems();

            cboSelectIceCream.ItemsSource = Items;
            cboSelectIceCream.SelectedIndex = 0;
        }

        public void RefreshCboSelectIceCream()
        {
            cboSelectIceCream.Items.Refresh();
        }

        public void RefreshDgOrders()
        {
            //dgOrders.ItemsSource = dBMethods
            dgOrders.Items.Refresh();
        }

        private void cboSelectIceCream_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Item selectedItem = cboSelectIceCream.SelectedItem as Item;
            lblPrice.Content = selectedItem.UnitPrice;
            tboDescription.Text = selectedItem.Description;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            int quantity;
            if (cboSelectIceCream.SelectedItem != null)
            {
                if (int.TryParse(txtQuantity.Text, out quantity) && quantity > 0)
                {
                    Order tmpOrder = new Order()
                    {
                        Quantity = quantity,
                        Item = cboSelectIceCream.SelectedItem as Item,
                    };
                    OrdersToAdd.Add(tmpOrder);
                    //context.OrderSet.Add(tmpOrder);
                    RefreshDgOrders();
                    txtQuantity.Text = "";
                }
                else MessageBox.Show("Ugyldigt antal indtastet", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else MessageBox.Show("Ingen vare valgt", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            int quantity;
            Item selectedItem = cboSelectIceCream.SelectedItem as Item;
            if (int.TryParse(txtQuantity.Text, out quantity) && quantity > 0 && selectedItem != null) lblTotalPrice.Content = selectedItem.UnitPrice * quantity;
            else lblTotalPrice.Content = 0;
        }

        private void btnPlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            string sOrders = "";
            foreach (Order o in OrdersToAdd)
            {
                sOrders += "-" + o.ToString() + "\n";
            }

            MessageBoxResult dialogResult = MessageBox.Show($"Er du sikker på at du vil bestille: \n{sOrders}", "Bekræft bestilling", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dialogResult == MessageBoxResult.Yes)
            {
                dBMethods.addOrders(OrdersToAdd);
                OrdersToAdd.Clear();
                RefreshDgOrders();
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            // if nothing is selected in the datagrid. the method returns
            if (dgOrders.SelectedItem != null)
            {
                Order tmpOrder = dgOrders.SelectedItem as Order;
                if (OrdersToAdd.Exists(Order => Order == tmpOrder))
                {
                    MessageBoxResult boxResult = MessageBox.Show($"Er du sikker på at du vil fjerne ordren: \n {tmpOrder.ToString()}", "Vil du fjerne ordren?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (boxResult == MessageBoxResult.Yes)
                    {
                        OrdersToAdd.Remove(tmpOrder);
                        RefreshDgOrders();
                    }
                    else return;
                }
                else MessageBox.Show("Denne ordre er allerede bestilt. den kan derfor ikke fjernes", "Ordre kan ikke fjerenes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("Ingen ordre valgt", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Din nuværende bestilling vil ikke blive gemt. Vil du fortsætte?", "Nuværende bestilling vil ikke blive gemt", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                btnShowAll.IsEnabled = false;
                btnClearAll.IsEnabled = true;
                btnAdd.IsEnabled = false;
                btnRemove.IsEnabled = false;
                btnPlaceOrder.IsEnabled = false;

                dgOrders.ItemsSource = null;
                if (dgOrders.Items.Count > 0) dgOrders.Items.Clear();
                dgOrders.ItemsSource = dBMethods.getAllOrders();
            }
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            btnShowAll.IsEnabled = true;
            btnClearAll.IsEnabled = false;
            btnAdd.IsEnabled = true;
            btnRemove.IsEnabled = true;
            btnPlaceOrder.IsEnabled = true;

            dgOrders.ItemsSource = null;
            dgOrders.Items.Clear();
            FillDataGrid();
        }

        private void btnShowOverview_Click(object sender, RoutedEventArgs e)
        {
            dgOrderOverview.ItemsSource = dBMethods.CreateOverviewList();
        }

        private void btnShowOverviewLinq_Click(object sender, RoutedEventArgs e)
        {
            dgOrderOverview.ItemsSource = dBMethods.createOverviewListLinq();
        }
    }

    public class DBMethods
    {
        EntityModelContainer context = new EntityModelContainer();

        public List<Item> getAllItems()
        {
            if (context.ItemSet.Count() > 0)
            {
                return context.ItemSet.ToList();

            }
            else
            {
                return new List<Item>();
            }
        }

        public List<Order> getAllOrders()
        {
            if (context.OrderSet.Count() > 0)
            {
                return context.OrderSet.ToList();
            }
            else
            {
                return new List<Order>();
            }
        }

        public void AddItems(List<Item> items)
        {
            try
            {
                foreach (Item item in items)
                {
                    context.ItemSet.Add(item);
                    context.SaveChanges();
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void addOrders(List<Order> orders)
        {
            try
            {
                foreach (Order order in orders)
                {
                    context.OrderSet.Add(order);
                    context.SaveChanges();
                }
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Det skete en fejl da ordren(e) blev gemt i databasen:\n {ex.Message}", "Database fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<IOrderOverview> CreateOverviewList()
        {
            List<IOrderOverview> orderOverview = new List<IOrderOverview>();

            foreach (Item item in getAllItems())
            {
                orderOverview.Add(item);
                foreach (Order order in item.Order)
                {
                    orderOverview.Add(order);
                }
            }

            return orderOverview;
        }

        public List<IOrderOverview> createOverviewListLinq()
        {
            //return (from item in getAllItems()
            //         where item.Quantity > 100
            //         select item as IOrderOverview).ToList();

            return (from order in getAllOrders() where order.Quantity > 56 select order.Item as IOrderOverview).Distinct().ToList();

        }
    }

    public partial class Item : IOrderOverview
    {
        public string OverviewUnitPrice { get { return UnitPrice.ToString(); } }
        //public int Quantity { get { if (this.Order.Count > 0) return this.Order.Sum(Order => Order.Quantity); else return 0; } }
        public int Quantity
        {
            get
            {
                int sum = 0;
                foreach (Order order in this.Order)
                {
                    sum += order.Quantity;
                }
                return sum;
            }
        }
        public decimal Total { get { if (this.Order.Count > 0) return this.Order.Sum(Order => Order.TotalPrice); else return 0; } }
        public override string ToString()
        {
            return Name.ToString();
        }

        public Item(int id, string name, decimal unitPrice, string description)
        {
            this.Id = id;
            this.Name = name;
            this.UnitPrice = unitPrice;
            this.Description = description;
        }

        /// <summary>
        /// Constructor used to create an item that contains a list of orders
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="unitPrice"></param>
        /// <param name="description"></param>
        /// <param name="orders">Contains all orders of this item</param>
        public Item(int id, string name, decimal unitPrice, string description, ICollection<Order> orders)
        {
            this.Id = id;
            this.Name = name;
            this.UnitPrice = unitPrice;
            this.Description = description;
            this.Order = orders;
        }
    }

    public partial class Order : IOrderOverview
    {
        public decimal TotalPrice { get { return Convert.ToDecimal(this.Item.UnitPrice) * Convert.ToDecimal(this.Quantity); } }
        public string Name { get { return ""; } }
        public string OverviewUnitPrice { get { return ""; } }
        public decimal Total { get { return TotalPrice; } }

        //empty constructor needed by the EntityModel.Context
        //public Order()
        //{

        //}

        //public Order(int quantity, Item item)
        //{
        //    this.Quantity = quantity;
        //    //this.ItemId = item.Id;
        //    this.Item = item;
        //}

        //public Order(int quantity, Item item, int id)
        //{
        //    this.Quantity = quantity;
        //    //this.ItemId = item.Id;
        //    this.Item = item;
        //    this.Id = id;
        //}

        public override string ToString()
        {
            //                    quantity   name    unitprice     total
            return string.Format("{0} stk. '{1}' af {2}kr.- ialt {3}kr.-", this.Quantity.ToString(), this.Item.Name, this.Item.UnitPrice.ToString(), (this.Item.UnitPrice * Convert.ToInt32(this.Quantity)).ToString());
        }
    }
}
