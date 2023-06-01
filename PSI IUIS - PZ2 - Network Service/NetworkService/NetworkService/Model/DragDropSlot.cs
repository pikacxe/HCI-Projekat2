using NetworkService.Helpers;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkService.Model
{
    public class DragDropSlot : BindableBase
    {
        private Reactor slot;
        public Reactor Slot
        {
            get => slot;
            set
            {
                if (value != slot)
                {
                    slot = value;
                    OnPropertyChanged(nameof(Slot));
                    OnPropertyChanged("ValueNotValid");
                }
            }
        }

        private bool occupied = false;
        public bool Occupied
        {
            get => occupied;
            set
            {
                if (occupied != value)
                {
                    occupied = value;
                    OnPropertyChanged(nameof(Occupied));
                }
            }
        }
        public Point Center { get; set; } = new Point();
        public DragDropSlot()
        {
            slot = new Reactor();
        }
    }
}
