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
    }
}
