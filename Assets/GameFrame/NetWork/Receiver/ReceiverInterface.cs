using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public interface ReceiverInterface
{
    void Receiver(byte subCode, OperationResponse response);
}
