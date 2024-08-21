using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;
using Unity.Collections;
using Unity.Jobs;

sealed class StickShow : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] Mesh _mesh = null;
    [SerializeField] Material _material = null;
    [SerializeField] Audience _audience = Audience.Default();

    [SerializeField]
    private int tempo = 120;
    [SerializeField]
    GameObject tempoTextGo;
    Text tempoText;

    private int nextTempo = 120;
    
    // テンポ変更後の、次の拍を打つ時間
    private float changeTempoNextBeatTime = 0;
    // 前回の拍を打った時間
    private float lastBeatTime = 0;
    // 次の拍を打つ時間
    private float nextBeatTime = 0;

    #endregion

    #region Private objects

    NativeArray<Matrix4x4> _matrices;
    NativeArray<Color> _colors;
    GraphicsBuffer _colorBuffer;
    MaterialPropertyBlock _matProps;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _matrices = new NativeArray<Matrix4x4>
          (_audience.TotalSeatCount, Allocator.Persistent,
           NativeArrayOptions.UninitializedMemory);

        _colors = new NativeArray<Color>
          (_audience.TotalSeatCount, Allocator.Persistent,
           NativeArrayOptions.UninitializedMemory);

        _colorBuffer = new GraphicsBuffer
          (GraphicsBuffer.Target.Structured,
           _audience.TotalSeatCount, sizeof(float) * 4);

        _matProps = new MaterialPropertyBlock();

        tempoText = tempoTextGo.GetComponent<Text>();
        MyStart();
    }

    void MyStart(){
        lastBeatTime = Time.time;
        // tempo2拍分で棒が往復するので、2で割っている。
        nextBeatTime = Time.time + (60.0f / (tempo / 2));
        nextTempo = tempo;
        Debug.Log("tempo: " + tempo);
        Debug.Log("lastBeatTime: " + lastBeatTime);
        Debug.Log("nextBeatTime: " + nextBeatTime);
    }

    void OnDestroy()
    {
        _matrices.Dispose();
        _colors.Dispose();
        _colorBuffer.Dispose();
    }

    void Update()
    {
        Profiler.BeginSample("Stick Update");

        /*
        var job = new AudienceAnimationJob()
          { config = _audience, xform = transform.localToWorldMatrix,
            time = Time.time, matrices = _matrices, colors = _colors };
        job.Schedule(_audience.TotalSeatCount, 64).Complete();
*/

      if(Time.time >= nextBeatTime){
            tempo = nextTempo;
            lastBeatTime = nextBeatTime;
            // tempo2拍分で棒が往復するので、2で割っている。
            nextBeatTime = Time.time + (60.0f / (tempo/2));
        }
        float timePosition = (Time.time - lastBeatTime) / (nextBeatTime - lastBeatTime);
        float phase = (timePosition -0.5f) * 2;

        var job = new AudienceAnimationJobWithPhase()
        {config = _audience, xform = transform.localToWorldMatrix,
            time = Time.time, phase = phase, matrices = _matrices, colors = _colors };
        job.Schedule(_audience.TotalSeatCount, 64).Complete();

        Profiler.EndSample();

        _colorBuffer.SetData(_colors);
        _material.SetBuffer("_InstanceColorBuffer", _colorBuffer);

        var rparams = new RenderParams(_material) { matProps = _matProps };
        var (i, step) = (0, _audience.BlockSeatCount);
        for (var sx = 0; sx < _audience.blockCount.x; sx++)
        {
            for (var sy = 0; sy < _audience.blockCount.y; sy++, i += step)
            {
                _matProps.SetInteger("_InstanceIDOffset", i);
                Graphics.RenderMeshInstanced
                  (rparams, _mesh, 0, _matrices, step, i);
            }
        }
    }
    public void changeTempo(int tempo, float changeTempoNextBeatTime){
        this.nextTempo = tempo;
        this.changeTempoNextBeatTime = changeTempoNextBeatTime;
        tempoText.text = "Tempo: " + tempo;
    }

    #endregion
}
