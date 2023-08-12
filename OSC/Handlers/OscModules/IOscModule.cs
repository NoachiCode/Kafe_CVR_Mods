﻿using MelonLoader;
using Rug.Osc.Core;

namespace Kafe.OSC.Handlers.OscModules;

public abstract class OscHandler {
    internal abstract void Enable();
    internal abstract void Disable();

    internal virtual void ReceiveMessageHandler(OscMessage oscMsg) {
        MelonLogger.Msg("[Info] You attempted to send a message to a module that doesn't not allow receiving. " +
                        $"Address Attempted: {oscMsg.Address} I hope you are sending these on a loop, because I TOLD you" +
                        $"to not send to this endpoint on the docs :eyes_rolling: git spammed x)");
    }
}
