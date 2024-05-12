using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HotelApp
{
    public partial class MainWindow : Window
    {
        // Властивість для зберігання даних готелю
        public Hotel Hotel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            string filePath = "hotel_data.json"; // Шлях до файлу з даними готелю

            // Спроба завантажити дані про готель
            try
            {
                Hotel = Hotel.LoadData(filePath);

                // Перевірка на коректність кількості кімнат
                if (Hotel.Rooms.Count != 60)
                {
                    ErrorMessageBox("Некоректна кількість кімнат у готелі.");
                    Hotel.ClearAll();
                    Hotel = new Hotel(); // Створити новий готель з коректною кількістю кімнат
                    Hotel.SaveData(filePath);
                }
            }
            catch (FileNotFoundException)
            {
                ErrorMessageBox($"Файл {filePath} не знайдено.");
                Hotel = new Hotel(); // Створити новий готель, якщо файл відсутній
            }
            catch (Exception ex)
            {
                ErrorMessageBox($"Помилка при завантаженні даних: {ex.Message}");
                Hotel = new Hotel(); // Створити новий готель у разі виникнення помилки
            }

            DataContext = Hotel;
            RefreshUI();
        }

        private void Room_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Room room)
            {
                try
                {
                    RoomDetailWindow roomDetailWindow = new RoomDetailWindow(Hotel, room);
                    roomDetailWindow.ShowDialog();
                    RefreshUI();
                }
                catch (Exception ex)
                {
                    ErrorMessageBox($"Помилка при роботі з деталями кімнати: {ex.Message}");
                }
            }
            else
            {
                ErrorMessageBox("Помилка при отриманні даних кімнати.");
            }
        }

        private void AdvanceTime(TimeSpan duration)
        {
            try
            {
                Hotel.AdvanceTime(duration);
                RefreshUI();
            }
            catch (Exception ex)
            {
                ErrorMessageBox($"Помилка при зміні часу: {ex.Message}");
            }
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
            try
            {
                // Оновлення статусу кімнат в готелі
                foreach (var room in Hotel.Rooms)
                {
                    room.IsOccupied = Hotel.Reservations.Any(r => r.Room == room && r.IsActiveOn(Hotel.CurrentTime));
                }

                RoomsControl.Items.Refresh();
                Hotel.SaveData("hotel_data.json");
                OccupiedCountTextBlock.Text = $"Кількість зайнятих номерів: {Hotel.Rooms.Count(r => r.IsOccupied)}";
                FreeCountTextBlock.Text = $"Кількість вільних номерів: {Hotel.Rooms.Count(r => !r.IsOccupied)}";
            }
            catch (Exception ex)
            {
                ErrorMessageBox($"Помилка при оновленні інтерфейсу: {ex.Message}");
            }
        }

        // Метод для виведення повідомлень про помилки
        private void ErrorMessageBox(string message)
        {
            MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}