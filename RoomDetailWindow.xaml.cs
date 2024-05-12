using System;
using System.Linq;
using System.Windows;

namespace HotelApp
{
    public partial class RoomDetailWindow : Window
    {
        private Hotel _hotel;
        private Room _room;
        private Reservation _currentReservation;

        public RoomDetailWindow(Hotel hotel, Room room)
        {
            InitializeComponent();
            _hotel = hotel;
            _room = room;
            UpdateReservationList();
            UpdatePrice();
        }

        private int GetRoomPricePerDay()
        {
            switch (_room.Type)
            {
                case RoomType.Standard:
                    return 100;
                case RoomType.JuniorSuite:
                    return 135;
                case RoomType.Suite:
                    return 175;
                default:
                    return 0;
            }
        }

        private void UpdatePrice()
        {
            DateTime? checkInDate = CheckInDatePicker.SelectedDate;
            DateTime? checkOutDate = CheckOutDatePicker.SelectedDate;

            if (checkInDate.HasValue && checkOutDate.HasValue && checkInDate.Value < checkOutDate.Value)
            {
                int totalDays = (checkOutDate.Value - checkInDate.Value).Days;
                int pricePerDay = GetRoomPricePerDay();
                PriceTextBlock.Text = $"Ціна: ${pricePerDay * totalDays}";
            }
            else
            {
                PriceTextBlock.Text = $"Ціна: ${GetRoomPricePerDay()}";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string guestName = GuestName.Text;
            DateTime? checkInDate = CheckInDatePicker.SelectedDate;
            DateTime? checkOutDate = CheckOutDatePicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(guestName) || !checkInDate.HasValue || !checkOutDate.HasValue || checkInDate.Value >= checkOutDate.Value || checkInDate.Value < _hotel.CurrentTime.Date)
            {
                MessageBox.Show("Неправильні дані. Перевірте, будь ласка, введені значення.");
                return;
            }

            DateTime checkInDateTime = checkInDate.Value.Date.AddHours(12);
            DateTime checkOutDateTime = checkOutDate.Value.Date.AddHours(12);

            // Перевірка на наявність конфліктів з іншими бронюваннями
            bool hasConflict = _hotel.Reservations.Any(r =>
                r.Room == _room &&
                !(checkOutDateTime <= r.CheckInDate || checkInDateTime >= r.CheckOutDate)
            );

            if (hasConflict)
            {
                MessageBox.Show("Цей номер вже зайнятий в обраний час. Перевірте, будь ласка, введені значення.");
                return;
            }

            if (_currentReservation == null)
            {
                _hotel.Reservations.Add(new Reservation
                {
                    GuestName = guestName,
                    CheckInDate = checkInDateTime,
                    CheckOutDate = checkOutDateTime,
                    Room = _room
                });
            }
            else
            {
                _currentReservation.GuestName = guestName;
                _currentReservation.CheckInDate = checkInDateTime;
                _currentReservation.CheckOutDate = checkOutDateTime;
            }

            UpdateReservationList();
            _hotel.SaveData("hotel_data.json");
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UpdateReservationList()
        {
            ReservationsList.ItemsSource = _hotel.Reservations.Where(r => r.Room == _room).ToList();
            ReservationsList.Items.Refresh();
        }

        private void DeleteReservation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            var reservation = button.DataContext as Reservation;

            if (reservation != null)
            {
                _hotel.Reservations.Remove(reservation);
                UpdateReservationList();
                _hotel.SaveData("hotel_data.json");
            }
        }

        private void ReservationsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _currentReservation = ReservationsList.SelectedItem as Reservation;

            if (_currentReservation != null)
            {
                GuestName.Text = _currentReservation.GuestName;
                CheckInDatePicker.SelectedDate = _currentReservation.CheckInDate.Date;
                CheckOutDatePicker.SelectedDate = _currentReservation.CheckOutDate.Date;
                UpdatePrice();
            }
        }

        private void CheckInDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdatePrice();
        }

        private void CheckOutDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdatePrice();
        }
    }
}