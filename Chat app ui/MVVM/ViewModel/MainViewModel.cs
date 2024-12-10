using Chat_app_ui.Core;
using Chat_app_ui.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat_app_ui.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {

        public ObservableCollection<MessageModel> Messages { get; set; }
        public ObservableCollection<ContactModel> Contacts { get; set; }

        //Commands
        public RelayCommand SendCommand { get; set; }

        private ContactModel _selectedContact;

        public ContactModel SelectedContact
        {
            get { return _selectedContact; }
            set 
            { 
                _selectedContact = value;
                OnPropertyChanged();
            }
        }


        private string _message;

        public string Message
        {
            get { return _message; }
            set { _message = value; 
            OnPropertyChanged(); }
        }


        public MainViewModel()
        {
            Messages=new ObservableCollection<MessageModel>();
            Contacts=new ObservableCollection<ContactModel>();

            SendCommand = new RelayCommand(o => 
            {
                Messages.Add(new MessageModel
                {
                    Message = Message,
                    FirstMessage = false
                });

                Message = "";
            });


            Messages.Add(new MessageModel
            {
                Username = "Allison",
                UsernameColour = "#409aff",
                ImageSource = "https://user-images.githubusercontent.com/6140137/103431357-2a308d00-4b94-11eb-8d57-fa3aad480428.png",
                Message = "test",
                Time = DateTime.Now,
                IsNativeOrigin = false,
                FirstMessage = true
            });

            for (int i = 0; i < 3; i++)
            {
                Messages.Add(new MessageModel
                {
                    Username = "Allison",
                    UsernameColour = "#409aff",
                    ImageSource = "https://user-images.githubusercontent.com/6140137/103431357-2a308d00-4b94-11eb-8d57-fa3aad480428.png",
                    Message = "test",
                    Time = DateTime.Now,
                    IsNativeOrigin = false,
                    FirstMessage = false
                });
            }

            for (int i = 0; i < 4; i++)
            {
                Messages.Add(new MessageModel
                {
                    Username = "Bunny",
                    UsernameColour = "#409aff",
                    ImageSource = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSjeXV2uqjQnL5hQII_q_J-yEwIShnqU98H0Q&s",
                    Message = "test",
                    Time = DateTime.Now,
                    IsNativeOrigin = true,
                });
            }

            Messages.Add(new MessageModel
            {
                Username = "Bunny",
                UsernameColour = "#409aff",
                ImageSource = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSjeXV2uqjQnL5hQII_q_J-yEwIShnqU98H0Q&s",
                Message = "last",
                Time = DateTime.Now,
                IsNativeOrigin = true,
            });

            for (int i = 0; i < 5; i++)
            {
                Contacts.Add(new ContactModel
                {
                    Username = $"Allison {i}",
                    ImageSource = "https://w7.pngwing.com/pngs/309/279/png-transparent-stick-man-illustration-warframe-internet-meme-youtube-know-your-meme-high-resolution-poker-face-miscellaneous-white-english-thumbnail.png",
                    Messages = Messages

                });
            }
        }
    }
}
