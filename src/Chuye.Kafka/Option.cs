﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chuye.Kafka {
    public class Option {
        private Uri[] _brokerUris;
        private ConnectionFactory _factory;

        public IReadOnlyList<Uri> BrokerUris {
            get { return _brokerUris; }
        }

        public Option(params Uri[] brokerUris) {
            _brokerUris = brokerUris;
        }

        public ConnectionFactory GetConnectionFactory() {
            if (_factory != null) {
                return _factory;
            }

            Interlocked.CompareExchange(ref _factory, new ConnectionFactory(), null);
            return _factory;
        }
    }
}
