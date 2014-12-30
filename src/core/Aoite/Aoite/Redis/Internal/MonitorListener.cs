﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aoite.Redis.Internal
{
    class MonitorListener : RedisListner<object>
    {
        public event EventHandler<RedisMonitorEventArgs> MonitorReceived;

        public MonitorListener(RedisConnection connection)
            : base(connection)
        { }

        public string Start()
        {
            string status = Call(RedisCommand.Monitor());
            Listen(x => x.Read());
            return status;
        }

        protected override void OnParsed(object value)
        {
            OnMonitorReceived(value);
        }

        protected override bool Continue()
        {
            return Connection.Connected;
        }

        void OnMonitorReceived(object message)
        {
            if (MonitorReceived != null)
                MonitorReceived(this, new RedisMonitorEventArgs(message));
        }
    }
}
