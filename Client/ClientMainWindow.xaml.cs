using EntityFrameworkExample;
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

namespace MyClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ClientFunc Func = new ClientFunc();
        List<Order> OrdersToAdd = new List<Order>(); //contains orders not yet sent to server. 

        public MainWindow()
        {
            InitializeComponent();

            //Fill combobox and select the first item
            SetcboSelectIceCreamItemSource();
            cboSelectIceCream.SelectedIndex = 0;
            dgOrders.ItemsSource = OrdersToAdd;
        }

        private async void SetcboSelectIceCreamItemSource()
        {
            cboSelectIceCream.ItemsSource = await Func.GetAllItemsFromServer();
        }

        //Update tboDescription and lblPrice to show data about the selected ice cream
        private void cboSelectIceCream_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Item selectedItem = cboSelectIceCream.SelectedItem as Item;
            tboDescription.Text = selectedItem.Description;
            lblPrice.Content = selectedItem.UnitPrice;
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
                    dgOrders.Items.Refresh();
                    txtQuantity.Text = "";
                }
                else MessageBox.Show("Ugyldigt antal indtastet", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else MessageBox.Show("Ingen vare valgt", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        //Updates totalPrice if any ice cream is selected and the quantity is a number greater than 0
        private void txtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            int quantity;
            Item selectedItem = cboSelectIceCream.SelectedItem as Item;
            if (int.TryParse(txtQuantity.Text, out quantity) && quantity > 0 && selectedItem != null) lblTotalPrice.Content = selectedItem.UnitPrice * quantity;
            else lblTotalPrice.Content = 0;
        }

        private async void btnPlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            string sOrders = "";
            foreach (Order o in OrdersToAdd)
            {
                sOrders += "-" + o.ToString() + "\n";
            }

            MessageBoxResult dialogResult = MessageBox.Show($"Er du sikker på at du vil bestille: \n{sOrders}", "Bekræft bestilling", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (dialogResult == MessageBoxResult.Yes)
            {
                try
                {
                    string response = await Func.SendNewOrdersToServer(OrdersToAdd);
                    OrdersToAdd.Clear();
                    dgOrders.Items.Refresh();
                    MessageBox.Show(response,response, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Der opstod en fejl, ordrene blev derfor ikke gemt. Prøv igen./n fejlbesked: {ex.Message}", "Ordrene blev ikke gemt", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem != null)
            {
                Order tmpOrder = dgOrders.SelectedItem as Order;
                if (OrdersToAdd.Exists(Order => Order == tmpOrder))
                {
                    MessageBoxResult boxResult = MessageBox.Show($"Er du sikker på at du vil fjerne ordren: \n {tmpOrder.ToString()}", "Vil du fjerne ordren?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (boxResult == MessageBoxResult.Yes)
                    {
                        OrdersToAdd.Remove(tmpOrder);
                        dgOrders.Items.Refresh();
                    }
                    else return;
                }
                else MessageBox.Show("Denne ordre er allerede bestilt. den kan derfor ikke fjernes", "Ordre kan ikke fjerenes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else MessageBox.Show("Ingen ordre valgt", "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        //Shows all orders in dgOrderOverview
        private async void btnShowOverview_Click(object sender, RoutedEventArgs e)
        {
            //if (dgOrderOverview.Items.Count > 0) { dgOrderOverview.ItemsSource = null; dgOrderOverview.Items.Clear(); }
            dgOrderOverview.ItemsSource = await Func.GetOrdersOverviewFromServer();
        }

        private async void btnShowAllOrders_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Din nuværende bestilling vil ikke blive gemt. Vil du fortsætte?", "Nuværende bestilling vil ikke blive gemt", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                btnShowAllOrders.IsEnabled = false;
                btnClearAll.IsEnabled = true;
                btnAdd.IsEnabled = false;
                btnRemove.IsEnabled = false;
                btnPlaceOrder.IsEnabled = false;

                //dgOrders.ItemsSource = null;
                //if (dgOrders.Items.Count > 0) dgOrders.Items.Clear();
                dgOrders.ItemsSource = await Func.getAllOrdersFromServer();
            }
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            btnShowAllOrders.IsEnabled = true;
            btnClearAll.IsEnabled = false;
            btnAdd.IsEnabled = true;
            btnRemove.IsEnabled = true;
            btnPlaceOrder.IsEnabled = true;

            //dgOrders.ItemsSource = null;
            //dgOrders.Items.Clear();
            dgOrders.ItemsSource = OrdersToAdd;
        }
    }
}
