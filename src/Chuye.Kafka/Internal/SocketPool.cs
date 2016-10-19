﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Kafka.Internal {
    class SocketPool : ObjectPool<Socket> {
        private readonly Uri _uri;

        public SocketPool(Uri uri) {
            _uri = uri;
        }

        protected override Socket Constructing() {
            var socket = new ReusableSocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, this);
            socket.Connect(_uri.Host, _uri.Port);
            return socket;
        }

        public override Socket AcquireItem() {
            var socket = base.AcquireItem();
            while (!IsConnected(socket)) {
                DetachItem(socket);
                socket = AcquireItem();
                socket.Connect(_uri.Host, _uri.Port);
            }
            return socket;
        }

        //http://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        private bool IsConnected(Socket socket) {
            return !((socket.Poll(100, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
        }

        protected override void OnItemDetached(Socket item) {
            ((ReusableSocket)item).Destroy();
        }

        class ReusableSocket : Socket {
            private readonly SocketPool _pool;

            public ReusableSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, SocketPool pool)
                : base(addressFamily, socketType, protocolType) {
                _pool = pool;
            }

            protected override void Dispose(bool disposing) {
                // NOT dispose, wait for ObjectPool's DetachItem()
                //base.Dispose(disposing); 
                _pool.ReturnItem(this);
            }

            public void Destroy() {
                base.Dispose(true);
            }
        }
    }

}
