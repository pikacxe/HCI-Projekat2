using NetworkService.Helpers;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        private string logsPath = ConfigurationManager.AppSettings.Get("logsPath");

        private static ObservableCollection<Reactor> reactors = new ObservableCollection<Reactor>();
        public static ObservableCollection<Reactor> Reactors
        {
            get => reactors;
            set
            {
                if (value != reactors)
                {
                    reactors = value;
                }
            }
        }
        public static List<ReactorType> types = new List<ReactorType>(){
            new ReactorType { Name = "Resident time distribution", ImgPath=ConfigurationManager.AppSettings.Get("typeRTD") },
            new ReactorType { Name = "Thermo Couple", ImgPath = ConfigurationManager.AppSettings.Get("typeThermoCouple") } };


        #region DragDropState
        public static ObservableCollection<DragDropSlot> slots = new ObservableCollection<DragDropSlot>
            {
                new DragDropSlot(){Center= new Point(50 ,75)},
                new DragDropSlot(){Center= new Point(170,75)},
                new DragDropSlot(){Center= new Point(290,75)},
                new DragDropSlot(){Center= new Point(410,75)},
                new DragDropSlot(){Center= new Point(50 ,215)},
                new DragDropSlot(){Center= new Point(170,215)},
                new DragDropSlot(){Center= new Point(290,215)},
                new DragDropSlot(){Center= new Point(410,215)},
                new DragDropSlot(){Center= new Point(50 ,345)},
                new DragDropSlot(){Center= new Point(170,345)},
                new DragDropSlot(){Center= new Point(290,345)},
                new DragDropSlot(){Center= new Point(410,345)},
                new DragDropSlot(){Center= new Point(50 ,475)},
                new DragDropSlot(){Center= new Point(170,475)},
                new DragDropSlot(){Center= new Point(290,475)},
                new DragDropSlot(){Center= new Point(410,475)},
                new DragDropSlot(){Center= new Point(50 ,605)},
                new DragDropSlot(){Center= new Point(170,605)},
                new DragDropSlot(){Center= new Point(290,605)},
                new DragDropSlot(){Center= new Point(410,605)},
            };
        public static ObservableCollection<Line> lines = new ObservableCollection<Line>();
        public static ObservableCollection<Reactor> treeViewReactors = new ObservableCollection<Reactor>(Reactors);

        #endregion
        #region Commands
        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand ExitCommand { get; private set; }
        public MyICommand UndoCommand { get; private set; }
        private TableViewModel tableViewModel = new TableViewModel();
        private DragDropViewModel dragDropViewModel = new DragDropViewModel();
        private GrafViewModel grafViewModel = new GrafViewModel();
        public static ICommand UndoLastCommand { get; set; }
        #endregion

        private BindableBase currentViewModel;
        private BindableBase lastViewModel;

        public BindableBase CurrentViewModel
        {
            get => currentViewModel;
            set => SetProperty(ref currentViewModel, value, "CurrentViewModel");
        }

        public MainWindowViewModel()
        {
            createListener(); //Povezivanje sa serverskom aplikacijom
            NavCommand = new MyICommand<string>(OnNav);
            ExitCommand = new MyICommand(Exit);
            UndoCommand = new MyICommand(Undo);
            GenerateData(10);
            LoadHistory();
            currentViewModel = tableViewModel;
            treeViewReactors = new ObservableCollection<Reactor>(Reactors);
        }

        private void LoadHistory()
        {
            int[] counters = new int[Reactors.Count()];
            using (StreamReader sr = new StreamReader(logsPath, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string[] line = sr.ReadLine().Split('_');
                    string[] data = line[2].Split(':');
                    int id = int.Parse(data[0]);
                    int value = int.Parse(data[1]);
                    if (value < 250 || value > 350)
                    {
                        Reactors[id].ReadingHistory[counters[id]].TimeStamp = "invalid value";
                        Reactors[id].ReadingHistory[counters[id]].Value = value;
                    }
                    else
                    {
                        Reactors[id].ReadingHistory[counters[id]].TimeStamp = line[0];
                        Reactors[id].ReadingHistory[counters[id]].Value = value;
                    }
                    counters[id] = (counters[id] + 1) % 5;
                }
            }
        }


        private void Undo()
        {
            if (UndoLastCommand != null)
            {
                UndoLastCommand.Execute(null);
                UndoLastCommand = null;
            }
        }

        private void GenerateData(int count)
        {
            Random r = new Random();
            for (int i = 0; i < count; i++)
            {
                reactors.Add(new Reactor { ID = i + 1, Name = $"Name {i + 1}", Type = types[r.Next(0, 2)] });
            }
        }

        private void OnNav(string name)
        {
            switch (name)
            {
                case "table": lastViewModel = CurrentViewModel; CurrentViewModel = tableViewModel; break;
                case "graf": lastViewModel = CurrentViewModel; CurrentViewModel = grafViewModel; break;
                case "dragdrop": lastViewModel = CurrentViewModel; CurrentViewModel = dragDropViewModel; break;
                default: break;
            }
            UndoLastCommand = new MyICommand(UndoNav);
        }

        private void UndoNav()
        {
            CurrentViewModel = lastViewModel;
        }

        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25565);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            Byte[] data = Encoding.ASCII.GetBytes(reactors.Count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            Console.WriteLine(incomming); //Na primer: "Entitet_1:272"
                            string[] data = incomming.Trim().Split(':');
                            int id = int.Parse(data[0].Split('_')[1].Trim());
                            int value = int.Parse(data[1].Trim());
                            Console.WriteLine($"ID:{id} Value:{value}");
                            if (id != -1 && id < reactors.Count())
                            {
                                reactors[id].AddValue(value);
                                WriteLog(incomming);
                            }
                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }

        public void WriteLog(string log)
        {
            using (StreamWriter sw = new StreamWriter(logsPath, true))
            {
                sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}_{log}");
            }
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}

