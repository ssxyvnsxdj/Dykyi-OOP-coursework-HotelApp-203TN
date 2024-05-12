using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq; // Додано простір імен для LINQ
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
                        suffix = "С";
                        break;
                    case RoomType.JuniorSuite:
                        suffix = "Л";
                        break;
                    case RoomType.Suite:
                        suffix = "Л+";
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

        public void ClearAll()
        {
            Rooms.Clear();
            Reservations.Clear();
        }

        public void AdvanceTime(TimeSpan duration)
        {
            CurrentTime = CurrentTime.Add(duration);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SaveData(string filePath)
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

        public static Hotel LoadData(string filePath)
        {
            if (!File.Exists(filePath)) return new Hotel();
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

                // Відновимо посилання на номер у кожному бронюванні
                foreach (var reservation in hotel.Reservations)
                {
                    var room = hotel.Rooms.FirstOrDefault(r => r.Number == reservation.Room.Number);
                    if (room != null)
                    {
                        reservation.Room = room;
                    }
                }

                return hotel ?? new Hotel();
            }
            catch
            {
                return new Hotel(); // Якщо дані некоректні, повертаємо новий готель
            }
        }
    }
}