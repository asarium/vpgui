using System;
using System.Collections.Generic;
using System.Linq;
using VPSharp;

namespace VPGUI.Models
{
    public class VPMessagesViewModel
    {
        private VPFileMessages _messages;

        public VPMessagesViewModel(VPFileMessages messages)
        {
            this._messages = messages;

            var msgs = messages.Messages.Select(msg => new VPFileMessageView(msg));

            this.Messages = msgs.ToList();
        }

        public List<VPFileMessageView> Messages { get; internal set; }
    }
}
