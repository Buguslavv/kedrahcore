using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Kedrah.Util {
    public class WebServer {
        private TcpListener tcpListener;
        private int port;
        private string directory;

        public WebServer(string directory) {
            this.directory = directory;
            
            for (port = 1000; port <= 10000; port++) {
                try {
                    tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
                    tcpListener.Start();
                    break;
                }
                catch {
                    continue;
                }
            }

            Thread thread = new Thread(new ThreadStart(StartListen));
            thread.Start();
        }

        public int Port {
            get {
                return port;
            }
            set {
                ;
            }
        }

        private static string MimeType(string Filename) {
            string mime = "application/octetstream";
            string ext = System.IO.Path.GetExtension(Filename).ToLower();
            Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (rk != null && rk.GetValue("Content Type") != null)
                mime = rk.GetValue("Content Type").ToString();
            return mime;
        } 

        public void SendHeader(string sHttpVersion, string sMIMEHeader, int iTotBytes, string sStatusCode, ref Socket socket) {
            String sBuffer = "";

            if (sMIMEHeader.Length == 0) {
                sMIMEHeader = "text/html";
            }

            sBuffer = sBuffer + sHttpVersion + sStatusCode + "\r\n";
            sBuffer = sBuffer + "Server: cx1193719-b\r\n";
            sBuffer = sBuffer + "Content-Type: " + sMIMEHeader + "\r\n";
            sBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
            sBuffer = sBuffer + "Content-Length: " + iTotBytes + "\r\n\r\n";

            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);

            SendToBrowser(bSendData, ref socket);
        }

        public void SendToBrowser(String sData, ref Socket socket) {
            SendToBrowser(Encoding.ASCII.GetBytes(sData), ref socket);
        }

        public void SendToBrowser(Byte[] bSendData, ref Socket socket) {
            if (socket.Connected)
                socket.Send(bSendData, bSendData.Length, 0);
        }

        public void StartListen() {
            int iStartPos = 0;
            String sRequest;
            String sDirName;
            String sRequestedFile;
            String sErrorMessage;
            String sLocalDir;
            String sMyWebServerRoot = "web\\" + directory + "\\";
            String sPhysicalFilePath = "";
            String sResponse = "";

            while (true) {
                Socket socket = tcpListener.AcceptSocket();

                if (socket.Connected) {
                    Byte[] bReceive = new Byte[1024];
                    int i = socket.Receive(bReceive, bReceive.Length, 0);
                    string sBuffer = Encoding.ASCII.GetString(bReceive);

                    if (sBuffer.Substring(0, 3) != "GET") {
                        socket.Close();
                        return;
                    }

                    iStartPos = sBuffer.IndexOf("HTTP", 1);
                    string sHttpVersion = sBuffer.Substring(iStartPos, 8);
                    sRequest = sBuffer.Substring(0, iStartPos - 1);
                    sRequest.Replace("\\", "/");

                    if ((sRequest.IndexOf(".") < 1) && (!sRequest.EndsWith("/"))) {
                        sRequest = sRequest + "/";
                    }

                    iStartPos = sRequest.LastIndexOf("/") + 1;
                    sRequestedFile = sRequest.Substring(iStartPos);
                    sDirName = sRequest.Substring(sRequest.IndexOf("/"), sRequest.LastIndexOf("/") - 3);

                    if (sDirName == "/")
                        sLocalDir = sMyWebServerRoot;
                    else {
                        sLocalDir = sMyWebServerRoot + sDirName.Replace('/', '\\');
                    }

                    if (sLocalDir.Length == 0) {
                        sErrorMessage = "<h2>The requested directory does not exist.</h2>";
                        SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", ref socket);
                        SendToBrowser(sErrorMessage, ref socket);
                        socket.Close();
                        continue;
                    }

                    if (sRequestedFile.Length == 0) {
                        sRequestedFile = "index.html";
                    }

                    sPhysicalFilePath = sLocalDir + sRequestedFile;

                    if (File.Exists(sPhysicalFilePath) == false) {
                        sErrorMessage = "<h2>The requested file does not exist.</h2>";
                        SendHeader(sHttpVersion, "", sErrorMessage.Length, " 404 Not Found", ref socket);
                        SendToBrowser(sErrorMessage, ref socket);
                    }
                    else {
                        int iTotBytes = 0;
                        sResponse = "";
                        FileStream fs = new FileStream(sPhysicalFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        BinaryReader reader = new BinaryReader(fs);
                        byte[] bytes = new byte[fs.Length];
                        int read;
                        while ((read = reader.Read(bytes, 0, bytes.Length)) != 0) {
                            sResponse = sResponse + Encoding.ASCII.GetString(bytes, 0, read);
                            iTotBytes = iTotBytes + read;
                        }
                        reader.Close();
                        fs.Close();
                        SendHeader(sHttpVersion, MimeType(sPhysicalFilePath), iTotBytes, " 200 OK", ref socket);
                        SendToBrowser(bytes, ref socket);
                    }
                    socket.Close();
                }
            }
        }
    }
}
