using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Windows;

namespace HotelApp
{
    public partial class RoomDetailWindow : Window
    {
        private Hotel _hotel;
        private Room _room;
        private Reservation _currentReservation;

        // Конструктор вікна деталей кімнати
        public RoomDetailWindow(Hotel hotel, Room room)
        {
            InitializeComponent();
            _hotel = hotel;
            _room = room;

            // Оновлення списку бронювань для кімнати
            UpdateReservationList();
            // Оновлення вартості проживання
            UpdatePrice();
        }

        // Отримання ціни за добу в залежності від типу кімнати
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
                    return 0; // Якщо тип кімнати не відомий
            }
        }

        // Оновлення ціни на основі введених дат
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
            else if (checkInDate.HasValue && checkOutDate.HasValue)
            {
                MessageBox.Show("Дата заїзду повинна бути раніше дати виїзду.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                PriceTextBlock.Text = $"Ціна: ${GetRoomPricePerDay()}";
            }
        }

        // Обробка натискання на кнопку "Зберегти"
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string guestName = GuestName.Text.Trim();
            DateTime? checkInDate = CheckInDatePicker.SelectedDate;
            DateTime? checkOutDate = CheckOutDatePicker.SelectedDate;

            // Перевірка введених даних
            if (string.IsNullOrEmpty(guestName))
            {
                MessageBox.Show("Будь ласка, введіть ім'я гостя.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!checkInDate.HasValue || !checkOutDate.HasValue)
            {
                MessageBox.Show("Будь ласка, введіть коректні дати заїзду та виїзду.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (checkInDate.Value >= checkOutDate.Value)
            {
                MessageBox.Show("Дата заїзду повинна бути раніше дати виїзду.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            if (checkInDate.Value < _hotel.CurrentTime.Date)
            {
                MessageBox.Show("Дата заїзду не може бути раніше поточної дати.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime checkInDateTime = checkInDate.Value.Date.AddHours(12);
            DateTime checkOutDateTime = checkOutDate.Value.Date.AddHours(12);

            // Перевірка на конфлікт з існуючими бронюваннями
            bool hasConflict = _hotel.Reservations.Any(r =>
                r.Room == _room &&
                !(checkOutDateTime <= r.CheckInDate || checkInDateTime >= r.CheckOutDate)
            );

            if (hasConflict)
            {
                MessageBox.Show("Цей номер вже зайнятий в обраний час. Будь ласка, перевірте введені дати.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            // Додавання або оновлення бронювання
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

            // Оновлення UI та збереження даних
            UpdateReservationList();
            _hotel.SaveData("hotel_data.json");
            Close();
        }

        // Обробка натискання на кнопку "Скасувати"
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Оновлення списку бронювань для кімнати
        private void UpdateReservationList()
        {
            ReservationsList.ItemsSource = _hotel.Reservations.Where(r => r.Room == _room).ToList();
            ReservationsList.Items.Refresh();
        }

        // Обробка натискання на кнопку "Видалити"
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
            else
            {
                MessageBox.Show("Не вдалося визначити обране бронювання для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Обробка зміни вибраного бронювання у списку
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
            else
            {
                GuestName.Clear();
                CheckInDatePicker.SelectedDate = null;
                CheckOutDatePicker.SelectedDate = null;
                UpdatePrice();
            }
        }

        // Обробка зміни дати заїзду
        private void CheckInDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdatePrice();
        }

        // Обробка зміни дати виїзду
        private void CheckOutDatePicker_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdatePrice();
        }
    }
}