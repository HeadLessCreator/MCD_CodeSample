using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatableCivilian : EatableBase
{
    //처음 먹힐 때 - 시민, 경찰 현재 로직 같음
    protected override void FirstRespond()
    {
        //비명소리 사운드
        string[] dyingScreams;

        if (isMale)
        {
            dyingScreams = new string[4] {
                        "Scream_Male1", "Scream_Male2",
                        "Scream_Male3", "Scream_Male4"
                        };
        }
        else
        {
            dyingScreams = new string[4] {
                        "Scream_Female1", "Scream_Female2",
                        "Scream_Female3", "Scream_Female4"
                        };
        }

        string dyingScream = dyingScreams[Random.Range(0, dyingScreams.Length)];
        SoundManager.Instance.PlaySound(dyingScream);

        //그림자 제거
        Transform shadowTr = transform.Find("Shadow");
        if (shadowTr != null)
        {
            shadowTr.gameObject.SetActive(false);
        }

        //이벤트 - 퀘스트, 미션, 업적 기록
        OnHumanKilled.Invoke();
    }

    //터질 때, 두 번째, 애니메이션에서 실행됨
    protected override void SecondRespond()
    {
        int vfxIndex = 0;

        int randomFireworksSoundIndex = Random.Range(1, 3);
        SoundManager.Instance.PlaySound($"Confetti_{randomFireworksSoundIndex}");

        var vfxProperties = new VFXProperties();
        vfxProperties.vfxName = "ConfettiVFX";
        vfxProperties.vfxPosition = transform.position;
        Debug.Log($"Confetti position: {vfxProperties.vfxPosition}");
        vfxProperties.vfxRotation = onEatenVFXOptions[vfxIndex].transform.rotation;
        vfxProperties.vfxScale = onEatenVFXOptions[vfxIndex].transform.localScale;
        vfxProperties.vfxPlayTime = 1f;
        VFXManager.Instance.OnVFXPlayed(vfxProperties);
    }
}
