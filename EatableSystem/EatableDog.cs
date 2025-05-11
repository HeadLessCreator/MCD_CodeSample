using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatableDog : EatableBase
{
    //ó�� ���� ��
    protected override void FirstRespond()
    {
        SoundManager.Instance.PlaySound("SFX_DogScream");
        transform.Find("Shadow").gameObject.SetActive(false);
    }

    //���� ��, �� ��°, �ִϸ��̼ǿ��� �����
    protected override void SecondRespond()
    {
        //����
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
