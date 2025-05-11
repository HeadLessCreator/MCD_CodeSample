using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 먹을 수 있는 오브젝트의 인터페이스. Player가 먹을 수 있도록 공통 계약 정의.
/// </summary>
public interface IEatable
{
    void OnEaten(int damage);
}