using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;


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

        // Свойство, которое возвращает текст для отображения
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

        // Check if the reservation is active on a specific date
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
            for (int i = 1; i <= 40; i++)
                Rooms.Add(new Room { Number = i, Type = RoomType.Standard, IsOccupied = false });
            for (int i = 41; i <= 50; i++)
                Rooms.Add(new Room { Number = i, Type = RoomType.JuniorSuite, IsOccupied = false });
            for (int i = 51; i <= 60; i++)
                Rooms.Add(new Room { Number = i, Type = RoomType.Suite, IsOccupied = false });
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
            string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static Hotel LoadData(string filePath)
        {
            if (!File.Exists(filePath)) return new Hotel();
            string json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<Hotel>(json);
            return data ?? new Hotel();
        }
    }
}
