using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client__.Net_.MVVM.Model
{
    public class Group
    {
        //public int Id { get; set; }
        public string GroupName { get; set; }

        public string ImageSource { get; set; }

        public string Messages { get; set; }

        //public string LastMessage => Messages.Last().message;
    }
}
