
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Text.Json;
using ChatEjemploMovil.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatEjemploMovil.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private HubConnection _connection;

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value; OnPropertyChanged();
                ((Command)SendCommand).ChangeCanExecute();
            }
        }



        private string _nickname;
        private int _userId;

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set { _isLoggedIn = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Usuarios { get; } = new();
        public ObservableCollection<MessageModel> Mensajes { get; } = new();

        public ICommand LoginCommand { get; }
        public ICommand SendCommand { get; }

        public ChatViewModel()
        {
            LoginCommand = new Command(async () => await Login());
            SendCommand = new Command(async () => await SendMessage(), () => !string.IsNullOrWhiteSpace(Message));
        }

        private async Task Login()
        {
            try
            {
                using var client = new HttpClient();
                var res = await client.GetAsync($"https://www.hacemosalgo.net/Hub/api/usuarios?email={Uri.EscapeDataString(Email)}");

                if (!res.IsSuccessStatusCode)
                    throw new Exception("Usuario no encontrado");

                var json = await res.Content.ReadAsStringAsync();
                var usuario = JsonSerializer.Deserialize<UserModel>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (!usuario.Status)
                    throw new Exception("Cuenta desactivada");

                _userId = usuario.Id;
                _nickname = usuario.Nickname;
                IsLoggedIn = true;

                await Conectar();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task Conectar()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://www.hacemosalgo.net/Hub/chatHub")
                .Build();

            _connection.On<MessageModel>("Receivemessage", mensaje =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var esMio = mensaje.Sender == _nickname;
                    mensaje.IsMine = esMio;
                    Mensajes.Add(mensaje);
                });
            });

            _connection.On<string, List<string>>("UsuarioConectado", (nickname, lista) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Usuarios.Clear();
                    foreach (var u in lista)
                        Usuarios.Add(u == _nickname ? $"{u} (tú)" : u);
                });
            });

            await _connection.StartAsync();
            await _connection.InvokeAsync("RegistrarUsuario", _nickname);
        }

        private async Task SendMessage()
        {
            var msg = new MessageModel
            {
                Id = _userId,
                Sender = _nickname,
                Message = Message
            };

            await _connection.InvokeAsync("SendMessaje", msg);
            Message = string.Empty;
        }

        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



 
    }
}
