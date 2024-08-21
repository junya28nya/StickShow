using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempoManager : MonoBehaviour
{
    [SerializeField]
    float tempo = 120;


    // タップテンポのリセット時間。実用テンポより遅かったらリセットする。
    [SerializeField]
    float tapTempoResetTime = 2.0f;

    [SerializeField]
    GameObject stickObject;
    StickShow testStick;

    // 何回タップでテンポを推定するか。2回だと揺れの影響が大きいから3くらい
    // 2以上の値とする。tapTempo()で割り算の分母が0になってしまうため。
    int calcTapTempoCount = 3;

    List<float> tapTimeList = new List<float>();

    void Start(){
        testStick = stickObject.GetComponent<StickShow>();
    }

    public void tapTempo(){

        // 最後のタップからtapTempoResetTime秒以上経過していたら、タップテンポのカウントをリセットする。
        if(tapTimeList.Count > 0 && Time.time - tapTimeList[tapTimeList.Count - 1] >= tapTempoResetTime){
            tapTimeList.Clear();
        }
        tapTimeList.Add(Time.time);
        if(tapTimeList.Count >= calcTapTempoCount){
            float sum = 0;
            for(int i = 0; i < tapTimeList.Count -1; i++){
                sum += tapTimeList[i + 1] - tapTimeList[i];
            }
            tempo = 60 / (sum / (tapTimeList.Count - 1));
            int tempoInt = (int)tempo;
            testStick.changeTempo(tempoInt, 100);
            Debug.Log("tempo: " + tempo);
            // 常に最新のCalcTapTempoCount個数のタップ時間で計算する。
            tapTimeList.RemoveAt(0);
        }
    }
}
