using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkService.Helpers;

namespace NetworkService.Model
{
    public class DataPoint : BindableBase
    {
        private int value;
        private int verticalPosition;
        private string timeStamp = "unknown";
        public int Value
        {
            get => value;
            set
            {
                if(value != this.value)
                {
                    this.value = value;
                    OnPropertyChanged("Value");
                }
            }
        }
        public int VerticalPosition 
        {
            get => this.verticalPosition;
            set
            {
                if(this.verticalPosition != value)
                {
                    this.verticalPosition = value;
                    OnPropertyChanged("VerticalPosition");
                }
            }
        }

        public string TimeStamp
        {
            get => timeStamp;
            set
            {
                if(value != timeStamp)
                {
                    timeStamp = value;
                    OnPropertyChanged("TimeStamp");
                }
            }
        }
    }
}
