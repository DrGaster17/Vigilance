using System;

using System.IO;

using System.Net;
using System.Net.Sockets;

using System.Text;

using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Vigilance.Discord.Networking
{
    public abstract class NetworkClient : IDisposable
    {
        public const int ReceptionBuffer = 8192;

        private bool isDisposed;

        public NetworkClient()
            : this(null)
        {
        }

        public NetworkClient(TimeSpan reconnectionInterval)
            : this(null, reconnectionInterval)
        {
        }

        public NetworkClient(string ipAddress, ushort port)
            : this(new IPEndPoint(IPAddress.Parse(ipAddress), port))
        {
        }

        public NetworkClient(string ipAddress, ushort port, TimeSpan reconnectionInterval)
            : this(new IPEndPoint(IPAddress.Parse(ipAddress), port), reconnectionInterval)
        {
        }

        public NetworkClient(IPEndPoint ipEndPoint)
            : this(ipEndPoint, TimeSpan.FromSeconds(5))
        {
        }

        public NetworkClient(IPEndPoint ipEndPoint, TimeSpan reconnectionInterval)
        {
            IPEndPoint = ipEndPoint;
            ReconnectionInterval = reconnectionInterval;
        }

        ~NetworkClient() => Dispose(false);

        public abstract void OnReceivePartial(string data, int length);
        public abstract void OnReceiveFull(string data, int length);
        public abstract void OnSendError(Exception exception);
        public abstract void OnReceiveError(Exception exception);
        public abstract void OnDataSent(string data, int length);
        public abstract void OnConnecting(IPAddress iPAddress, ushort port, TimeSpan reconnectionInterval);
        public abstract void OnConnectionError(Exception exception);
        public abstract void OnConnected();
        public abstract void OnConnectionUpdateError(Exception exception);
        public abstract void OnConnectionTerminated(Task task);

        public TcpClient TcpClient { get; private set; }
        public IPEndPoint IPEndPoint { get; private set; }
        public bool IsConnected => TcpClient?.Connected ?? false;
        public TimeSpan ReconnectionInterval { get; private set; }

        public JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.Objects,
        };

        public async Task Start() => await Start(CancellationToken.None);

        public async Task Start(CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName);

            await Update(cancellationToken).ContinueWith(task => OnConnectionTerminated(task)).ConfigureAwait(false);
        }

        public void Close() => Dispose();

        public async ValueTask SendAsync<T>(T data) => await SendAsync(data, CancellationToken.None);

        public async ValueTask SendAsync<T>(T data, CancellationToken cancellationToken)
        {
            try
            {
                if (!IsConnected)
                    return;

                string serializedObject = JsonConvert.SerializeObject(data, JsonSerializerSettings);

                byte[] bytesToSend = Encoding.UTF8.GetBytes(serializedObject + '\0');

                await TcpClient.GetStream().WriteAsync(bytesToSend, 0, bytesToSend.Length, cancellationToken);

                OnDataSent(serializedObject, bytesToSend.Length);
            }
            catch (Exception exception) when (exception.GetType() != typeof(OperationCanceledException))
            {
                OnSendError(exception);
            }
        }


        public void Dispose() => Dispose(true);


        protected virtual void Dispose(bool shouldDisposeAllResources)
        {
            if (shouldDisposeAllResources)
            {
                TcpClient.Dispose();
                TcpClient = null;

                IPEndPoint = null;
            }

            isDisposed = true;

            GC.SuppressFinalize(this);
        }

        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            StringBuilder totalReceivedData = new StringBuilder();
            byte[] buffer = new byte[ReceptionBuffer];

            while (true)
            {
                Task<int> readTask = TcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                await Task.WhenAny(readTask, Task.Delay(Timeout.Infinite, cancellationToken));

                cancellationToken.ThrowIfCancellationRequested();

                int bytesRead = await readTask;

                if (bytesRead > 0)
                {
                    string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (receivedData.IndexOf('\0') != -1)
                    {
                        foreach (var splittedData in receivedData.Split('\0'))
                        {
                            if (totalReceivedData.Length > 0)
                            {
                                OnReceiveFull(totalReceivedData.ToString() + splittedData, bytesRead);

                                totalReceivedData.Clear();
                            }
                            else if (!string.IsNullOrEmpty(splittedData))
                            {
                                OnReceiveFull(splittedData, bytesRead);
                            }
                        }
                    }
                    else
                    {
                        OnReceivePartial(receivedData, bytesRead);

                        totalReceivedData.Append(receivedData);
                    }
                }

                if (bytesRead == 0 && totalReceivedData.Length > 1)
                    OnReceiveFull(totalReceivedData.ToString(), bytesRead);
            }
        }

        private async Task Update(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    OnConnecting(IPEndPoint.Address, (ushort)IPEndPoint.Port, ReconnectionInterval);

                    TcpClient = new TcpClient();

                    await TcpClient.ConnectAsync(IPEndPoint.Address, IPEndPoint.Port);

                    OnConnected();

                    await ReceiveAsync(cancellationToken);
                }
                catch (IOException ioException)
                {
                    OnReceiveError(ioException);
                }
                catch (SocketException socketException) when (socketException.ErrorCode == 10061)
                {
                    OnReceiveError(socketException);
                }
                catch (Exception exception) when (exception.GetType() != typeof(OperationCanceledException))
                {
                    OnReceiveError(exception);
                }

                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(ReconnectionInterval, cancellationToken);
            }
        }
    }
}