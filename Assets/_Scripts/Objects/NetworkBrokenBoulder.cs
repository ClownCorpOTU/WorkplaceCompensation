using UnityEngine;
using Fusion;

public class BrokenBoulder : NetworkBehaviour
{
    public struct PieceState : INetworkStruct
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }

    [SerializeField] private float despawnTimer = 5f;

    [Networked, Capacity(20)] private NetworkArray<PieceState> _pieceStates => default;
    [Networked] private TickTimer _lifeTimer { get; set; }

    private Rigidbody[] _childRBs;
    private MeshRenderer[] _renderers;
    private MaterialPropertyBlock _propBlock;
    private static readonly int _baseColorID = Shader.PropertyToID("_BaseColor");

    
    public override void Spawned()
    {
        _childRBs = GetComponentsInChildren<Rigidbody>();
        _renderers = GetComponentsInChildren<MeshRenderer>();
        _propBlock = new MaterialPropertyBlock();

        if (Object.HasStateAuthority)
        {
            _lifeTimer = TickTimer.CreateFromSeconds(Runner, despawnTimer);
        }
        else
        {
            foreach (var rb in _childRBs) rb.isKinematic = true;
        }
    }

    public void ApplyInitialExplosion(Vector3 origin, float force, float radius)
    {
        if (!Object.HasStateAuthority) return;
        foreach (var rb in _childRBs) rb.AddExplosionForce(force, origin, radius, 1.5f);
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            for (int i = 0; i < _childRBs.Length; i++)
            {
                _pieceStates.Set(i, new PieceState {
                    Position = _childRBs[i].transform.localPosition,
                    Rotation = _childRBs[i].transform.localRotation
                });
            }
            
            if (_lifeTimer.Expired(Runner)) Runner.Despawn(Object);
        }
    }

    public override void Render()
    {
        // 1. Sync Positions
        if (!Object.HasStateAuthority)
        {
            for (int i = 0; i < _childRBs.Length; i++)
            {
                _childRBs[i].transform.localPosition = _pieceStates[i].Position;
                _childRBs[i].transform.localRotation = _pieceStates[i].Rotation;
            }
        }
    }
}