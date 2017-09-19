using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLibrary
{
    /// <summary>
    /// Socket�����
    /// </summary>
    public class Server : SocketBase
    {
        private TcpListener _listener;
        private IPAddress ipAddress;
        private int port;

        /// <summary>
        /// Ĭ��������IP�Ĵ˶˿�
        /// </summary>
        /// <param name="port">�˿ں�</param>
        public Server(int port)
        {
            this.ipAddress = IPAddress.Any;
            this.port = port;
        }
        /// <summary>
        /// ������IP�Ĵ˶˿�
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">�˿ں�</param>
        public Server(string ip, int port)
        {
            this.ipAddress = IPAddress.Parse(ip);
            this.port = port;
        }

        /// <summary>
        /// ��ȡ���Ӽ���
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, Connection> GetConnections()
        {
            return this.Connections;
        }

        /// <summary>
        /// ��ȡָ��������������,��ѯ��������null
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public Connection GetConnection(string connectionName)
        {
            Connection connection;
            this.Connections.TryGetValue(connectionName, out connection);
            return connection;
        }

        private Thread _listenConnection;
        /// <summary>
        /// �򿪼���
        /// </summary>
        public void StartServer()
        {
            _listener = new TcpListener(this.ipAddress, this.port);
            _listener.Start();

            _listenConnection = new Thread(new ThreadStart(Start));
            _listenConnection.Start();

            this.StartListenAndSend();
        }

        private void Start()
        {
            try
            {
                while (true)
                {
                    if (_listener.Pending())
                    {
                        TcpClient client = _listener.AcceptTcpClient();
                        string piEndPoint = client.Client.RemoteEndPoint.ToString();
                        Connection connection = new Connection(client, piEndPoint);
                        this.Connections.TryAdd(piEndPoint, connection);
                        this.OnConnected(this, connection);
                    }
                    Thread.Sleep(200);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// �رռ���
        /// </summary>
        public void StopServer()
        {
            _listenConnection.Abort();
            this.EndListenAndSend();
        }
    }
}