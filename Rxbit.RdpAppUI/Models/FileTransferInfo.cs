using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rxbit.RdpAppUI.Models
{
    public class FileTransferInfo : INotifyPropertyChanged
    {
        private string _fileName;
        private long _fileSize;
        private long _bytesTransferred;
        private TransferStatus _status;
        private TransferDirection _direction;
        private double _progress;

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(nameof(FileName)); }
        }

        public long FileSize
        {
            get => _fileSize;
            set { _fileSize = value; OnPropertyChanged(nameof(FileSize)); }
        }

        public long BytesTransferred
        {
            get => _bytesTransferred;
            set
            {
                _bytesTransferred = value;
                OnPropertyChanged(nameof(BytesTransferred));
                Progress = (double)value / FileSize * 100;
            }
        }

        public double Progress
        {
            get => _progress;
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

        public TransferStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }

        public TransferDirection Direction
        {
            get => _direction;
            set { _direction = value; OnPropertyChanged(nameof(Direction)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public enum TransferStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }

    public enum TransferDirection
    {
        Upload,
        Download
    }
}
