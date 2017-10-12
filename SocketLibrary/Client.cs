using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SocketLibrary
{
    /// <summary>
    /// Socket的客户端
    /// </summary>
    public class Client : SocketBase
    {
        /// <summary>
        ///  超时时间
        /// </summary>
        public const int CONNECTTIMEOUT = 10;
        private TcpClient client;
        private IPAddress ipAddress;
        private int port;
        private Thread _listenningClientThread;
        private string _clientName;
        /// <summary>
        /// 连接的Key
        /// </summary>
        public string ClientName
        {
            get { return _clientName; }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ipaddress">地址</param>
        /// <param name="port">端口</param>
        public Client(string ipaddress, int port)
            : this(IPAddress.Parse(ipaddress), port)
        {
        }
        /// <summary>
        ///初始化 
        /// </summary>
        /// <param name="ipaddress">地址</param>
        /// <param name="port">端口</param>
        public Client(IPAddress ipaddress, int port)
        {
            this.ipAddress = ipaddress;
            this.port = port;
            this._clientName = ipAddress + ":" + port;
        }

        /// <summary>
        /// 打开链接
        /// </summary>
        public void StartClient()
        {
            this.StartListenAndSend(true);//开启父类的监听线程
            _listenningClientThread = new Thread(new ThreadStart(Start));
            _listenningClientThread.Start();
        }
        /// <summary>
        /// 关闭连接并释放资源
        /// </summary>
        public void StopClient()
        {
            //缺少通知给服务端 自己主动关闭了
            _listenningClientThread.Abort();
            this.EndListenAndSend();
        }
        /// <summary>
        /// 获取连接集合
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, Connection> GetConnections()
        {
            return this.Connections;
        }
        /// <summary>
        /// 获取指定连接名的连接,查询不到返回null
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public Connection GetConnection(string connectionName)
        {
            Connection connection;
            this.Connections.TryGetValue(connectionName, out connection);
            return connection;
        }

        private void Start()
        {
            while (true)
            {
                if (!this.Connections.ContainsKey(this._clientName))
                {
                    try
                    {
                        client = new TcpClient();
                        client.SendTimeout = CONNECTTIMEOUT;
                        client.ReceiveTimeout = CONNECTTIMEOUT;
                        client.Connect(ipAddress, port);
                        this.Connections.TryAdd(this._clientName, new Connection(client, this._clientName));
                    }
                    catch (Exception e)
                    { //定义连接失败事件
                    }
                }
                Thread.Sleep(200);
            }
        }
    }
}
