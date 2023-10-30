using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

/**
 * Author: Mohammad Homayoon Fayez
 * Zealand - Akademy of Technologies and Business
 * Date: April 2022
 * Copyright 2022 Mohammad Homayoon Fayez (mofa@zealand.dk)
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 **/

namespace SslEchoClient
{
    class SslClient
    {
        private string _targetHost;
        private int _targetPort;
        private TcpClient _clientSocket;
        private Stream _unsecureStream;
        private SslStream _sslStream;
        private bool _leaveInnerStreamOpen = false;
        private bool _checkCertificateRevocation = false;
        private SslProtocols _enabledSSLProtocols = SslProtocols.Tls;
        private X509CertificateCollection _certificateCollection;
        private RemoteCertificateValidationCallback _remoteCertificateValidationCallback;
        private LocalCertificateSelectionCallback _localCertificateSelectionCallback;
        public SslClient(string targetHost, int targetPort)
        {
            _targetHost = targetHost;
            _targetPort = targetPort;
            _certificateCollection = new X509CertificateCollection();
        }

        public bool Connect()
        {
            _clientSocket = new TcpClient(_targetHost, _targetPort);
            _unsecureStream = _clientSocket.GetStream();
            Console.WriteLine("Client Connected");
            return true;
        }

        public bool AuthenticateAsClient()
        {
           _remoteCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);
            _sslStream = new SslStream(_unsecureStream, _leaveInnerStreamOpen, _remoteCertificateValidationCallback);
            _sslStream.AuthenticateAsClient(_targetHost, _certificateCollection, _enabledSSLProtocols, _checkCertificateRevocation);

            return true;
        }


        public void Talk()
        {
            StreamReader sr = new StreamReader(_sslStream);
            StreamWriter sw = new StreamWriter(_sslStream);
            sw.AutoFlush = true; // enable automatic flushing

            Console.WriteLine("\n Now you can talk to the server securly. \n Type \"by\" to quit. Start your WIRESHARK and check the TCP/IP layers (SSL/TLS communication)\n");

            string message = Console.ReadLine();

            while (message != "by")
            {
                sw.WriteLine(message);
                string serverAnswer = sr.ReadLine();
                Console.WriteLine("Server: " + serverAnswer);
                message = Console.ReadLine();
            }

            _sslStream.Close();
            _clientSocket.Close();
        }


        private bool ValidateServerCertificate(object sender, X509Certificate serverCertificate,
X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Server [subject] and CA [issuer] certificate params : " + serverCertificate.ToString());

            Console.WriteLine("SSL Policy errors: " + sslPolicyErrors.ToString());
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                Console.WriteLine("Client validation of server certificate successful.");
                return true;
            }
            Console.WriteLine("Errors in certificate validation:");
            Console.WriteLine(sslPolicyErrors);
            return false;
        }

        private X509Certificate CertificateSelectionCallback(object sender, string targetHost,
X509CertificateCollection localCollection, X509Certificate remoteCertificate, string[]
acceptableIssuers)
        {
            return localCollection[0];
        }
    }
}
