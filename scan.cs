using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
/*
scan_agv: agv_log를 읽은 후에 해당 시점에 agv가 해야 할 작업이 존재하거나, 일을 받을 경우 작업을 시작함
scan_machine: machine_log를 읽은 후에 해당 시점에 machine 해야 할 작업이 존재하거나, 일을 받을 경우 작업을 시작함
*/
public class scan : ScriptableObject
{
    int time_id = 0;
    public int scan_agv(Dictionary<int, Dictionary<int, Queue<Vector3>>> agv_log, int time, int time_magnification, List<base_agv> agv_list)
    {   
        // 존재하는 모든 agv를 검색
        foreach(var agv_id in agv_log.Keys)
        {
            // 해당 시점이 100번의 frame이 진행된 상황인 경우
            if(Time.frameCount%time_magnification == 1){
                // 해당 agv가 현 시점에 발생하는 사건을 가지고 있는 경우 작업 개시를 명령
                if(agv_log[agv_id].ContainsKey(time))
                {
                    // agv의 이동 경로를 설정
                    agv_list[agv_id].set_route(agv_log[agv_id][time]);
                    // 생성, 작업 개시 명령을 내림
                    agv_list[agv_id].Input_Order();
                    // 이동 명령
                    agv_list[agv_id].Move(); 
                }   
            }

            // 해당 agv가 작업중(이동 중)인 경우
            else if(agv_list[agv_id].work)
            {
                // 이동을 명령
                agv_list[agv_id].Move();
            }
        }
        int count_agvs = 0;
        foreach(var agv in agv_list)
        {
            if(agv.work)
            {
                count_agvs +=1;
            }
        }
        return count_agvs;
    }
    
    public int scan_machine(Dictionary<int, Dictionary<int, string>> machine_log, int time, int time_magnification, List<machine> machine_list)
    {
        int count_machines = 0;
        foreach(var machine_id in machine_log.Keys)
        {
            if(machine_log[machine_id].ContainsKey(time))
            {
                machine_list[machine_id].Change_model(machine_log[machine_id][time]);
                if(machine_list[machine_id].work)
                {
                    count_machines += 1;
                }
            }
        }
        return count_machines;
    }
    public int scan_inspect(Dictionary<int, Dictionary<int, Queue<Vector3>>> inspect_log, int time, int time_magnification, List<inspection> inspect_list)
    {
        // 존재하는 모든 agv를 검색
        foreach(var inspect_id in inspect_log.Keys)
        {
            // 해당 시점이 100번의 frame이 진행된 상황인 경우
            if(Time.frameCount%time_magnification == 1){
                // 해당 inspect가 현 시점에 발생하는 사건을 가지고 있는 경우 작업 개시를 명령
                if(inspect_log[inspect_id].ContainsKey(time))
                {
                    // inspect의 이동 경로를 설정
                    inspect_list[inspect_id].set_route(inspect_log[inspect_id][time]);
                    // 생성, 작업 개시 명령을 내림
                    inspect_list[inspect_id].Input_Order();
                    // 이동 명령
                    inspect_list[inspect_id].Move(); 
                }   
            }

            // 해당 inspect가 작업중(이동 중)인 경우
            else if(inspect_list[inspect_id].work)
            {
                // 이동을 명령
                inspect_list[inspect_id].Move();
            }
        }
        int count_inspects = 0;
        foreach(var inspect in inspect_list)
        {
            if(inspect.work)
            {
                count_inspects +=1;
            }
        }
        return count_inspects;
    }
    public void scan_stacker(Dictionary<string, Dictionary<string,Queue<int>>> StackerLog, int time, Dictionary<string, int> stacker_info)
    {
        foreach(var stacker_id in StackerLog["IN"].Keys)
        {
            if(StackerLog["IN"][stacker_id].Count != 0)
            {
                if(StackerLog["IN"][stacker_id].Peek() == time )
                {
                    stacker_info[stacker_id] += 1;
                    //Debug.Log($"{stacker_id}:{stacker_info[stacker_id]}");
                    StackerLog["IN"][stacker_id].Dequeue();
                }
            }
            if(StackerLog["OUT"][stacker_id].Count != 0)
            {
                if(StackerLog["OUT"][stacker_id].Peek() == time)
                {
                    stacker_info[stacker_id] -= 1;
                    //Debug.Log($"{stacker_id}:{stacker_info[stacker_id]}");
                    StackerLog["OUT"][stacker_id].Dequeue();
                }
            }
        }
    }
}
