# Unity 프로젝트를 사용하는 방법

1. **Unity 라이센스를 받아서 Unity Hub를 설치**  
   - [Unity 공식 홈페이지](https://unity.com/)에서 라이센스를 신청하고 Unity Hub를 다운로드합니다.  

2. **Unity Hub에서 프로젝트를 생성**  
   - Unity Hub를 실행하고 새 프로젝트를 생성합니다.  

3. **본 repo의 unity package를 다운로드**  
   - 해당 GitHub Repository를 로컬에 클론하거나 다운로드합니다.  

4. **Unity 패키지 가져오기**  
   - `unity_project` 폴더에 있는 `Simulation_package_version1.unitypackage` 파일을 Unity 프로젝트로 드래그 앤 드롭하여 가져옵니다.  
5. **Unity Simulation만 진행 할 경우**
   - 'Unity_Simul_V1.1'만 다운로드 후 My project.exe파일을 실행


```mermaid
classDiagram
   class GameManager {
        +string info_key
        +infos info
        +TMP_Text displaytext
        +GameObject canvas
        +List~base_agv~ build_agvs
        +List~base_agv~ wash_agvs
        +List~base_agv~ dry_agvs
        +List~base_agv~ return_agvs
        +List~machine~ build_machine
        +List~machine~ wash_machine
        +List~machine~ dry_machine
        +List~inspection~ inspection_inspector
        +GameObject Line_Build
        +GameObject Line_Wash
        +GameObject Line_Dry
        +GameObject Line_Return
        +GameObject Line_Inspect
        +json_read json_data
        +scan scan_env
        -_setting_agv()
        -_setting_machine()
        -_setting_inspection()
        +Start()
        +Update()
    }

    class infos {
        +int Work_Build_Agvs
        +int Work_Dry_Agvs
        +int Work_Wash_Agvs
        +int Work_Return_Agvs
        +int Work_Inspectors
        +int Work_Build_Machines
        +int Work_Dry_Machines
        +int Work_Wash_Machines
        +int Work_Return_Machines
    }
    class inspection {
        <<ScriptableObject>>
        int inspection_id
        string modelPath_idle
        string modelPath_wait
        string modelPath_work

        GameObject parent_line
        GameObject current_line

        GameObject inspect_work
        GameObject inspect_wait
        GameObject inspect_idle
        GameObject model

        Queue~Vector3~ route
        Vector3 current_desitnation
        Vector3 wait_place

        bool work

        void set(int inspection_id)
        void set_route(Queue~Vector3~ route)
        void GenerateModelAtlineLeft()
        void GenerateModelAtlineRight()
        void _Change_model(string model_state)
        void Input_Order()
        void Move()
        void wait_work()
        void End_Work()
    }
    class json_read {
        <<ScriptableObject>>
        +simulation_config config

        +Dictionary~int, Dictionary~int, Queue~Vector3~~~ buildAgvData
        +Dictionary~int, Dictionary~int, Queue~Vector3~~~ washAgvData
        +Dictionary~int, Dictionary~int, Queue~Vector3~~~ dryAgvData
        +Dictionary~int, Dictionary~int, Queue~Vector3~~~ returnAgvData
        +Dictionary~int, Dictionary~int, Queue~Vector3~~~ inspectData

        +Dictionary~int, Queue~List~int~~~~ buildMachineData
        +Dictionary~int, Queue~List~int~~~~ washMachineData
        +Dictionary~int, Queue~List~int~~~~ dryMachineData

        +void convert_json_to_dictionary()
        +void read_config()
        +void read_log()
        -Dictionary~int, Dictionary~int, Queue~Vector3~~~ _route_log_setting(string name, string text)
        -Dictionary~int, Queue~List~int~~~~ _machine_log_setting(string name, string text)
    }

    class simulation_config {
        +int Num_Build_Machine
        +int Num_Wash_Machine
        +int Num_Dry_Machine
        +int Num_Inspector
        +int Num_Build_Agvs
        +int Num_Wash_Agvs
        +int Num_Dry_Agvs
        +int Num_Return_Agvs
        +int Num_Pallets
    }

class machine {
        <<ScriptableObject>>
        +int machine_id
        +GameObject parent_line
        +string modelPath_Idle
        +string modelPath_work

        +GameObject model
        +GameObject Machine_idle
        +GameObject Machine_work

        +int start_time
        +int end_time
        +bool work

        +void set(int id, GameObject parent_line)
        +void GenerateModelAtlineLeft()
        +void GenerateModelAtlineRight()
        +void Activate_model(List~int~ start_end_pair)
        +void End_model()
    }

   class scan {
           <<ScriptableObject>>
           +int scan_agv(Dictionary~int, Dictionary~int, Queue~Vector3~~~ agv_log, List~base_agv~ agv_list)
           +int scan_machine(Dictionary~int, Queue~List~int~~~~ machine_log, List~machine~ machine_list)
           +int scan_inspect(Dictionary~int, Dictionary~int, Queue~Vector3~~~ inspect_log, List~inspection~ inspect_list)
       }
   json_read --> simulation_config
   GameManager --> inspection
   GameManager ..> scan
   GameManager --> machine
   GameManager --> json_read
   GameManager --> infos

   
    

