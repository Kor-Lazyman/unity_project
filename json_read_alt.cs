using UnityEngine;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
/*
config: json으로 읽어온 기본 시뮬레이션 데이터
Machines, Stackers, Bases, Connect, Inspect: 좌표 정보가 담겨있는 도면을 읽은 후 해당 도면에 정보 추출
AgvData: json에서 읽어온 데이터(이동경로)를 저장 할 공간
MachineData: json에서 읽어온 machine의 on/off 데이터를 읽어옴
convert_json_to_dictionary: json데이터에서 읽은 정보를 dictionary로 전환하는 함수
_read_config_, read_log: 각 json에 대한 데이터들을 dictionary로 전환
_read_objects_: string으로 작성된 도면을 읽어서 좌표로 전환하는 기능 제공
_read_coordinates_: 도면을 분야별로 정리
_setting_route_: 전체 동선 설정 기능 제공
_making_route_, _inspect_route_: 해당 분야 별 루트를 queue로 정리해서 제공공 

*/
public class json_read_alt : ScriptableObject
{
    // 시뮬레이션 설정 정보를 저장하는 변수
    public simulation_config config;

    // Test용 모델 정보(설계상 좌표)
    public Dictionary<string, Vector3> Machines = new Dictionary<string, Vector3>();
    public Dictionary<string, Vector3> Stackers = new Dictionary<string, Vector3>();
    public Dictionary<string, Vector3> Bases = new Dictionary<string, Vector3>();
    public Dictionary<string, Vector3> Connect = new Dictionary<string, Vector3>();
    public Dictionary<string, Vector3> Inspect = new Dictionary<string, Vector3>();

    // 이동 동선
    public Dictionary<int, Dictionary<int, Queue<Vector3>>> buildAgvData = new Dictionary<int, Dictionary<int, Queue<Vector3>>>();
    public Dictionary<int, Dictionary<int, Queue<Vector3>>> washAgvData = new Dictionary<int, Dictionary<int, Queue<Vector3>>>();
    public Dictionary<int, Dictionary<int, Queue<Vector3>>> dryAgvData = new Dictionary<int, Dictionary<int, Queue<Vector3>>>();
    public Dictionary<int, Dictionary<int, Queue<Vector3>>> returnAgvData = new Dictionary<int, Dictionary<int, Queue<Vector3>>>();
    public Dictionary<int, Dictionary<int, Queue<Vector3>>> inspectData = new Dictionary<int, Dictionary<int, Queue<Vector3>>>();

    // machine 작동 기록소
    public Dictionary<int ,Dictionary<int, string>> buildMachineData = new Dictionary<int ,Dictionary<int, string>>();
    public Dictionary<int ,Dictionary<int, string>> washMachineData = new Dictionary<int ,Dictionary<int, string>>();
    public Dictionary<int ,Dictionary<int, string>> dryMachineData = new Dictionary<int ,Dictionary<int, string>>();
    public Dictionary<string, Dictionary<string,Queue<int>>> StackerLog = new Dictionary<string, Dictionary<string,Queue<int>>>();
    
    // 시뮬레이션 설정 정보를 담는 클래스
    public class simulation_config
    {
        // machine에 대한 정보
        public int Num_Build_Machine;
        public int Num_Wash_Machine;
        public int Num_Dry_Machine;
        // inspector에 대한 정보
        public int Num_Inspector;
        // agv에 대한 정보
        public int Num_Build_Agvs;
        public int Num_Wash_Agvs;
        public int Num_Dry_Agvs;
        public int Num_Return_Agvs;
        public int Num_Pallets;
    }
    
    // JSON 데이터를 읽어 Dictionary로 변환하는 함수
    public void convert_json_to_dictionary()
    {
        _read_config_();
        _read_objects_();
    }
    // 시뮬레이션 설정 정보를 JSON에서 읽는 함수
    public void _read_config_()
    {
        // json데이터를 text정보로 load
        TextAsset config_simulation_asset = Resources.Load<TextAsset>("3Dprinter_data");
        string config_simulation_text = config_simulation_asset.text;
        // 해당 text를 class의 형식에 맞게 정보를 전환
        config = JsonConvert.DeserializeObject<simulation_config>(config_simulation_text);
    }
    
