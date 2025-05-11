using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatableCar : EatableBase
{
    // 현재는 자동차, 경찰차, 군용기 로직 같음

    //처음 먹힐 때
    protected override void FirstRespond()
    {
        string eatingSFXName = "Bite_Car";
        SoundManager.Instance.PlaySound(eatingSFXName);

        CarController carCon = this.GetComponent<CarController>();
        carCon.Stop();
        carCon.enabled = false;
        carCon.isGettingDestroyed = true;

        OnVehicleDestroyed.Invoke();
    }

    //터질 때, 두 번째, 애니메이션에서 실행됨
    protected override void SecondRespond()
    {
        //사운드
        SoundManager.Instance.PlaySound("Swallow_Car");

        //콘페티
        int num = Random.Range(0, onEatenVFXOptions.Length);
        var vfxProperties = new VFXProperties();
        vfxProperties.vfxName = onEatenVFXOptions[num].name;
        vfxProperties.vfxPosition = transform.position;
        if (vfxProperties.vfxName == "LiquidMuzzleOil")
        {
            Quaternion randomRotation = Quaternion.Euler(new Vector3(0f, Random.Range(0, 360), 0f));
            vfxProperties.vfxRotation = randomRotation;
        }
        else
        {
            vfxProperties.vfxRotation = onEatenVFXOptions[num].transform.rotation;
        }
        vfxProperties.vfxScale = onEatenVFXOptions[num].transform.localScale;
        vfxProperties.vfxPlayTime = 1f;
        VFXManager.Instance.OnVFXPlayed(vfxProperties);
    }
}
