using System;
using UnityEngine;

// SFX 타입과 오디오 클립 매핑 데이터
[Serializable]
public class SfxClipData
{
    public SfxType type;     // 효과음 종류
    public AudioClip clip;   // 오디오 클립
}