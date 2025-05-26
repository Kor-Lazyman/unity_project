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
    %% === 주요 클래스 ===
    class GameManager {
        - json_read_alt json_data_alt  %% JSON 데이터 파서
        - scan scan_env  %% AGV, 머신, 검사기 동작 제어 클래스
        - List~base_agv~ build_agvs  %% AGV 객체 리스트
        - List~machine~ build_machine  %% 머신 객체 리스트
        - List~inspection~ inspection_inspector  %% 검사기 객체 리스트
        - void _setting_agv(...) %% AGV 초기화 함수
        - void _setting_machine(...) %% 머신 초기화 함수
        - void _setting_inspection(...) %% 검사기 초기화 함수
    }

    class base_agv {
        - GameObject model  %% AGV의 3D 모델
        - Queue~Vector3~ route  %% 이동 경로 큐
        - void set() %% AGV 리소스 로드 및 기본 세팅
        - void set_route(...) %% 경로 설정
        - void Input_Order() %% 작업 시작 명령
        - void Move() %% 이동 수행
        - void _Change_model(...) %% 모델 상태(work/idle/wait) 전환
    }

    class inspection {
        - GameObject model  %% 검사기의 3D 모델
        - Queue~Vector3~ route  %% 이동 경로 큐
        - void set(...) %% 검사기 리소스 로드 및 초기화
        - void set_route(...) %% 경로 설정
        - void Input_Order() %% 작업 시작 명령
        - void Move() %% 이동 수행
        - void _Change_model(...) %% 모델 상태 전환
    }

    class machine {
        - GameObject model  %% 머신 3D 모델
        - void set(...) %% 머신 초기화
        - void Change_model(...) %% 상태에 따른 모델 전환
    }

    class scan {
        - int scan_agv(...) %% AGV 작업 처리
        - int scan_machine(...) %% 머신 상태 갱신
        - int scan_inspect(...) %% 검사기 작업 처리
        - void scan_stacker(...) %% 스태커 입출 기록 갱신
    }

    class json_read_alt {
        - simulation_config config  %% 시뮬레이션 설정값 클래스
        - Dictionary~int, Dictionary~int, Queue~Vector3~~~ buildAgvData %% AGV 로그 데이터
        - Dictionary~string, Vector3~ Machines %% 머신 위치 정보
        - void convert_json_to_dictionary() %% JSON 파싱 시작 함수
    }

    class json_read_alt.simulation_config {
        int Num_Build_Machine %% 빌드 머신 수
        int Num_Wash_Machine %% 세척 머신 수
        int Num_Dry_Machine %% 건조 머신 수
        int Num_Inspector %% 검사기 수
        int Num_Build_Agvs %% 빌드 AGV 수
        int Num_Wash_Agvs %% 세척 AGV 수
        int Num_Dry_Agvs %% 건조 AGV 수
        int Num_Return_Agvs %% 반환 AGV 수
        int Num_Pallets %% 팔레트 수
    }

    class CameraController {
        - Transform orbitCenter %% 궤도 중심점
        - float radius %% 회전 반지름
        - void Update() %% 카메라 입력 및 궤도 이동 처리
    }

    %% === 클래스 간 관계 ===
    GameManager --> json_read_alt : uses
    GameManager --> scan : uses
    GameManager --> base_agv : instantiates
    GameManager --> machine : instantiates
    GameManager --> inspection : instantiates

    json_read_alt --> json_read_alt.simulation_config : contains

```
```mermaid
sequenceDiagram
    autonumber
    participant AGV as base_agv
    participant Machine as machine
    participant Inspect as inspection
    participant Scan as scan
    participant GM as GameManager
    participant JSON as json_read_alt

    %% 초기화
    GM->>JSON: convert_json_to_dictionary
    JSON-->>GM: 설정 및 로그 데이터 반환

    GM->>AGV: set (여러 AGV 인스턴스 초기화)
    GM->>Machine: set (여러 Machine 인스턴스 초기화)
    GM->>Inspect: set (여러 Inspection 인스턴스 초기화)

    %% FixedUpdate 루프
    loop FixedUpdate 루프
        GM->>Scan: scan_agv(log, time, agv_list)
        Scan->>AGV: set_route
        Scan->>AGV: Input_Order
        Scan->>AGV: Move

        GM->>Scan: scan_machine(log, time, machine_list)
        Scan->>Machine: Change_model

        GM->>Scan: scan_inspect(log, time, inspect_list)
        Scan->>Inspect: set_route
        Scan->>Inspect: Input_Order
        Scan->>Inspect: Move

        GM->>Scan: scan_stacker(log, time)
        Scan-->>GM: Stacker count 변경

        GM->>GM: 카메라 추적, UI 텍스트 갱신
    end
```
```mermaid
flowchart TD
    %% 초기화 및 설정
    A["Start GameManager (게임 매니저 시작)"] --> B["json_read_alt.convert_json_to_dictionary (JSON 설정 읽기)"]
    B --> C1["_setting_agv (AGV 초기화)"]
    B --> C2["_setting_machine (머신 초기화)"]
    B --> C3["_setting_inspection (검사기 초기화)"]

    C1 --> D["Create AGVs (base_agv) (AGV 인스턴스 생성)"]
    C2 --> E["Create Machines (machine) (머신 인스턴스 생성)"]
    C3 --> F["Create Inspectors (inspection) (검사기 인스턴스 생성)"]

    F --> G["Time.fixedDeltaTime = 0.001 (물리 프레임 간격 설정)"]
    G --> H["FixedUpdate Loop (고정 업데이트 루프)"]

    %% FixedUpdate 내부 스캔 함수들
    H --> I["scan_env.scan_agv (AGV 작업 스캔)"]
    H --> J["scan_env.scan_machine (머신 상태 스캔)"]
    H --> K["scan_env.scan_inspect (검사기 작업 스캔)"]
    H --> L["scan_env.scan_stacker (스태커 입출력 스캔)"]

    I --> I1["base_agv.set_route (AGV 경로 설정)"]
    I1 --> I2["base_agv.Input_Order (AGV 작업 시작 명령)"]
    I2 --> I3["base_agv.Move (AGV 이동 실행)"]

    J --> J1["machine.Change_model (머신 상태 전환)"]

    K --> K1["inspection.set_route (검사기 경로 설정)"]
    K1 --> K2["inspection.Input_Order (검사기 작업 시작 명령)"]
    K2 --> K3["inspection.Move (검사기 이동 실행)"]

    L --> L1["Update Stacker Count (스태커 수량 갱신)"]

    H --> M["Update Camera (카메라 상태 갱신)"]
    M --> N["Update UI Text / Speed (UI 텍스트/속도 표시 갱신)"]

    %% 스타일 지정
    style A fill:#f9f,stroke:#333,stroke-width:2px
    style H,H1,H2,H3,H4 fill:#ccf,stroke:#000
    style I,J,K,L fill:#ccf,stroke:#000
    style G fill:#bbf,stroke:#333,stroke-width:1px

```


