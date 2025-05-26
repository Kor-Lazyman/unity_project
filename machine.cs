using UnityEngine;
using System.Collections.Generic;
using System;
/*
machine_id: machine의 식별번호
parent_line: 최상위 gameobject
modelPath: resource에 저장되어있는 모델의 이름
machine: 생성된 gameobject
set: 머신에 대한 기본 정보를 설정하는 공간
*/

public class machine : ScriptableObject
{
    // 머신의 고유 ID
    public int machine_id;
    // 머신이 속한 라인의 부모 오브젝트
    public GameObject parent_line;
    // 머신의 기본 모델 (Idle 상태)
    public string modelPath_Idle = "Machine_idle";
    // 머신의 작동 중 모델 (Work 상태)
    public string modelPath_work = "Machine_work";
    // 머신의 작동 중 모델 (Work 상태)
    public string modelPath_ready = "Machine_ready";
    // 현재 씬에 배치된 머신 오브젝트
    public GameObject model;
    // Idle 상태의 머신 프리팹 (모델델)
    public GameObject Machine_work;
    public GameObject Machine_idle;
    public GameObject Machine_ready;
    // machine의 작동여부 판단단
    public bool work = false;

    // 머신을 설정하는 함수
    public void set(int id, Vector3 pos, Vector3 Base)
    {
        this.parent_line = parent_line;
        this.machine_id = id;
        this.Machine_idle = Resources.Load<GameObject>(modelPath_Idle);
        this.Machine_work = Resources.Load<GameObject>(modelPath_work);
        this.Machine_ready = Resources.Load<GameObject>(modelPath_ready);
        // 모델 생성
        this.GenerateModel(pos, Base);
    }

    public void GenerateModel(Vector3 pos, Vector3 Base)
    {
        Vector3 position = pos;
        this.model = Instantiate(this.Machine_idle, position, Quaternion.identity);
        this.model.transform.LookAt(new Vector3(pos.x, pos.y, Base.z));
    }
    // 모델을 State에 맞게 적절히 변환
    public void Change_model(string state)
    {
        Vector3 position = this.model.transform.position;
        Quaternion rot = this.model.transform.rotation;
        Destroy(this.model);

        if (state == "Idle")
        {
            this.work = false;
            this.model = Instantiate(this.Machine_idle, position, rot);
        }
        else if (state == "Ready")
        {
            this.work = false;
            this.model = Instantiate(this.Machine_ready, position, rot);
        }
        else if (state == "Work")
        {
            this.work = true;
            this.model = Instantiate(this.Machine_work, position, rot);
        }

    }
}
