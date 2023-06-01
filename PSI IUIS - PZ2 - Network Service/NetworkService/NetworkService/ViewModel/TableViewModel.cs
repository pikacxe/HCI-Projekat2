using MindFusion.UI.Wpf;
using NetworkService.Helpers;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NetworkService.ViewModel
{
    public class TableViewModel : BindableBase
    {

        private string layoutFile = ConfigurationManager.AppSettings.Get("layoutFile");
        public KeyboardLayout kbLayout { get; set; }
        private ObservableCollection<Reactor> reactors;
        public ObservableCollection<Reactor> Reactors
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Search))
                {
                    return reactors;
                }
                else
                {
                    return new ObservableCollection<Reactor>(reactors.Where(r => FindAll(r)));
                }
            }
            set
            {
                if (value != reactors)
                {
                    reactors = value;
                    OnPropertyChanged("Reactors");
                }
            }
        }

        public List<ReactorType> types
        {
            get => MainWindowViewModel.types;
        }

        private Reactor current;

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

        private Reactor Added = new Reactor();
        private int savedIndex = 0;
        public bool isName { get; set; } = true;

        private int selectedIndex = -1;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    OnPropertyChanged("SelectedIndex");
                    OnPropertyChanged("CanDelete");
                    OnPropertyChanged("Current");
                }
            }
        }
        public bool CanDelete
        {
            get => selectedIndex != -1;
        }


        private Visibility keyboardVisible = Visibility.Collapsed;
        public Visibility KeyboardVisible
        {
            get => keyboardVisible;
            set
            {
                if (value != keyboardVisible)
                {
                    keyboardVisible = value;
                    OnPropertyChanged("KeyboardVisible");
                }
            }
        }
        private Visibility keyboardNotVisible = Visibility.Visible;
        public Visibility KeyboardNotVisible
        {
            get => keyboardNotVisible;
            set
            {
                if (value != keyboardNotVisible)
                {
                    keyboardNotVisible = value;
                    OnPropertyChanged("KeyboardNotVisible");
                }
            }
        }
        private string search = string.Empty;
        public string Search
        {
            get => search;
            set
            {
                if (value != search)
                {
                    search = value;
                    OnPropertyChanged("Reactors");
                    OnPropertyChanged("Search");
                }
            }
        }

        public MyICommand AddComand { get; private set; }
        public MyICommand DeleteCommand { get; private set; }
        public MyICommand ClearCommand { get; private set; }
        public MyICommand<TextBox> KeyBoardFocusedCommand { get; private set; }
        public MyICommand CloseKeyboard { get; private set; }

        public TableViewModel()
        {
            AddComand = new MyICommand(OnAdd);
            DeleteCommand = new MyICommand(OnDelete);
            ClearCommand = new MyICommand(OnClear);
            KeyBoardFocusedCommand = new MyICommand<TextBox>(OnOpenKeyBoard);
            CloseKeyboard = new MyICommand(OnCloseKeyboard);
            Current = new Reactor();
            reactors = MainWindowViewModel.Reactors;
            kbLayout = KeyboardLayout.Create(layoutFile);
        }

        private void OnCloseKeyboard()
        {
            KeyboardNotVisible = Visibility.Visible;
            KeyboardVisible = Visibility.Collapsed;
        }

        private void OnOpenKeyBoard(TextBox tb)
        {
            KeyboardVisible = Visibility.Visible;
            KeyboardNotVisible = Visibility.Collapsed;
            MainWindowViewModel.UndoLastCommand = CloseKeyboard;
        }

        private void OnClear()
        {
            Added = Current.CopyTo();
            savedIndex = SelectedIndex;
            SelectedIndex = -1;
            Current = new Reactor();
            OnCloseKeyboard();
            MainWindowViewModel.UndoLastCommand = new MyICommand(UndoClear);
        }

        private void UndoClear()
        {
            if (savedIndex != -1)
            {
                SelectedIndex = savedIndex;
            }
            else
            {
                Current = Added;
            }
        }

        private string duplicateID = string.Empty;
        public string DuplicateID
        {
            get => duplicateID;
            set
            {
                if(value != duplicateID)
                {
                    duplicateID = value;
                    OnPropertyChanged("DuplicateID");
                }
            }
        }

        private void OnAdd()
        {
            Current.Validate();
            if(reactors.Where(x=> x.ID == Current.ID).Any())
            {
                DuplicateID = "ID already exists!";
                OnPropertyChanged("DuplicateID");
                return;
            }
            if (!Current.IsValid)
            {
                return;
            }
            Added = Current.CopyTo();
            reactors.Add(Current);
            MainWindowViewModel.treeViewReactors.Add(Current);
            Current = new Reactor();
            MainWindowViewModel.UndoLastCommand = new MyICommand(UndoAdd);
            OnPropertyChanged("Reactors");
            OnCloseKeyboard();
        }

        private void UndoAdd()
        {
            reactors.Remove(Added);
            MainWindowViewModel.treeViewReactors.Remove(Added);
        }

        private void OnDelete()
        {
            Added = Current.CopyTo();
            reactors.Remove(Current);
            MainWindowViewModel.treeViewReactors.Remove(Current);
            Current = new Reactor();
            OnCloseKeyboard();
            MainWindowViewModel.UndoLastCommand = new MyICommand(UndoDelete);
            OnPropertyChanged("Reactors");
        }

        private void UndoDelete()
        {
            reactors.Add(Added);
            MainWindowViewModel.treeViewReactors.Add(Added);
        }

        private bool FindAll(Reactor reactor)
        {
            Regex regex = new Regex(search);
            return isName ? regex.IsMatch(reactor.Name) : regex.IsMatch(reactor.Type.Name);
        }

    }
}
