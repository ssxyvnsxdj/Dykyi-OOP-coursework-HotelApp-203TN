using System;
using System.Windows;

namespace HotelApp
{
    public partial class MainWindow : Window
    {
        public Hotel Hotel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Hotel = Hotel.LoadData("hotel_data.json");
            DataContext = Hotel;
        }

        private void Room_Click(object sender, RoutedEventArgs e)
        {
            RoomDetailWindow roomDetailWindow = new RoomDetailWindow();
            roomDetailWindow.Show();
        }

        private void Advance12Hours(object sender, RoutedEventArgs e)
        {
            Hotel.AdvanceTime(TimeSpan.FromHours(12));
            RefreshUI();
        }

        private void Advance1Day(object sender, RoutedEventArgs e)
        {
            Hotel.AdvanceTime(TimeSpan.FromDays(1));
            RefreshUI();
        }

        private void Advance3Days(object sender, RoutedEventArgs e)
        {
            Hotel.AdvanceTime(TimeSpan.FromDays(3));
            RefreshUI();
        }
        // тест 2 // тесттттт
        // New commit
        private void RefreshUI()
        {
            RoomsControl.Items.Refresh();
            Hotel.SaveData("hotel_data.json");
        }

        private void Vasya ()
        {
            RoomsControl.Items.Refresh();
        }
    }
}