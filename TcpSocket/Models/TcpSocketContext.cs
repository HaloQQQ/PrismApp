using System;
using System.Collections.Generic;

namespace TcpSocket.Models
{
    public class TcpSocketContext : BaseSocketContext
    {
        private IList<string> connList = null!;

        public IList<string> ConnList
        {
            get => this.connList;
            set
            {
                this.connList = value;
                CallModel();
            }
        }

        public ushort MaxClientCount { get; set; } = 10;

        private volatile bool _canReConnect;

        public bool CanReConnect
        {
            get => this._canReConnect;

            set
            {
                this._canReConnect = value;
                CallModel();
                this.CanReConnectChanged?.Invoke(this._canReConnect);
            }
        }

        public event Action<bool> CanReConnectChanged;
    }
}