using NetworkService.Helpers;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NetworkService.ViewModel
{
    public class DragDropViewModel : BindableBase
    {
        public ObservableCollection<Reactor> Reactors
        {
            get => MainWindowViewModel.treeViewReactors;

        }
        public ObservableCollection<DragDropSlot> Slots
        {
            get => MainWindowViewModel.slots;
        }
        public ObservableCollection<Line> Lines
        {
            get => MainWindowViewModel.lines;
        }
        private bool isDrawing = false;
        private Reactor draggedItem = null;
        public Reactor DraggedItem
        {
            get => draggedItem;
            set
            {
                if (value != draggedItem)
                {
                    draggedItem = value;
                    OnPropertyChanged("DraggedItem");
                }
            }
        }

        private DragDropSlot lastUsed = new DragDropSlot();
        private List<Line> savedLines = new List<Line>();
        private Reactor last = new Reactor();
        private Line lastLine = new Line();

        private Visibility expanded = Visibility.Collapsed;
        public Visibility Expanded
        {
            get => expanded;
            set
            {
                if (value != expanded)
                {
                    expanded = value;
                    OnPropertyChanged("Expanded");
                }
            }
        }
        private string expanderButtonText = "⏩";
        public string ExpanderButtonText
        {
            get => expanderButtonText;
            set
            {
                if (value != expanderButtonText)
                {
                    expanderButtonText = value;
                    OnPropertyChanged("ExpanderButtonText");
                }
            }
        }
        private int expanderWidth = 50;
        public int ExpanderWidth
        {
            get => expanderWidth;
            set
            {
                if (value != expanderWidth)
                {
                    expanderWidth = value;
                    OnPropertyChanged("ExpanderWidth");
                }
            }
        }
        public MyICommand ExpandCommand { get; private set; }
        public MyICommand<TreeView> StartDragTreeCommand { get; private set; }
        public MyICommand<ListView> StartDragListCommand { get; private set; }
        public MyICommand<DragDropSlot> DropCommand { get; private set; }
        public MyICommand<DragDropSlot> ClearCommand { get; private set; }
        public MyICommand<DragDropSlot> StartDrawing { get; private set; }
        public Line currentLine { get; set; }
        private bool isDraggingTree = false;
        public bool IsDraggingTree
        {
            get => isDraggingTree;
            set
            {
                if (value != isDraggingTree)
                {
                    isDraggingTree = value;
                    OnPropertyChanged("IsDraggingTree");
                }
            }
        }
        private bool isDraggingList = false;
        public bool IsDraggingList
        {
            get => isDraggingList;
            set
            {
                if (value != isDraggingList)
                {
                    isDraggingList = value;
                    OnPropertyChanged("IsDraggingList");
                }
            }
        }

        private bool DragOver = false;

        public DragDropViewModel()
        {
            ExpandCommand = new MyICommand(ExpandCollapse);
            StartDragTreeCommand = new MyICommand<TreeView>(OnStartDragTreeView);
            StartDragListCommand = new MyICommand<ListView>(OnStartDragListView);
            ClearCommand = new MyICommand<DragDropSlot>(ClearSlot);
            DropCommand = new MyICommand<DragDropSlot>(OnDrop);
            StartDrawing = new MyICommand<DragDropSlot>(OnStartDraw);
        }

        private void OnStartDraw(DragDropSlot slot)
        {
            isDraggingList = false;
            if (slot.Occupied)
            {
                if (!isDrawing)
                {
                    Random r = new Random();
                    isDrawing = true;
                    currentLine = new Line();
                    currentLine.StrokeThickness = 10;
                    currentLine.Stroke = new SolidColorBrush(Color.FromRgb((byte)r.Next(1, 255), (byte)r.Next(1, 255), (byte)r.Next(1, 255)));
                    currentLine.X1 = slot.Center.X;
                    currentLine.Y1 = slot.Center.Y;
                }
                else
                {
                    isDrawing = false;
                    currentLine.X2 = slot.Center.X;
                    currentLine.Y2 = slot.Center.Y;
                    lastLine = currentLine;
                    if (!ContainsLine(currentLine))
                    {
                        Lines.Add(currentLine);
                        MainWindowViewModel.UndoLastCommand = new MyICommand(UndoDraw);
                    }
                    currentLine = null;
                }
            }
        }

        private bool ContainsLine(Line currentLine)
        {
            foreach(var x in Lines)
            {
                if (AreLinesEqual(currentLine, x))
                {
                    return true;
                }
            }
            return false;
        }
        private bool AreLinesEqual(Line line1, Line line2)
        {
            // Compare all combinations of start and end points
            bool arePointsEqual1 = (line1.X1 == line2.X1 && line1.Y1 == line2.Y1) || (line1.X1 == line2.X2 && line1.Y1 == line2.Y2);
            bool arePointsEqual2 = (line1.X2 == line2.X1 && line1.Y2 == line2.Y1) || (line1.X2 == line2.X2 && line1.Y2 == line2.Y2);

            return arePointsEqual1 && arePointsEqual2;
        }

        private void UndoDraw()
        {
            Lines.Remove(lastLine);
        }

        public void ClearSlot(DragDropSlot slot)
        {
            lastUsed = slot;
            last = slot.Slot.CopyTo();
            Reactors.Add(slot.Slot.CopyTo());
            slot.Slot = new Reactor();
            slot.Occupied = false;
            ClearLine(slot);
            MainWindowViewModel.UndoLastCommand = new MyICommand(UndoClear);
        }

        private void UndoClear()
        {
            Reactors.Remove(last);
            lastUsed.Slot = last;
            lastUsed.Slot.ValidateValue();
            foreach (var x in savedLines)
            {
                Lines.Add(x);
            }
            savedLines = new List<Line>();
            isDraggingList = false;
        }

        private void ClearLine(DragDropSlot slot)
        {
            var lines = new List<Line>(Lines);
            foreach (var x in lines)
            {
                if ((x.X1 == slot.Center.X && x.Y1 == slot.Center.Y) || (x.X2 == slot.Center.X && x.Y2 == slot.Center.Y))
                {
                    savedLines.Add(x);
                    Lines.Remove(x);
                }
            }
        }

        private void OnStartDragTreeView(TreeView treeView)
        {
            if (!IsDraggingTree)
            {
                IsDraggingTree = true;
                DraggedItem = (Reactor)treeView.SelectedItem;
                DragDrop.DoDragDrop(treeView, DraggedItem, DragDropEffects.Move | DragDropEffects.Copy);
                DraggedItem = null;
                isDraggingTree = false;
            }
        }

        private void OnStartDragListView(ListView listView)
        {
            if (!IsDraggingList)
            {
                IsDraggingList = true;
                DragDropSlot slot = ((DragDropSlot)listView.SelectedItem);
                DraggedItem = slot.Slot;
                DragOver = false;
                DragDrop.DoDragDrop(listView, DraggedItem, DragDropEffects.Move | DragDropEffects.Copy);
                IsDraggingList = false;
                if (DragOver)
                {
                    slot.Occupied = false;
                    slot.Slot = new Reactor();
                    ClearLine(slot);
                }
            }
        }

        private void OnDrop(DragDropSlot slot)
        {
            if (!slot.Occupied)
            {
                slot.Occupied = true;
                slot.Slot = DraggedItem;
                isDraggingList = false;
                lastUsed = slot;
                Reactors.Remove(slot.Slot);
                DragOver = true;
                MainWindowViewModel.UndoLastCommand = new MyICommand(UndoDrop);
            }
        }

        private void UndoDrop()
        {
            Reactors.Add(lastUsed.Slot);
            lastUsed.Occupied = false;
            lastUsed.Slot = new Reactor();
        }

        private void ExpandCollapse()
        {
            if (Expanded == Visibility.Collapsed)
            {
                ExpanderWidth = 250;
                ExpanderButtonText = "⏪";
                Expanded = Visibility.Visible;
            }
            else
            {
                ExpanderWidth = 50;
                ExpanderButtonText = "⏩";
                Expanded = Visibility.Collapsed;
            }
            MainWindowViewModel.UndoLastCommand = ExpandCommand;
        }
    }
}
