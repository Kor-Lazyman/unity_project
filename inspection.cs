using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
/*
inspection_id: 해당 inspector의 식별번호(1~20)
modelPath: resource에 저장된 3d 모델의 이름
parent_line: 모델이 생성될 위치의 최상위 gameobject
current_line: 생성된 모델이 위치한 gameobject
inspect: 불러온 모델이 저장 될 공간
model: 현재 출력되고 있는 모델
route: 모델이 이동할 위치를 저장할 공간
current_destination: 현재 이동하고 있는 모델의 목표점
wait_place: 작업을 진행하지 않을 경우 대기할 위치
work: 작업 여부를 보여주는 변수
set: inspection의 instance에 데이터를 설정하는 과정
set_route: 앞으로 이동 할 목표점의 좌표를 설정
GenerateModel: 모델을 처음 생성할 때 사용 하는 함수
_change_model: 모델의 상태(work, idle, wait)를 전환하는 함수
input_order: 모델을 idle상태에서  work 상태로 전환을 명령
Move: 목표좌표로 이동을 명령(직선이동)
wait_work/end_work: 도착지에 도달 했을 경우, 알맞은 모델(idle/wait)로 전환
*/
public class inspection : ScriptableObject
{
    public int inspection_id;
    public Camera inspectionCamera;
    public string modelPath_idle = "Banana Man"; 
    public string modelPath_wait = "Banana Man";
    public string modelPath_work = "Banana Man"; 

    public GameObject parent_line;
    public GameObject current_line;

    public GameObject inspect_work;
    public GameObject inspect_wait; 
    public GameObject inspect_idle; 
    public GameObject model;

    public Queue<Vector3> route;
    public Vector3 current_desitnation;
    public Vector3 wait_place = new Vector3(); 
    public float elapsed_time = 0.01f;
    public float speed = 1f;

    public bool work = false;
    
    public void set(int inspection_id)
    {   
        this.inspect_idle = Resources.Load<GameObject>(modelPath_idle);
        this.inspect_wait = Resources.Load<GameObject>(modelPath_wait);
        this.inspect_work = Resources.Load<GameObject>(modelPath_work);
        this.inspection_id = inspection_id;

        if(inspection_id<=10)
        {
            this.current_line = this.parent_line.transform.Find($"inspect{this.inspection_id}")?.gameObject;
        }
        else
        {
            this.current_line = this.parent_line.transform.Find($"inspect{this.inspection_id/2}")?.gameObject;
        }
        

        if (this.inspect_idle == null && this.inspect_work == null) 
        {
            Debug.LogError("모델 inspect_idle와 inspect_work를 Assets/resources에서 load 실패");
        }
        else if (this.inspect_idle == null)
        {
            Debug.LogError("모델 inspect_idle을 Assets/resources에서 load 실패");
        }
        else if (this.inspect_work == null)
        {
            Debug.LogError("모델 inspect_work를를 Assets/resources에서 load 실패");
        }
        
        if (inspection_id<=10)
        {
            this.GenerateModelAtlineLeft();
        }
        else
        {
            this.GenerateModelAtlineRight();
        }
    }
    // log에서 agv가 이동한 경로
    public void set_route(Queue<Vector3> route)
    {
        this.route = route;
        this.current_desitnation =  this.route.Dequeue();
    }
    public void GenerateModelAtlineLeft()
    {
        //  Line 1의 가운데를 기준으로 생성
        Vector3 position = this.current_line.transform.position;// 현재 위치(global)을 측정
        position.x = -(11 - 2*this.inspection_id); // 각 Inspect 사이의 거리는 2(Inspect 중심을 기준으로)
        position.y = 0;
        position.z -= 5;// (Line 1의 중앙을 기준으로 가로로 5만큼 떨어지게 만듬듬)
        this.wait_place = position;
        this.model = Instantiate(this.inspect_idle, position, Quaternion.identity);// 현재 위치에 모델을 생성 *Quaternion은 3차원 회전 정보
        this.inspectionCamera = this.model.GetComponentInChildren<Camera>(true);

    }
    public void GenerateModelAtlineRight()
    {
        Vector3 position = this.current_line.transform.position;// 현재 위치(global)을 측정
        position.x = -(11 - 2*(this.inspection_id-10)); // 각 Inspect 사이의 거리는 2(Inspect 중심을 기준으로)
        position.y = 0;
        position.z += 5; // (Line 1의 중앙을 기준으로 Z푹 5만큼 떨어지게 만듬듬)
        this.wait_place = position;
        this.model = Instantiate(this.inspect_idle, position, Quaternion.identity);
        this.inspectionCamera = this.model.GetComponentInChildren<Camera>(true);
       

    }
    public void _Change_model(string model_state)
    {
       
        Vector3 targetPosition = this.model.transform.position;
        Destroy(this.model);// 기존 모델 삭제
        // 작업 상태로 전환
        if (model_state == "work")
        {
            this.work = true;
            this.model = Instantiate(this.inspect_work, targetPosition, Quaternion.identity);

        }
        // 휴식 상태로 전환
        else if(model_state == "idle")
        {
            this.model = Instantiate(this.inspect_idle, targetPosition, Quaternion.identity);
        }

        // 대기 상태로 전환
        else if(model_state == "wait")
        {
            this.model = Instantiate(this.inspect_wait, targetPosition, Quaternion.identity);
        }
        this.inspectionCamera = this.model.GetComponentInChildren<Camera>(true);

    }
    // Order가 들어 올 경우 model을 만들거나 상태를 변환 시킴
    public void Input_Order()
    {
        // 모델이 생성되어 있는 상태라면 Work로 전환
        this._Change_model("work");
    }
    // Start_place: 출발 
    public void Move()
    {
        // 남은 목적지가 남아있다면 
        if (this.route.Count>0)
        {
            // 현재 위치와 목적지가 같다면 목적지 변경
            if(this.model.transform.position == this.current_desitnation)
            {
                // 다음 목적지를 큐에서 추출
                this.current_desitnation =  this.route.Dequeue();
                this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, this.current_desitnation, this.speed*this.elapsed_time );
            }
            else   
            {
                this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, this.current_desitnation, this.speed*this.elapsed_time );
            }

        }

        // 마지막 이동이라면
        else
        {
            // 작업 종료
            if(this.model.transform.position == this.current_desitnation)
            {
                this.work = false;
                if(this.wait_place == this.current_desitnation)
                {
                    this.End_Work();
                }
                
            }
            else
            {
                this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, this.current_desitnation, this.speed*this.elapsed_time );
            } 
        }

        
    }
    // 일이 끝난 경우 Idle로 상태를 전환
    public void wait_work()
    {
     this._Change_model("wait");
    }

    public void End_Work()
    {
     this._Change_model("idle");
    }
}