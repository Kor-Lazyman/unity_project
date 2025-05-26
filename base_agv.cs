using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
/*
modelPath: resource에 저장된 3d 모델의 이름
parent_line: 모델이 생성될 위치의 최상위 gameobject
current_line: 생성된 모델이 위치한 gameobject
inspect: 불러온 모델이 저장 될 공간
model: 현재 출력되고 있는 모델
route: 모델이 이동할 위치를 저장할 공간
current_destination: 현재 이동하고 있는 모델의 목표점
wait_place: 작업을 진행하지 않을 경우 대기할 위치
work: 작업 여부를 보여주는 변수
set: agv의 instance에 데이터를 설정하는 과정
set_route: 앞으로 이동 할 목표점의 좌표를 설정
GenerateModel: 모델을 처음 생성할 때 사용 하는 함수
_change_model: 모델의 상태(work, idle, wait)를 전환하는 함수
input_order: 모델을 idle상태에서  work 상태로 전환을 명령
Move: 목표좌표로 이동을 명령(직선이동)
wait_work/end_work: 도착지에 도달 했을 경우, 알맞은 모델(idle/wait)로 전환
*/
public class base_agv : ScriptableObject
{
    public Camera agvCamera; // AGV에 달려있는 카메라 지정
    public string modelPath_idle = "idle_agv"; // 불러올 모델 파일 경로
    public string modelPath_wait = "pallet_agv";
    public string modelPath_work = "box_agv"; // 불러올 모델 파일 경로
    public GameObject agv_work; // work 모델 저장
    public GameObject agv_wait; // wait 모델 저장
    public GameObject agv_idle; // idle 모델 저장

    public GameObject model; // 현재 출력되고 있는 모델

    public Queue<Vector3> route; // 이동할 경로를 저장

    public Vector3 current_destination;
    public Vector3 wait_place = new Vector3(); // 작업이 끝나고 agv의 대기공간

    public bool work = false;
    public float elapsed_time = 0.01f;
    public float speed = 1f;

    public void set()
    {   
        // 모델을 load
        this.agv_idle = Resources.Load<GameObject>(modelPath_idle);
        this.agv_wait = Resources.Load<GameObject>(modelPath_wait);
        this.agv_work = Resources.Load<GameObject>(modelPath_work);
        // load 실패 log 출력력
        if (this.agv_idle == null && this.agv_work == null) 
        {
            Debug.LogError("모델 agv_idle와 agv_work를 Assets/resources에서 load 실패");
        }
        else if (this.agv_idle == null)
        {
            Debug.LogError("모델 agv_idle을 Assets/resources에서 load 실패");
        }
        else if (this.agv_work == null)
        {
            Debug.LogError("모델 agv_work를를 Assets/resources에서 load 실패");
        }
    }
    // log에서 데이터를 읽어 route를 설정
    public void set_route(Queue<Vector3> route)
    {
        this.route = route;
        this.current_destination =  this.route.Dequeue();
    }
    // 기본 설정 위치에 모델을 생성
    public void _GenerateModelAtlineMid()
    {   
        // 현재 위치에 모델을 생성 *Quaternion은 3차원 회전 정보
        this.model = Instantiate(this.agv_idle, this.wait_place,  Quaternion.Euler(0, 0f, 0));
        this.agvCamera = this.model.GetComponentInChildren<Camera>(true);
    }

    public void _Change_model(string model_state)
    {
        Vector3 targetPosition = this.model.transform.position;
        Destroy(this.model);// 기존 모델 삭제
        // 작업 상태로 전환
        if (model_state == "work")
        {
            this.work = true;
            this.model = Instantiate(this.agv_work, targetPosition, Quaternion.Euler(0, 0f, 0));
        }
        // 휴식 상태로 전환
        else if(model_state == "idle")
        {
            this.model = Instantiate(this.agv_idle, targetPosition, Quaternion.Euler(0, 0f, 0));
        }

        // 대기 상태로 전환
        else if(model_state == "wait")
        {
            this.model = Instantiate(this.agv_wait, targetPosition, Quaternion.Euler(0, 0f, 0));
        }
        this.agvCamera = this.model.GetComponentInChildren<Camera>(true);
    }
    // Order가 들어 올 경우 model을 만들거나 상태를 변환 시킴킴
    public void Input_Order()
    {
        // 모델이 생성되어 있는 상태라면 Work로 전환
        if (this.model != null){
            this._Change_model("work");
        }
        // 모델이 아직 생성되어 있지 않았다면 Line1의 중앙에 모델 생성성
        else{
            this._GenerateModelAtlineMid();
            this._Change_model("work");
        }
        
    }
    // Start_place: 출발 
    public void Move()
    {
        
        // 남은 목적지가 남아있다면 
        if (this.route.Count>0)
        {
            // 현재 위치와 목적지가 같다면 목적지 변경
            if(this.model.transform.position == this.current_destination)
            {
                // 다음 목적지를 큐에서 추출
                this.current_destination =  this.route.Dequeue();
                // 이동 명령
                this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, this.current_destination, this.speed*this.elapsed_time );
            }
            else   
            {
                // 이동 명령
                this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, this.current_destination, this.speed*this.elapsed_time );
            }

        }
        // 마지막 이동이라면
        else
        {
            // 작업 종료
            if(this.model.transform.position == this.current_destination)
            {
                this.work = false;
                
                if(this.wait_place == this.current_destination)
                {
                    this.End_Work();
                }
                else
                {
                    this.wait_work();
                }
                
            }
            else
            {
                // 이동 명령
                this.model.transform.position = Vector3.MoveTowards(this.model.transform.position, this.current_destination, this.speed*this.elapsed_time );
            } 
        }
        this.model.transform.LookAt(this.current_destination);
        
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
/*
        Vector3 targetPosition = this.model.transform.position;
        Destroy(this.model);// 기존 모델 삭제
        // 빈 팔렛을 들고있는 상태
        if (model_state == "pallet")
        {
            this.work = true;
            this.model = Instantiate(this.agv_work, targetPosition, Quaternion.Euler(0, 0f, 0));
        }
        // 제조 중간의 제품을 들고있는 상태
        else if (model_state == "wip")
        {
            this.work = true;
            this.model = Instantiate(this.agv_work, targetPosition, Quaternion.Euler(0, 0f, 0));
        }
        // 기본 상태로 전환
        else if(model_state == "empty")
        {
            this.work = true;
            this.model = Instantiate(this.agv_idle, targetPosition, Quaternion.Euler(0, 0f, 0));
        }
        else if(model_state == "idle")
        {
            this.model = Instantiate(this.agv_idle, targetPosition, Quaternion.Euler(0, 0f, 0));
        }
        */