using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NetworkService.Helpers;

namespace NetworkService.Model
{
    public class Reactor : ValidationBase
    {
        private int id;
        private string name;
        private ReactorType type;
        private double measuredValue;
        private List<DataPoint> readingHistory;
        private int counter = 0;

        public int ID
        {
            get => id;
            set
            {
                if (value != id)
                {
                    id = value;
                    OnPropertyChanged("ID");
                }
            }
        }
        public string Name
        {
            get => name;
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string SmallName
        {
            get
            {
                return name.Length > 10 ? name.Substring(0, 10) + "..." : name;
            }
        }

        public ReactorType Type
        {
            get => type;
            set
            {
                if (value != type)
                {
                    type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public double MeasuredValue
        {
            get => measuredValue;
            set
            {
                if (value != measuredValue)
                {
                    measuredValue = value;
                    OnPropertyChanged("MeasuredValue");
                    OnPropertyChanged("ReadingHistory");
                }
            }
        }

        public List<DataPoint> ReadingHistory
        {
            get => readingHistory;
            set
            {
                readingHistory = value.Take(5).ToList();
                OnPropertyChanged("ReadingHistory");
            }
        }
        private Visibility notValidValue = Visibility.Visible;
        public Visibility NotValidValue
        {
            get => notValidValue;
            set
            {
                if (value != notValidValue)
                {
                    notValidValue = value;
                    OnPropertyChanged("NotValidValue");
                }
            }
        }

        public Reactor() : this(0, string.Empty, null, 0)
        {

        }

        public Reactor(int id, string name, ReactorType type, double measuredValue)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.measuredValue = measuredValue;
            this.readingHistory = new List<DataPoint>()
            {
                new DataPoint{ Value = 0, VerticalPosition = 10},
                new DataPoint{ Value = 0, VerticalPosition = 120},
                new DataPoint{ Value = 0, VerticalPosition = 240},
                new DataPoint{ Value = 0, VerticalPosition = 360},
                new DataPoint{ Value = 0, VerticalPosition = 480}
            };
        }
        public void AddValue(int value)
        {
            MeasuredValue = value;
            if (ValidateValue())
            {
                NotValidValue = Visibility.Visible;
                ReadingHistory[counter].Value = value;
                ReadingHistory[counter].TimeStamp = DateTime.Now.ToString(" HH:mm:ss");
                counter = (counter + 1) % 5;
            }
            else
            {
                NotValidValue = Visibility.Hidden;
                ReadingHistory[counter].Value = value;
                ReadingHistory[counter].TimeStamp = "invalid value";
                counter = (counter + 1) % 5;
            }
        }
        public Reactor CopyTo()
        {
            return new Reactor(id, name, type, measuredValue);
        }

        public bool ValidateValue()
        {
            return MeasuredValue >= 250 && MeasuredValue <= 350;
        }

        private bool ValidateType()
        {
            return Type == null;
        }
        protected override void ValidateSelf()
        {
            if (ID < 1)
            {
                ValidationErrors["ID"] = "ID must be positive and greater than 0!";
            }
            else
            {
                ValidationErrors["ID"] = string.Empty;
            }
            if (string.IsNullOrWhiteSpace(Name))
            {
                ValidationErrors["Name"] = "Name is required!";
            }
            else
            {
                ValidationErrors["Name"] = string.Empty;
            }
            if (ValidateType())
            {
                ValidationErrors["Type"] = "Type is required!";
            }
            else
            {
                ValidationErrors["Type"] = string.Empty;
            }
        }
    }
}
