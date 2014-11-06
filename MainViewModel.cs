using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace molbox
{
    class MainViewModel : ObservableObject
    {
        CancellationTokenSource _cts;
        SerialPort port;

        double A = -0.0167125;
        double B = 1.02243;
        double C = -0.0141098;

        private string[] _rawFlowReading;
        private string _flowUnit;
        private double _flowValue;
        private double _factoredValue;
        private double _inversedValue;

        private string _flowReading;
        public string FlowReading
        {
            get { return _flowReading; }
            set
            {
                if (_flowReading != value)
                {
                    _flowReading = value;
                    OnPropertyChanged("FlowReading");
                }
            }
        }

        private string _factoredReading;
        public string FactoredReading
        {
            get { return _factoredReading; }
            set
            {
                if (_factoredReading != value)
                {
                    _factoredReading = value;
                    OnPropertyChanged("FactoredReading");
                }
            }
        }

        private string _inversedReading;
        public string InversedReading
        {
            get { return _inversedReading; }
            set
            {
                if (_inversedReading != value)
                {
                    _inversedReading = value;
                    OnPropertyChanged("InversedReading");
                }
            }
        }

        private string _flowAverage;
        public string FlowAverage
        {
            get { return _flowAverage; }
            set
            {
                if (_flowAverage != value)
                {
                    _flowAverage = value;
                    OnPropertyChanged("FlowAverage");
                }
            }
        }

        private string _factoredAverage;
        public string FactoredAverage
        {
            get { return _factoredAverage; }
            set
            {
                if (_factoredAverage != value)
                {
                    _factoredAverage = value;
                    OnPropertyChanged("FactoredAverage");
                }
            }
        }
        private string _averagingLabel;
        public string AveragingLabel
        {
            get { return _averagingLabel; }
            set
            {
                if (_averagingLabel != value)
                {
                    _averagingLabel = value;
                    OnPropertyChanged("AveragingLabel");
                }
            }
        }

        private ICommand _readCommand;
        public ICommand ReadCommand
        {
            get
            {
                if (_readCommand == null)
                {
                    _readCommand = new RelayCommand(
                        param => this.ReadFlowStartStop(true));
                }
                return _readCommand;
            }
        }

        private ICommand _stopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (_stopCommand == null)
                {
                    _stopCommand = new RelayCommand(
                        param => this.ReadFlowStartStop(false));
                }
                return _stopCommand;
            }
        }

        public MainViewModel()
        {
            port = new SerialPort("COM7", 9600, Parity.Even, 7, StopBits.One);
            port.NewLine = Environment.NewLine;
            port.Open();
        }

        private void ReadFlowStartStop(bool start)
        {
            if (start)
            {
                _cts = new CancellationTokenSource();
                ThreadPool.QueueUserWorkItem(_ => ReadMolboxFlow(_cts.Token));
            }
            else
            {
                if (_cts != null)
                {
                    _cts.Cancel();
                    _cts = null;
                }
            }
        }

        private void ReadMolboxFlow(CancellationToken ct)
        {
            List<double> _flowValues = new List<double>();
            List<double> _factoredValues = new List<double>();
            try
            {
                while (true)
                {
                    if (ct.IsCancellationRequested)
                        break;
                    AveragingLabel = string.Format("Averaging ({0}):", 5 - _flowValues.Count);
                    port.WriteLine("FR");
                    _rawFlowReading = port.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    _flowUnit = _rawFlowReading[2];
                    _flowValue = double.Parse(_rawFlowReading[1]);
                    _factoredValue = propaneToMB51(_flowValue);
                    _inversedValue = MB51toPropane(_factoredValue);
                    _flowValues.Add(_flowValue);
                    _factoredValues.Add(_factoredValue);

                    if (_rawFlowReading[0] != "R")
                    {
                        _flowValues.Clear();
                        FlowAverage = string.Format("{0} {1}", "-.----", _flowUnit);
                        FactoredAverage = string.Format("{0} {1}", "-.----", _flowUnit);
                    }
                    if (_flowValues.Count == 5)
                    {
                        FlowAverage = string.Format("{0:0.0000} {1}", _flowValues.Average(), _flowUnit);
                        FactoredAverage = string.Format("{0:0.0000} {1}", _factoredValues.Average(), _flowUnit);
                        _flowValues.Clear();
                        _factoredValues.Clear();
                    }

                    FlowReading = string.Format("{0:0.0000} {1}", _flowValue, _flowUnit);
                    FactoredReading = string.Format("{0:0.0000} {1}", _factoredValue, _flowUnit);
                    InversedReading = string.Format("{0:0.0000} {1}", _inversedValue, _flowUnit);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception raised.\r\n" + ex.Message);
            }
        }

        private double propaneToMB51(double flowValue)
        {
            return (A * (flowValue * flowValue)) + (B * flowValue) + C;
        }

        private double MB51toPropane(double flowValue)
        {
            return (-B + Math.Sqrt((B * B) - ((4 * A) * (C - flowValue)))) / (2 * A);
        }
    }
}
