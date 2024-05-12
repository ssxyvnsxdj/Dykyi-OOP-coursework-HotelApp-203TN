using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq; // Додано простір імен для LINQ
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Newtonsoft.Json;

namespace HotelApp
{
    public enum RoomType
    {
        Standard,
        JuniorSuite,
        Suite
    }

    public class Room
    {
        public int Number { get; set; }
        public RoomType Type { get; set; }
        public bool IsOccupied { get; set; }

        // Властивість, яка повертає текст для відображення
        public string DisplayText
        {
            get
            {
                string suffix;
                switch (Type)
                {
                    case RoomType.Standard:
                        suffix = "С"; // Стандарт
                        break;
                    case RoomType.JuniorSuite:
                        suffix = "Л"; // Полулюкс
                        break;
                    case RoomType.Suite:
                        suffix = "Л+"; // Люкс
                        break;
                    default:
                        suffix = "";
                        break;
                }
                return $"{Number}{suffix}";
            }
        }
    }

    public class Reservation
    {
        public string GuestName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public Room Room { get; set; }

        // Перевірка, чи активне бронювання на конкретну дату
        public bool IsActiveOn(DateTime date)
        {
            return CheckInDate <= date && CheckOutDate > date;
        }
    }

    public class OccupiedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isOccupied)
            {
                return isOccupied ? Brushes.Red : Brushes.Green;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Hotel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private DateTime _currentTime;

        public List<Room> Rooms { get; set; }
        public List<Reservation> Reservations { get; set; }

        public DateTime CurrentTime
        {
            get { return _currentTime; }
            set
            {
                if (_currentTime != value)
                {
                    _currentTime = value;
                    OnPropertyChanged(nameof(CurrentTime));
                }
            }
        }

        public Hotel()
        {
            Rooms = new List<Room>();
            Reservations = new List<Reservation>();
            _currentTime = new DateTime(2024, 1, 1, 0, 0, 0);
            InitializeRooms();
        }

        // Ініціалізація номерів готелю
        private void InitializeRooms()
        {
            if (Rooms.Count == 0)
            {
                for (int i = 1; i <= 40; i++)
                    Rooms.Add(new Room { Number = i, Type = RoomType.Standard, IsOccupied = false });
                for (int i = 41; i <= 50; i++)
                    Rooms.Add(new Room { Number = i, Type = RoomType.JuniorSuite, IsOccupied = false });
                for (int i = 51; i <= 60; i++)
                    Rooms.Add(new Room { Number = i, Type = RoomType.Suite, IsOccupied = false });
            }
        }

        // Очищення всіх даних готелю
        public void ClearAll()
        {
            Rooms.Clear();
            Reservations.Clear();
        }

        // Переміщення часу вперед
        public void AdvanceTime(TimeSpan duration)
        {
            CurrentTime = CurrentTime.Add(duration);
        }

        // Виклик події зміни властивості
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Збереження даних готелю у файл
        public void SaveData(string filePath)
        {
            try
            {
                var data = new
                {
                    Rooms = this.Rooms,
                    Reservations = this.Reservations,
                    CurrentTime = this.CurrentTime
                };
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                ErrorMessageBox($"Помилка при збереженні даних: {ex.Message}");
            }
        }

        // Завантаження даних готелю з файлу
        public static Hotel LoadData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ErrorMessageBox("Файл з даними не знайдено. Буде створено новий готель.");
                return new Hotel();
            }
            try
            {
                string json = File.ReadAllText(filePath);
                dynamic data = JsonConvert.DeserializeObject<dynamic>(json);

                var hotel = new Hotel
                {
                    Rooms = JsonConvert.DeserializeObject<List<Room>>(data.Rooms.ToString()),
                    Reservations = JsonConvert.DeserializeObject<List<Reservation>>(data.Reservations.ToString()),
                    _currentTime = DateTime.Parse(data.CurrentTime.ToString())
                };

                // Відновлення посилання на кімнату в кожному бронюванні
                foreach (var reservation in hotel.Reservations)
                {
                    var room = hotel.Rooms.FirstOrDefault(r => r.Number == reservation.Room.Number);
                    if (room != null)
                    {
                        reservation.Room = room;
                    }
                    else
                    {
                        ErrorMessageBox("Неправильні дані бронювання. Деякі бронювання містять помилки.");
                    }
                }

                return hotel;
            }
            catch (Exception ex)
            {
                ErrorMessageBox($"Помилка при завантаженні даних: {ex.Message}");
                return new Hotel(); // Якщо дані некоректні, повертаємо новий готель
            }
        }

        // Метод для виведення повідомлень про помилки
        private static void ErrorMessageBox(string message)
        {
            MessageBox.Show(message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}