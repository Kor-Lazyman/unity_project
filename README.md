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
# 사용 Asset
1. banan man: https://assetstore.unity.com/packages/3d/characters/humanoids/banana-man-196830
2. Kitchen Furniture Starterpack: https://assetstore.unity.com/packages/3d/props/furniture/kitchen-furniture-starterpack-209331
3. Cartoon Wooden Box: https://assetstore.unity.com/packages/3d/props/furniture/cartoon-wooden-box-242926
4. Controllable Forklift Free: https://assetstore.unity.com/packages/3d/vehicles/controllable-forklift-free-80275

```mermaid
classDiagram
    %% === Main Classes ===
    class GameManager {
        - json_read_alt json_data_alt
        - scan scan_env
        - List~base_agv~ build_agvs
        - List~machine~ build_machine
        - List~inspection~ inspection_inspector
        - void _setting_agv(...)
        - void _setting_machine(...)
        - void _setting_inspection(...)
    }

    class base_agv {
        - GameObject model
        - Queue~Vector3~ route
        - void set()
        - void set_route(...)
        - void Input_Order()
        - void Move()
        - void _Change_model(...)
    }

    class inspection {
        - GameObject model
        - Queue~Vector3~ route
        - void set(...)
        - void set_route(...)
        - void Input_Order()
        - void Move()
        - void _Change_model(...)
    }

    class machine {
        - GameObject model
        - void set(...)
        - void Change_model(...)
    }

    class scan {
        - int scan_agv(...)
        - int scan_machine(...)
        - int scan_inspect(...)
        - void scan_stacker(...)
    }

    class json_read_alt {
        - simulation_config config
        - Dictionary~int, Dictionary~int, Queue~Vector3~~~ buildAgvData
        - Dictionary~string, Vector3~ Machines
        - void convert_json_to_dictionary()
    }

    class json_read_alt.simulation_config {
        int Num_Build_Machine
        int Num_Wash_Machine
        int Num_Dry_Machine
        int Num_Inspector
        int Num_Build_Agvs
        int Num_Wash_Agvs
        int Num_Dry_Agvs
        int Num_Return_Agvs
        int Num_Pallets
    }

    class CameraController {
        - Transform orbitCenter
        - float radius
        - void Update()
    }

    %% === Relationships ===
    GameManager --> json_read_alt : uses
    GameManager --> scan : uses
    GameManager --> base_agv : instantiates
    GameManager --> machine : instantiates
    GameManager --> inspection : instantiates


    json_read_alt --> json_read_alt.simulation_config : contains
![Editor _ Mermaid Chart-2025-05-26-090924](https://github.com/user-attachments/assets/bcf70363-29db-43d4-9a58-338ebbd4a376)

