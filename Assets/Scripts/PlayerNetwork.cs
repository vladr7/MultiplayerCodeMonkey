using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;
    
    public struct PlayerData : INetworkSerializable
    {
        public int _hp;
        public bool _isDead;
        public FixedString128Bytes message;
        

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _hp);
            serializer.SerializeValue(ref _isDead);
            serializer.SerializeValue(ref message);
        }
    }

    private NetworkVariable<int> score = new NetworkVariable<int>(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private NetworkVariable<PlayerData> playerData = new NetworkVariable<PlayerData>(
        new PlayerData
        {
            _hp = 100,
            _isDead = false,
            message = "Hello World"
        },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        score.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + " ; score: " + score.Value);
        };
        playerData.OnValueChanged += (PlayerData previousValue, PlayerData newValue) =>
        {
            Debug.Log(OwnerClientId + " ; hp: " + playerData.Value._hp
                      + " ; isDead: " + playerData.Value._isDead +
                      " ; message: " + playerData.Value.message);
        };
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        Vector3 moveDir = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W))
        {
            moveDir += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveDir += Vector3.back;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveDir += Vector3.left;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDir += Vector3.right;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            // score.Value++;
            // TestServerRpc(message:"Message testing ServerRpc");
            // TestClientRpc(new ClientRpcParams
            // {
            //     Send = new ClientRpcSendParams
            //     {
            //         TargetClientIds = new List<ulong> { 1 }
            //     }
            // });
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
        }
        if(Input.GetKeyDown(KeyCode.X))
        {
            spawnedObjectTransform.GetComponent<NetworkObject>().Despawn(true);
            Destroy(spawnedObjectTransform.gameObject);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            playerData.Value = new PlayerData
            {
                _hp = 50,
                _isDead = true,
                message = "Changed the World!"
            };
        }

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void TestServerRpc(string message)
    {
        Debug.Log("TestServerRpc " + OwnerClientId + " ; message: " + message);
    }

    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log("TestClientRpc " + OwnerClientId);
    }
}