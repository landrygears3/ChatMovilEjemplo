
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using ChatEjemploBibliotecaClase.ViewModels;

namespace ChatEjemploMovil.ViewModels
{
    public class ChatViewModel : ChatController, INotifyPropertyChanged
    {
        

        public ICommand LoginCommand { get; }
        public ICommand SendCommand { get; }

        public ChatViewModel()
        {
            LoginCommand = new Command(async () => await Login());
            SendCommand = new Command(async () => await SendMessage(), () => !string.IsNullOrWhiteSpace(Message));
        }

    }
}
