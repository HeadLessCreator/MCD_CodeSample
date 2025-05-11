using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatableDog : EatableBase
{
    //처음 먹힐 때
    protected override void FirstRespond()
    {
        SoundManager.Instance.PlaySound("SFX_DogScream");
        transform.Find("Shadow").gameObject.SetActive(false);
    }

    //터질 때, 두 번째, 애니메이션에서 실행됨
    protected override void SecondRespond()
    {
        //사운드
        SoundManager.Instance.PlaySound("SFX_DogSqueak");

        //VFX Confetti
        int randomFireworksSoundIndex = Random.Range(1, 3);
        SoundManager.Instance.PlaySound($"Confetti_{randomFireworksSoundIndex}");

        var vfxProperties = new VFXProperties();
        vfxProperties.vfxName = "BlueConfettiVFX";
        vfxProperties.vfxPosition = transform.position;
        Debug.Log($"Confetti position: {vfxProperties.vfxPosition}");
        vfxProperties.vfxRotation = onEatenVFXOptions[0].transform.rotation;
        vfxProperties.vfxScale = Vector3.one * 1.1f;
        vfxProperties.vfxPlayTime = 1f;
        VFXManager.Instance.OnVFXPlayed(vfxProperties);
    }
}
