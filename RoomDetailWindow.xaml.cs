using System;
using System.Windows;

namespace HotelApp
{
    public partial class RoomDetailWindow : Window
    {
        public RoomDetailWindow()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Implement saving logic
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DeleteReservation_Click(object sender, RoutedEventArgs e)
        {
            // Implement delete logic
        }
    }
}