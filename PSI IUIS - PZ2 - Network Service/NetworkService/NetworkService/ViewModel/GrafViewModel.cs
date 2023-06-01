using NetworkService.Helpers;
using NetworkService.Model;
using System;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;

namespace NetworkService.ViewModel
{
    public class GrafViewModel:BindableBase
    {
        private ObservableCollection<Reactor> reactors;
        public ObservableCollection<Reactor> Reactors
        {
            get
            {
                return reactors;
            }
        }
        private Reactor current = new Reactor();

        public Reactor Current
        {
            get => current;
            set
            {
                if (value != current)
                {
                    current = value;
                    OnPropertyChanged("Current");
                }
            }
        }

        public GrafViewModel()
        {
            reactors = MainWindowViewModel.Reactors;
            Current = new Reactor();
        }
    }
}
