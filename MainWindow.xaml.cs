using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HotelApp
{
    public partial class MainWindow : Window
    {
        public Hotel Hotel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            string filePath = "hotel_data.json";
            Hotel = Hotel.LoadData(filePath);

            // Перевірка на випадок некоректної кількості
            if (Hotel.Rooms.Count != 60)
            {
                Hotel.ClearAll();
                Hotel = new Hotel(); // Створити новий готель з коректною кількістю номерів
                Hotel.SaveData(filePath);
            }

            DataContext = Hotel;
            RefreshUI();
        }

        private void Room_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var room = button.DataContext as Room;
            if (room != null)
            {
                RoomDetailWindow roomDetailWindow = new RoomDetailWindow(Hotel, room);
                roomDetailWindow.ShowDialog();
                RefreshUI();
            }
        }

        private void AdvanceTime(TimeSpan duration)
        {
            Hotel.AdvanceTime(duration);
            RefreshUI();
        }

        private void Advance12Hours(object sender, RoutedEventArgs e)
        {
            AdvanceTime(TimeSpan.FromHours(12));
        }

        private void Advance1Day(object sender, RoutedEventArgs e)
        {
            AdvanceTime(TimeSpan.FromDays(1));
        }

        private void Advance3Days(object sender, RoutedEventArgs e)
        {
            AdvanceTime(TimeSpan.FromDays(3));
        }

        private void RefreshUI()
        {
            foreach (var room in Hotel.Rooms)
            {
                room.IsOccupied = Hotel.Reservations.Any(r => r.Room == room && r.IsActiveOn(Hotel.CurrentTime));
            }

            RoomsControl.Items.Refresh();
            Hotel.SaveData("hotel_data.json");
            OccupiedCountTextBlock.Text = $"Кількість зайнятих номерів: {Hotel.Rooms.Count(r => r.IsOccupied)}";
            FreeCountTextBlock.Text = $"Кількість вільних номерів: {Hotel.Rooms.Count(r => !r.IsOccupied)}";
        }
    }
}