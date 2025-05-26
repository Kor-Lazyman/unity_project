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
%% === GameManager ===
class GameManager {
    +string info_key : 현재 선택된 정보 유형
    +Camera mainCamera : 메인 카메라
    +Camera Scene_camera : 전체 씬 카메라
    +Camera Current_Camera : 현재 카메라
    +base_agv tracking_agv : 추적 중 AGV
    +inspection tracking_inspector : 추적 중 검사자
    +TMP_Text displaytext : AGV 정보 텍스트
    +TMP_Text speeddisplay : 속도 표시 텍스트
    +TMP_Text cameratext : 카메라 상태 텍스트
    +GameObject canvas : UI 캔버스
    +List~base_agv~ build_agvs : Build 부서 AGV 목록
    +List~base_agv~ wash_agvs : Wash 부서 AGV 목록
    +List~base_agv~ dry_agvs : Dry 부서 AGV 목록
    +List~base_agv~ return_agvs : Return 부서 AGV 목록
    +List~machine~ build_machine : Build 부서 머신
    +List~machine~ wash_machine : Wash 부서 머신
    +List~machine~ dry_machine : Dry 부서 머신
    +List~inspection~ inspection_inspector : 검사자 목록
    +Dictionary~string,int~ Stacker_info : 스태커 수량
    +GameObject Line_Inspect : 검사 라인
    +GameObject Quit_Button~Button_4 : 시간 제어 버튼
    +GameObject Scene_Button~Inspect_Button : 카메라 버튼
    +GameObject Plus_Button~Minus_Button : 추적 변경 버튼
    +json_read_alt json_data_alt : json 로딩 클래스
    +scan scan_env : 스캔(명령 해석) 클래스
    +int Number_Camera : 현재 추적 카메라 번호
    +int magnification : 배속
    +float base_time : 기본 시간 단위
    +int time : 시뮬레이션 시간
    +int time_magnification : 시간 배율
    +string Camera_Type : 현재 카메라 상태
    +bool cooldown : 버튼 쿨다운
    +void Start()
    +void FixedUpdate()
    +void Keyboard_input()
    +void Scan_Env()
    +void Buttons()
    +void stop_button()
    +void speed_button(int)
    +void camera_button(string)
    +void Up_Down(string)
    +void agv_camera_change(List)
    +void inspect_camera_change()
    +void camera_tracking()
}

%% === base_agv ===
class base_agv {
    +Camera agvCamera : AGV 카메라
    +string modelPath_idle~work~wait : 모델 경로
    +GameObject agv_work~wait~idle : 모델 프리팹
    +GameObject model : 현재 모델
    +Queue~Vector3~ route : 경로
    +Vector3 current_destination : 목표 위치
    +Vector3 wait_place : 대기 위치
    +bool work : 작업 중 여부
    +float elapsed_time : 프레임 시간
    +float speed : 이동 속도
    +void set()
    +void set_route(Queue~Vector3~)
    +void _GenerateModelAtlineMid()
    +void _Change_model(string)
    +void Input_Order()
    +void Move()
    +void wait_work()
    +void End_Work()
}

%% === inspection ===
class inspection {
    +int inspection_id : 식별자
    +Camera inspectionCamera : 검사자 카메라
    +string modelPath_idle~work~wait : 모델 경로
    +GameObject parent_line : 상위 라인
    +GameObject current_line : 현재 위치
    +GameObject inspect_work~wait~idle : 프리팹
    +GameObject model : 현재 출력 중 모델
    +Queue~Vector3~ route : 이동 경로
    +Vector3 current_desitnation : 현재 목적지
    +Vector3 wait_place : 대기 위치
    +float elapsed_time : 시간 단위
    +float speed : 속도
    +bool work : 작업 중 여부
    +void set(int)
    +void set_route(Queue~Vector3~)
    +void GenerateModelAtlineLeft()
    +void GenerateModelAtlineRight()
    +void _Change_model(string)
    +void Input_Order()
    +void Move()
    +void wait_work()
    +void End_Work()
}

%% === machine ===
class machine {
    +int machine_id : 식별자
    +GameObject parent_line : 상위 라인
    +string modelPath_Idle~Work~Ready : 모델 경로
    +GameObject model : 현재 머신
    +GameObject Machine_idle~work~ready : 프리팹
    +bool work : 작동 여부
    +void set(int, Vector3, Vector3)
    +void GenerateModel(Vector3, Vector3)
    +void Change_model(string)
}

%% === json_read_alt ===
class json_read_alt {
    +simulation_config config : 설정 구조체
    +Dictionary~string,Vector3~ Machines~Bases~Connect~Inspect~Stackers
    +Dictionary~int, Dictionary~int, Queue~Vector3~~ buildAgvData~... : AGV 로그
    +Dictionary~int, Dictionary~int, string~ buildMachineData~... : 머신 로그
    +Dictionary~string, Dictionary~string, Queue~int~~ StackerLog : 스태커 로그
    +void convert_json_to_dictionary()
    +void _read_config_()
    +void _read_objects_()
    +Dictionary _setting_route_(string)
    +Dictionary _machine_log_setting_(...)
}

%% === scan ===
class scan {
    +int scan_agv(...) : AGV 동작 명령 수행
    +int scan_machine(...) : 머신 상태 업데이트
    +int scan_inspect(...) : 검사자 이동 명령
    +void scan_stacker(...) : 스태커 입출 기록 처리
}

%% === CameraController ===
class CameraController {
    +Transform orbitCenter : 회전 중심
    +float radius~height~tiltAngle : 궤도 반지름/높이/기울기
    +float totalFrames : 전체 프레임 수
    +float moveSpeed : 이동 속도
    +float rotationSpeed : 회전 속도
    +float zoomSpeed : 줌 속도
    +float minFOV~maxFOV : 줌 한계
    +void Start()
    +void Update()
}

%% === 관계선 설정 ===
GameManager --> base_agv : AGV 제어
GameManager --> inspection : 검사자 제어
GameManager --> machine : 머신 제어
GameManager --> scan : 명령 해석
GameManager --> json_read_alt : json 로딩

base_agv --> Camera : 카메라 사용
inspection --> Camera : 카메라 사용

json_read_alt --> simulation_config : 구성 정보 포함

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


