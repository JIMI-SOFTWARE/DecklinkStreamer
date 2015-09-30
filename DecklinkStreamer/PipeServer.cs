using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace DecklinkStreamer
{
    public class PipeServer
    {
        private Thread m_listenerThread = null;
        private volatile bool m_closing = false;
        private volatile List<NamedPipeServerStream> m_pipeStreams = null;
        private const int MAX_NUMBER_OF_INSTANCES = 64;
        private const int BUFFER_SIZE = 10 * 1024;


        public PipeServer(string pipeName)
        {
            m_pipeStreams = new List<NamedPipeServerStream>();
            this.PipeName = pipeName;
            StartListener();
        }

        public string PipeName
        {
            get;
            private set;
        }

        private void StartListener()
        {
            m_closing = false;
            m_listenerThread = new Thread(new ThreadStart(ListenerThreadProc));
            m_listenerThread.Start();
        }

        private void ListenerThreadProc()
        {
            try
            {
                Console.WriteLine(string.Format("pipe server {0} listener started", PipeName));
                while (!m_closing)
                {
                    try
                    {
                        if (m_pipeStreams.Count >= MAX_NUMBER_OF_INSTANCES)
                        {
                            Console.WriteLine(string.Format("number of pipe instances exceeded for {0}", PipeName));
                            Thread.Sleep(5000);
                            continue;
                        }
                        NamedPipeServerStream serverStream = new NamedPipeServerStream(PipeName, PipeDirection.Out, MAX_NUMBER_OF_INSTANCES, PipeTransmissionMode.Byte, PipeOptions.None, BUFFER_SIZE, BUFFER_SIZE);
                        serverStream.WaitForConnection();
                        if (serverStream.IsConnected)
                        {
                            Console.WriteLine(string.Format("client {0} connected to pipe {1}", m_pipeStreams.Count, PipeName));
                            m_pipeStreams.Add(serverStream);
                        }
                        else
                        {
                            throw new Exception("strange thing: pipe.waitForConnection() completed, but there is no connection exists");
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        //exit now
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("ERROR: {0}", ex.ToString()));
                        //no need to throw exception further. just wait for next client
                    }
                }
            }
            catch (ThreadAbortException)
            {
                //do nothing
            }
            finally
            {
                Console.WriteLine(string.Format("pipe server {0} listener stopped", PipeName));
            }
        }

        public void SendPacket(byte[] packet, int offset, int length)
        {
            for (int i = 0; i < m_pipeStreams.Count; i++)
            {
                NamedPipeServerStream stream = m_pipeStreams[i];
                try
                {
                    stream.Write(packet, offset, length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("ERROR: {0}", ex.ToString()));
                    DisconnectClient(stream);
                    Console.WriteLine(string.Format("Failed to write data to {0} instance of pipe {1}. Client disconnected.", i, PipeName));
                }
            }
        }

        private void DisconnectClient(NamedPipeServerStream stream)
        {
            try
            {
                if (stream.IsConnected)
                {
                    stream.Disconnect();
                }
            }
            catch
            {
                //do nothing here
            }
            finally
            {
                //remove the client
                if (m_pipeStreams.Contains(stream))
                {
                    m_pipeStreams.Remove(stream);
                }
            }
        }

        public void Close()
        {
            Console.WriteLine(string.Format("closing pipe server {0}...", PipeName));
            m_closing = true;
            //emulate new client connected to resume listener thread
            using (NamedPipeClientStream fakeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.In))
            {
                try
                {
                    fakeClient.Connect(100);
                    fakeClient.Close();
                }
                catch
                {
                    //server was stopped already
                }
            }
            if (m_listenerThread != null && m_listenerThread.IsAlive)
            {
                if (!m_listenerThread.Join(5000))
                {
                    m_listenerThread.Abort();
                }
            }
            m_listenerThread = null;

            Console.WriteLine(string.Format("disconnecting clients from pipe {0}...", PipeName));
            while (m_pipeStreams.Count > 0)
            {
                DisconnectClient(m_pipeStreams[0]);
            }

            Console.WriteLine(string.Format("Pipe server {0} stopped OK", PipeName));
        }
    }
}
