using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    namespace Message
    {
        public enum MessageType
        {
            DAMAGED,
            DEAD,
            //Add your user defined message type after
        }

        public interface IMessageReceiver
        {
            void OnReceiveMessage(MessageType type, object sender, object msg);
        }
    }
}