    // AGV 및 머신 로그 데이터를 JSON에서 읽는 함수
    public void _read_objects_()
    {
        // resource에서 json을 읽어와 TextAsset로 전환
        TextAsset drawing_coordinates_asset = Resources.Load<TextAsset>("drawing_coordinates");
        TextAsset machine_log_asset = Resources.Load<TextAsset>("machine_log_command");
        TextAsset stacker_log_asset = Resources.Load<TextAsset>("stacker_log_data");

        // 해당 에셋을 string으로 전환
        string drawing_coordinates_text = drawing_coordinates_asset.text;
        string machine_log_text = machine_log_asset.text;
        string stacker_log_text = stacker_log_asset.text;

        // stacker의 입출 기록 클래스 생성
        var coordinates_dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<List<float>>>>>(drawing_coordinates_text);
        StackerLog = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string,Queue<int>>>>(stacker_log_text);
    
        // 도면 읽어서 각 part별 도면 분류
        _read_coordinates_(coordinates_dict);

        // 도면 정보와, 이동 기록을 바탕으로 이동 경로 설계
        buildAgvData = _setting_route_("Build");
        washAgvData = _setting_route_("Wash");
        dryAgvData = _setting_route_("Dry");
        returnAgvData = _setting_route_("Return");
        inspectData = _setting_route_("Inspect");

        // machine의 변화 기록에 대한 정보 읽는 기능 제공
        buildMachineData = _machine_log_setting_("Build", machine_log_text);
        washMachineData = _machine_log_setting_("Wash", machine_log_text);
        dryMachineData = _machine_log_setting_("Dry", machine_log_text);
    }
    // 도면을 읽어서 part별 도면 분류
    private void _read_coordinates_(Dictionary<string, Dictionary<string, List<List<float>>>> coordinates_dict)
    {
        // 기계에 대한 도면을 읽어서 좌표로 기록
        foreach (var part in coordinates_dict["Machine"])
        {
            // part별로 분류
            int count = 0;
            foreach (var item in part.Value)
            {
                Machines[$"{part.Key}_Machine_{count}"] = new Vector3(item[0], item[1], item[2]);
                count ++;
            }
        }
        // Stacker에 대한 도면을 읽어서 좌표로 기록
        foreach (var name in coordinates_dict["Stacker"])
        {
            List<float> temp_stacker = name.Value[0];
            Stackers[name.Key] = new Vector3(temp_stacker[0], temp_stacker[1], temp_stacker[2]);
        }
        // Base에 대한 도면을 읽어서 좌표로 기록
        foreach (var name in coordinates_dict["Base"])
        {
            List<float> temp_Base = name.Value[0];
            Bases[name.Key] = new Vector3(temp_Base[0], temp_Base[1], temp_Base[2]);
        }
        // Connect에 대한 도면을 읽어서 좌표로 기록
        foreach (var name in coordinates_dict["Connect"])
        {
            List<float> temp_Connect = name.Value[0];
            Connect[name.Key] = new Vector3(temp_Connect[0], temp_Connect[1], temp_Connect[2]);
        }
        // Inspect에 대한 도면을 읽어서 좌표 기록
        int inspect_count = 0;
        foreach (var name in coordinates_dict["Inspect"])
        {
            List<float> temp_Inspect = name.Value[0];
            Inspect[$"inspector_{inspect_count}"] = new Vector3(temp_Inspect[0], temp_Inspect[1], temp_Inspect[2]);
        }
    }
    private Dictionary<int,Dictionary<int, Queue<Vector3>>> _setting_route_(string part)
    {
        //part: 부서이름
       
        TextAsset Name_Command_asset = Resources.Load<TextAsset>("Name_Command");
        string Name_Command_text = Name_Command_asset.text;
        // text를 dictionary 전환
        var parsedJson = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>>(Name_Command_text);
        // key를 파싱, List를 queue 형식으로 전환 하기 위한 공간 생성
        Dictionary<int, Dictionary<int, Queue<Vector3>>> data = new Dictionary<int, Dictionary<int, Queue<Vector3>>>();
        // 파싱하고, 최종 데이터로 전환
        foreach (var agv_id in parsedJson[part].Keys)// 파싱할 key를 설정
        {
            // 파싱한 데이터를 key로 설정, 데이터를 queue로 전환
            Dictionary<int, Queue<Vector3>> temp_dict = new Dictionary<int, Queue<Vector3>>();// 각 agv당 루트를 저장할 임시 dict 생성
            foreach (var kvp in parsedJson[part][agv_id])// queue로 전환할 정보를 선택
            {
                
                if (int.TryParse(kvp.Key, out int time_id)) // time_id를 int로 파싱
                {
                    Queue<Vector3> queue = new Queue<Vector3>();
                    if(part == "Inspect")
                    {
                        queue = _inspect_route_(parsedJson[part][agv_id][kvp.Key][0], parsedJson[part][agv_id][kvp.Key][1], parsedJson[part][agv_id][kvp.Key][2]);
                    }
                    else
                    {
                        queue = _making_route_(part, parsedJson[part][agv_id][kvp.Key][0], parsedJson[part][agv_id][kvp.Key][1], parsedJson[part][agv_id][kvp.Key][2]);
                    }
                    
                    //queue계산 코드 삽입
                    temp_dict[time_id] = queue;// queue를 임시 dict에 저장
                }
              
            }
            if (int.TryParse(agv_id, out int id))// agv_id를 id(int)로 파싱
            {
                data[id] = temp_dict;
            }      
        }
        return data;
    }
    //machine의 상태변환 설정
    private Dictionary<int, Dictionary<int, string>> _machine_log_setting_(string name, string config_simulation_text)
    {
        // 파싱할 임시 dict를 생성
        var parsedJson = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Dictionary<string, List<string>>>>>(config_simulation_text);
        // 최종 형태를 임시로 생성
        Dictionary<int, Dictionary<int, string>> data = new Dictionary<int, Dictionary<int, string>>();
        // 데이터를 추출 한 이후 파싱 및 queue에 등록 
        foreach (var kvp in parsedJson[name])// 파싱 할 데이터를 추출
        {
            if (int.TryParse(kvp.Key, out int machine_id)) // 키를 int로 변환
            {
                data[machine_id] = new Dictionary<int, string>();
                foreach (var log_data in kvp.Value)// 리스트를 추출
                {
                    if (int.TryParse(log_data.Key, out int time_id))
                    {
                        data[machine_id][time_id] = log_data.Value[1];// 리스트를 queue에 등록
                    }
                }
            }
        }
        return data;
    }
    // agv가 이동할 길을 log를 읽어서 준비
    private Queue<Vector3> _making_route_(string part, string move_type, string start, string target)
    {
        // move type: 목적지를 말하며, 특정 목적지 마다 이동 방식이 달라짐
        Queue<Vector3> destination = new Queue<Vector3>();
        
        if (move_type == "Line_Connect")
        {
            Vector3 move_first = new Vector3(Stackers[start].x, 0, Connect[target].z);
            destination.Enqueue(move_first);
            destination.Enqueue(Connect[target]);
        }
        else if (move_type == "Stacker")
        {
            // Line을 따라 움직이기에, 목적지의 좌표로 루트 계산
            Vector3 move_first = new Vector3(Stackers[target].x, 0, Connect[start].z);
            destination.Enqueue(move_first);
            destination.Enqueue(Stackers[target]);

        }
        else if (move_type == "Stacker_Connect")
        {
            destination.Enqueue(Connect[target]);
        }
        else if (move_type == "Base")
        {
            destination.Enqueue(Bases[target]);
        }
        else if (move_type == "Return")
        {
            Vector3 move_first = new Vector3();
            if (target == "Return_Base")
            {
                move_first = new Vector3(Stackers[start].x, 0, Bases[target].z);
                destination.Enqueue(move_first);
                destination.Enqueue(Bases[target]);
            }
            else if (target == "Pallet_Stacker")
            {
                move_first = new Vector3(Stackers[target].x, 0, Stackers[start].z);
                destination.Enqueue(move_first);
                destination.Enqueue(Stackers[target]);
            }
            else if (start == "Return_Base")
            {
                destination.Enqueue(Stackers[target]);
            }

        }
        else if (move_type == "Machine")
        {
            Vector3 temp = Machines[target];
            // 머신의 z축 기준 머신의 앞에 도달 하도록 하는 코드
            if (Bases[$"{part}_Base"].z - Machines[target].z > 0)
            {
                temp.z = Machines[target].z + 1;
            }
            else if (Bases[$"{part}_Base"].z - Machines[target].z < 0)
            {
                temp.z = Machines[target].z - 1;
            }
            //머신의 x축 기준 앞에 도달 하도록 하는 코드드
            else if (Machines[target].x > Bases[$"{part}_Base"].x)
            {
                temp.x = Machines[target].x - 1;
            }
            else if (Machines[target].x < Bases[$"{part}_Base"].x)
            {
                temp.x = Machines[target].x + 1;
            }
            destination.Enqueue(temp);
        }
        return destination;
    }
    private Queue<Vector3> _inspect_route_(string move_type, string start, string target)
    {
        Queue<Vector3> destination = new Queue<Vector3>();
        if(move_type == "Line_Connect")
        {
            Vector3 move_first = new Vector3 (Stackers[start].x, 0, Connect[target].z);
            destination.Enqueue(move_first);
            destination.Enqueue(Connect[target]);
        }
        else if(move_type == "Stacker")
        {
            Vector3 move_first = new Vector3 (Stackers[target].x, 0, Connect[start].z);
            destination.Enqueue(move_first);
            destination.Enqueue(Stackers[target]);

        }
        else if(move_type == "Stacker_Connect")
        {
            destination.Enqueue(Connect[target]);
        }
        else if(move_type == "Table")
        {
            destination.Enqueue(Inspect[target]);
        }
        else if(move_type == "Wait_Base")
        {
            destination.Enqueue(Inspect[target]);
        }
        return destination;
    }
    
}
