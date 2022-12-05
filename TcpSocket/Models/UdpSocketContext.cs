namespace TcpSocket.Models
{
    public class UdpSocketContext : BaseSocketContext
    {
        private string targetIP = null!;

        public string TargetIP
        {
            get => this.targetIP;
            set
            {
                this.targetIP = value;
                CallModel();
            }
        }

        private string targetPort = null!;

        public string TargetPort
        {
            get => this.targetPort;
            set
            {
                this.targetPort = value;
                CallModel();
            }
        }
    }
}