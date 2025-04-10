using ImTools;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Threading;
using UARTLogging.Models;
using Windows.Storage.Pickers;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Threading;
using Windows.UI;

namespace UARTLogging.ViewModels
{
    public class MainViewModel : BindableBase
    {
        public MainViewModel()
        {
            _updateTimer.Tick += _onUpdateTimerTick;
            _onLoadComPorts();
            _selectedPort = ComPorts[0];
            _selectedSpeed = Speeds[8];
            _selectedData = Datas[1].Value;
            _selectedParity = Paritys[0].Value;
            _selectedStopBit = Stopbits[0].Value;

        }

        private void _onUpdateTimerTick(object sender, EventArgs e)
        {
            Logs.Add(new LogModel
            {
                LogIndex = index++,
                LogTimeStamp = DateTime.Now,
                LogData = "XXXXXXXXXXXXXXXXXXXX"
            });
            //while (_dataQueue.Count > 0)
            //{
            //    DataReceived.Add(_dataQueue.Dequeue());
            //}

        }
        #region Properties
        private string _title = "UART Logging";
        public string Title { get { return _title; } set { SetProperty(ref _title, value); } }
        private SerialPort _serialPort;
        private Queue<string> _dataQueue = new Queue<string>();
        private int index;
        public ObservableCollection<LogModel> Logs { get; } = new ObservableCollection<LogModel>();
        private readonly DispatcherTimer _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        private bool _isReading = false;
        public bool IsReading 
        { 
            get { return _isReading; } 
            set 
            { 
                SetProperty(ref _isReading, value);
                IsStartEnable = !value;
            } 
        }
        private bool _isTimeStampChecked = true;
        public bool IsTimeStampChecked
        {
            get { return _isTimeStampChecked; }
            set
            {
                SetProperty(ref _isTimeStampChecked, value);
            }
        }
        private bool _isStartEnable = true;
        public bool IsStartEnable { get { return _isStartEnable; } set { SetProperty(ref _isStartEnable, value); } }
        private string _savePath = "Enter save folder here";
        public string SavePath
        {
            get { return _savePath; }
            set
            {
                SetProperty(ref _savePath, value);
            }
        }
        public ObservableCollection<string> ComPorts { get; } = new ObservableCollection<string>();
        public List<int> Speeds { get; } = new List<int>
        {
            110, 300, 600, 1200, 2400, 4800, 9600,
            14400, 19200, 28800, 38400, 57600,
            115200, 128000, 256000, 460800, 921600
        };
        public List<ComboBoxItemModel<int>> Datas { get; } = new()
        {
            new(7, "7 bit"),
            new(8, "8 bit")
        };
        public List<ComboBoxItemModel<Parity>> Paritys { get; } = new()
        {
            new(Parity.None, "none"),
            new(Parity.Odd, "odd"),
            new(Parity.Even, "even"),
            new(Parity.Mark, "mark"),
            new(Parity.Space, "space")
        };
        public List<ComboBoxItemModel<StopBits>> Stopbits { get; } = new()
        {
            new(StopBits.One, "1 bit"),
            new(StopBits.Two, "2 bit")
        };
        public List<ComboBoxItemModel<string>> FlowControls { get; } = new()
        {
            new("none", "none"),
            new("Xon/Xoff", "Xon/Xoff"),
            new("RTS/CTS", "RTS/CTS"),
            new("DSR/DTR", "DSR/DTR")
        };
        private string _selectedPort;
        public string SelectedPort {  get { return _selectedPort; } set { SetProperty(ref _selectedPort, value);  } }
        private int _selectedSpeed;
        public int SelectedSpeed { get { return _selectedSpeed; } set { SetProperty(ref _selectedSpeed, value); } }
        private int _selectedData;
        public int SelectedData { get { return _selectedData; } set { SetProperty(ref _selectedData, value); } }
        private Parity _selectedParity;
        public Parity SelectedParity { get { return _selectedParity; } set { SetProperty(ref _selectedParity, value); } }
        private StopBits _selectedStopBit;
        public StopBits SelectedStopBit { get { return _selectedStopBit; } set { SetProperty(ref _selectedStopBit, value); } }
        private string _selectedFlowControl = "none";
        public string SelectedFlowControl { get { return _selectedFlowControl; } set { SetProperty(ref _selectedFlowControl, value); } }
        #endregion
        #region Commands
        public DelegateCommand OnStartClick => new DelegateCommand(_onStartClick);
        public DelegateCommand OnStopClick => new DelegateCommand(_onStopClick);
        public DelegateCommand OnFolderPickerClick => new DelegateCommand(_onFolderPickerClick);
        private void _onStartClick()
        {
            try
            {
                _serialPort = new SerialPort(SelectedPort, SelectedSpeed, SelectedParity, SelectedData, SelectedStopBit);
                _serialPort.Handshake = Handshake.None;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(_onDataReceived);
                _serialPort.WriteTimeout = 500;
                if (_serialPort != null && !_serialPort.IsOpen)
                {
                    _serialPort.Open();
                    //DataReceived.Clear();
                    //DataReceived.Add($"[{(IsTimeStampChecked ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : "")}] Start logging");
                    _updateTimer.Start();
                    IsReading = true;
                }
            }
            catch (Exception ex)
            {
                //DataReceived.Clear();
                //DataReceived.Add("Error opening port: " + ex.Message);
                _updateTimer.Stop();
            }
        }
        private void _onStopClick()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    //DataReceived.Add($"[{(IsTimeStampChecked ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : "")}] Stop logging");
                    _updateTimer.Stop();
                    IsReading = false;
                }
            }
            catch (Exception ex)
            {
                //DataReceived.Add("Error closing port: " + ex.Message);
            }
        }
        private void _onFolderPickerClick()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text or Log files (*.txt;*.log)|*.txt;*.log";
            dialog.FileName = "Uartlog.txt";
            if (dialog.ShowDialog() == true)
            {
                SavePath = dialog.FileName;
            }    
        }
        private void _onDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(100); // Wait for the data to be available
            // Read the data from the serial port
            string data = _serialPort.ReadLine();
            if (IsTimeStampChecked)
            {
                _dataQueue.Enqueue("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]" + " : " + data.Trim());
            }
            else
            {
                _dataQueue.Enqueue(data.Trim());
            }
        }
        private void _onLoadComPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            ComPorts.Clear();
            foreach (string port in ports)
            {
                ComPorts.Add(port);
            }
        }
        #endregion
        
    }
}
