using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;
using TMPro;
using static machine; 
using static base_agv;
using static inspection; 
using static scan;
using static json_read_alt;
/*
build_agvs, wash_aagvs, dry_agvs: 각 부서에서 움직일 agv들을 담을 리스트, 각 리스트에는 원소로 base_agv의 instance가 저장됨
build_machine, wash_machine, dry_machine: 각 부서에서 작동될 machine들을 담을 리스트, 각 리스트에는 원소로 machine의 instance가 저장됨
Line_Build, Line_Wash, Line_Dry: 각 부서에서 시뮬레이션 상의 물리적으로 존재하는 line에 대한 정보를 저장
_setting_agv, _setting_machine: json_read를 통해 읽은 json파일을 바탕으로 agv와 machine을 각 작업부서의 List에 원소로 저장하는 함수
Start: 시뮬레이션이 시작하는 순간 작동하는 함수, 처음 Update가 진행되기 전에 작업이 이루어짐
Update: 1프레임이 지나면 호출되는 함수로, 시뮬레이션을 Update함
*/
public class GameManager : MonoBehaviour
{
    public string info_key = "Clear";
    public Camera mainCamera;
    public Camera Scene_camera;
    public Camera Current_Camera;
    public base_agv tracking_agv;
    public inspection tracking_inspector;
    public class infos
    {
        public int Work_Build_Agvs = 0;
        public int Work_Dry_Agvs = 0;
        public int Work_Wash_Agvs = 0;
        public int Work_Return_Agvs = 0;
        public int Work_Inspectors = 0;
        public int Work_Build_Machines = 0;
        public int Work_Dry_Machines = 0;
        public int Work_Wash_Machines = 0;
        public int Work_Return_Machines = 0;
    }
    public infos info = new infos();
    public TMP_Text displaytext;
    public TMP_Text speeddisplay;
    public TMP_Text cameratext;
    public GameObject canvas;
    // 시뮬레이션에서 움직일 AGV들에 대한 정보 공간
    public List<base_agv> build_agvs;
    public List<base_agv> wash_agvs;
    public List<base_agv> dry_agvs;
    public List<base_agv> return_agvs;
    // 시뮬레이션에서 작동될 machine들에 대한 정보 공간
    public List<machine> build_machine;
    public List<machine> wash_machine;
    public List<machine> dry_machine;
    public List<inspection> inspection_inspector;
    public Dictionary<string, int> Stacker_info = new Dictionary<string, int>(){
                                                    {"Stacker 1", 10},
                                                    {"Stacker 2", 0},
                                                    {"Stacker 3", 0},
                                                    {"Stacker 4", 0},
                                                    {"Stacker 5", 0}
                                                    };
    // 각 작업 공간의 Line을 정보로 입력
    public GameObject Line_Inspect; 
    // 시뮬레이션상의 버튼에 대한 정보
    public GameObject Quit_Button; // 시뮬레이션 종료 버튼튼
    // 속도 배율 버튼
    public GameObject Button_0;
    public GameObject Button_1;
    public GameObject Button_2;
    public GameObject Button_3;
    public GameObject Button_4;
    // 카메라 전환 버튼
    public GameObject Scene_Button;
    public GameObject Build_Button;
    public GameObject Dry_Button;
    public GameObject Wash_Button;
    public GameObject Return_Button;
    public GameObject Inspect_Button;
    // 관측 agv 선택 버튼 등록록
    public GameObject Plus_Button;
    public GameObject Minus_Button;

    public json_read_alt json_data_alt; // json_read에 대한 클래스 공간 배정
 
    public scan scan_env;  // 환경 정보를 읽고, 환경에 명령을 내리는 클래스 배정
 
    public int Number_Camera = 0; // 현재 카메라가 해당 범주에서(AGV/Inspect)에서 몇번 카메라인지 저장
   
    public int magnification = 1; // 시간 배율
    public float base_time = 0.01f;
    public int time = 0;
    public int time_magnification = 100;
    public string Camera_Type = "Scene";
    public bool cooldown = true;
    private int currentFrame = 0;
    // agv를 설정하는 함수
    void _setting_agv(Vector3 wait_place, int num_agvs, List<base_agv> agv_list)
    {
        // json에서 읽어온 데이터를 바탕으로 base_agv instance를 만들어 원소로 저장함
        for(int i = 0; i<num_agvs;i++)
        {
            // base_agv의 instance를 생성
            var temp_agv = ScriptableObject.CreateInstance<base_agv>();
            temp_agv.wait_place = wait_place;
            // agv를 세팅
            temp_agv.set();
            // 생성된 agv를 agv_list에 추가
            agv_list.Add(temp_agv);
        }      
    }
    // machine을 설정하는 함수
    void _setting_machine(string part, int num_machines, Dictionary<string, Vector3> machine_pos, Vector3 Base, List<machine> machine_list)
    {
        for(int i = 0; i<num_machines; i++)
        {
            var temp_machine = ScriptableObject.CreateInstance<machine>();
            temp_machine.set(i, machine_pos[$"{part}_Machine_{i}"], Base);
            machine_list.Add(temp_machine);
        }
    }
    // inspection을 설정하는 함수
    void _setting_inspection(GameObject Line, int num_inspector, List<inspection> inspection_inspector)
    {
        // json에서 읽어온 데이터를 바탕으로 inpection instance를 만들어 원소로 저장함
        for(int i = 1; i<=num_inspector; i++)
        {
            // inspection의 instance를 생성
            var temp_inspector = ScriptableObject.CreateInstance<inspection>();
            // inspection의 정보를 설정
            temp_inspector.parent_line = Line;
            temp_inspector.set(i);  
            // inspection에 대한 정보에 추가      
            inspection_inspector.Add(temp_inspector);
        }      
    }

    void Start()
    {
        this.displaytext.text = "";
        this.speeddisplay.text = "Now_Speed: X1";
        this.Current_Camera = this.mainCamera;

        // instance를 비어있던 json data와 scan_env에 할당
        this.json_data_alt = ScriptableObject.CreateInstance<json_read_alt>();
        this.scan_env  = ScriptableObject.CreateInstance<scan>();

        // json 파일을 C# dictionary구조로 변경
        this.json_data_alt.convert_json_to_dictionary();

        // 각 부서별 agv 설정
        this._setting_agv(this.json_data_alt.Bases["Build_Base"], this.json_data_alt.config.Num_Build_Agvs, this.build_agvs);
        this._setting_agv(this.json_data_alt.Bases["Wash_Base"], this.json_data_alt.config.Num_Wash_Agvs, this.wash_agvs);
        this._setting_agv(this.json_data_alt.Bases["Dry_Base"], this.json_data_alt.config.Num_Dry_Agvs, this.dry_agvs);
        this._setting_agv(this.json_data_alt.Bases["Return_Base"], this.json_data_alt.config.Num_Return_Agvs, this.return_agvs);
        
        // 각 부서별 machine 설정
        this._setting_machine("Build", this.json_data_alt.config.Num_Build_Machine, this.json_data_alt.Machines, this.json_data_alt.Bases["Build_Base"], this.build_machine);
        this._setting_machine("Wash", this.json_data_alt.config.Num_Wash_Machine, this.json_data_alt.Machines, this.json_data_alt.Bases["Wash_Base"],this.wash_machine);
        this._setting_machine("Dry", this.json_data_alt.config.Num_Dry_Machine, this.json_data_alt.Machines, this.json_data_alt.Bases["Dry_Base"], this.dry_machine);
        
        // inspection 설정정
        this._setting_inspection(this.Line_Inspect, this.json_data_alt.config.Num_Inspector, this.inspection_inspector);
        
        
        // fixedDeltaTime: 1 프레임이 업데이트 될 때까지 걸리는 단위 시간
        Time.fixedDeltaTime = 0.001f;   
    }
        
    void FixedUpdate()
    {
        this.Keyboard_input();// 숫자 키 입력에 대한 기능 수행
        this.Scan_Env(); // 환경을 읽고 명령들을 수행
        
        // 버튼이 눌렸는지 확인 10000프레임에 한번 버튼을 누를 수 있음
        if (Time.frameCount % 10000 == 0)
        {
            this.cooldown = true;
        }
        // 버튼의 쿨다운을 설정
        if (this.cooldown)
        {
            this.Buttons();
        }
       camera_tracking();
       if(Time.frameCount%this.time_magnification == 1)
        {
            this.time += 1;
        }
    }
    void Keyboard_input()
    {
        // 각 숫자 키가 입력 되었는지 확인
        if (Input.GetKey(KeyCode.Alpha1))
        {
            this.info_key = "Agv";
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            this.info_key = "Machine";
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            this.info_key = "Inspect";
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            this.info_key = "Stacker";
        }
        else if (Input.GetKey(KeyCode.Alpha5))
        {
            this.info_key = "Clear";
        }

        // 입력된 명령을 실행행
        if (this.info_key == "Agv")
        {
            this.displaytext.text = $"Total_Build_Agvs: {this.json_data_alt.config.Num_Build_Agvs}\n"
            + $"Work_Build_Agvs: {this.info.Work_Build_Agvs}\n"
            + $"Total_Wash_Agvs: {this.json_data_alt.config.Num_Wash_Agvs}\n"
            + $"Work_Wash_Agvs: {this.info.Work_Wash_Agvs}\n"
            + $"Total_Dry_Agvs: {this.json_data_alt.config.Num_Dry_Agvs}\n"
            + $"Work_Dry_Agvs: {this.info.Work_Dry_Agvs}\n"
            + $"Total_Return_Agvs: {this.json_data_alt.config.Num_Return_Agvs}\n"
            + $"Work_Return_Agvs: {this.info.Work_Return_Agvs}\n";
        }
        else if (this.info_key == "Machine")
        {
            this.displaytext.text = $"Total_Build_Machines: {this.json_data_alt.config.Num_Build_Machine}\n"
             + $"Work_Build_Machines: {this.info.Work_Build_Machines}\n"
             + $"Total_Wash_Machines: {this.json_data_alt.config.Num_Wash_Machine}\n"
             + $"Work_Wash_Machines: {this.info.Work_Wash_Machines}\n"
             + $"Total_Dry_Machines: {this.json_data_alt.config.Num_Dry_Machine}\n"
             + $"Work_Dry_Machines: {this.info.Work_Dry_Machines}\n";
        }
        else if (this.info_key == "Inspect")
        {
            this.displaytext.text = $"Total_Build_Inspectors: {this.json_data_alt.config.Num_Inspector}\n"
             + $"Work_Inspectors: {this.info.Work_Inspectors}\n";
        }
        else if (this.info_key == "Clear")
        {
            this.displaytext.text = "";
        }
        else if (this.info_key == "Stacker")
        {
            this.displaytext.text = $"Stacker 1: {Stacker_info["Stacker 1"]}\n"
            + $"Stacker 2: {Stacker_info["Stacker 2"]}\n"
            + $"Stacker 3: {Stacker_info["Stacker 3"]}\n"
            + $"Stacker 4: {Stacker_info["Stacker 4"]}\n"
            + $"Stacker 5: {Stacker_info["Stacker 5"]}\n";
        }
    }
    void Scan_Env()
    {
         // 현재 시뮬레이터의 환경을 읽어서, 특정 agv를 이동시킬지, 명령을 내릴지 결정
        this.info.Work_Build_Agvs = this.scan_env.scan_agv(this.json_data_alt.buildAgvData, this.time, this.time_magnification, this.build_agvs);
        this.info.Work_Dry_Agvs = this.scan_env.scan_agv(this.json_data_alt.dryAgvData, this.time, this.time_magnification, this.dry_agvs);
        this.info.Work_Wash_Agvs = this.scan_env.scan_agv(this.json_data_alt.washAgvData, this.time, this.time_magnification, this.wash_agvs);
        this.info.Work_Return_Agvs = this.scan_env.scan_agv(this.json_data_alt.returnAgvData, this.time, this.time_magnification, this.return_agvs);
        // 현재 시뮬레이터의 환경을 읽어서, 특정 inspector를 이동시킬지, 명령을 내릴지 결정
        this.info.Work_Inspectors = this.scan_env.scan_inspect(this.json_data_alt.inspectData, this.time, this.time_magnification, this.inspection_inspector);
        // 현재 시뮬레이터의 환경을 읽어서, 특정 machine을 작동시킬지, 중단시킬지 결정
        this.info.Work_Build_Machines = this.scan_env.scan_machine(this.json_data_alt.buildMachineData, this.time, this.time_magnification, this.build_machine);
        this.info.Work_Wash_Machines = this.scan_env.scan_machine(this.json_data_alt.washMachineData, this.time, this.time_magnification, this.wash_machine);
        this.info.Work_Dry_Machines = this.scan_env.scan_machine(this.json_data_alt.dryMachineData, this.time, this.time_magnification, this.dry_machine);
        this.scan_env.scan_stacker(this.json_data_alt.StackerLog, this.time ,this.Stacker_info);
    }
    void Buttons()
    {
        
        this.Quit_Button.GetComponent<Button>().onClick.AddListener(() => Application.Quit());
        this.Button_0.GetComponent<Button>().onClick.AddListener(() => stop_button());
        this.Button_1.GetComponent<Button>().onClick.AddListener(() => speed_button(1));
        this.Button_2.GetComponent<Button>().onClick.AddListener(() => speed_button(2));
        this.Button_3.GetComponent<Button>().onClick.AddListener(() => speed_button(4));
        this.Button_4.GetComponent<Button>().onClick.AddListener(() => speed_button(10));

        this.Scene_Button.GetComponent<Button>().onClick.AddListener(() => camera_button("Scene"));
        this.Build_Button.GetComponent<Button>().onClick.AddListener(() => camera_button("Build"));
        this.Wash_Button.GetComponent<Button>().onClick.AddListener(() => camera_button("Wash"));
        this.Dry_Button.GetComponent<Button>().onClick.AddListener(() => camera_button("Dry"));
        this.Return_Button.GetComponent<Button>().onClick.AddListener(() => camera_button("Return"));
        this.Inspect_Button.GetComponent<Button>().onClick.AddListener(() => camera_button("Inspect"));
       
        if(this.Camera_Type != "Scene" && this.cooldown == true)
        {
            this.Plus_Button.GetComponent<Button>().onClick.AddListener(() => Up_Down("Up"));
            this.Minus_Button.GetComponent<Button>().onClick.AddListener(() => Up_Down("Down"));   
            this.cooldown = false;
        }
    }
    void stop_button()
    {
        Time.timeScale = 0f;
        this.speeddisplay.text = "Now Stopped";
    }
    void speed_button(int input_button)
    {
        Time.timeScale = 1f;
        //RectTransform rectTransform = this.Button_1.GetComponent<RectTransform>();
        //Debug.Log(rectTransform.anchoredPosition);
        if(input_button == 1)
        {
            this.magnification = 1;
            this.speeddisplay.text = "Now_Speed: X1";
            this.time_magnification = 100;
        }
        else if(input_button == 2)
        {
            this.magnification = 2;
            this.speeddisplay.text = "Now_Speed: X2";
            this.time_magnification = 50;
        }
        else if(input_button == 4)
        {
            this.magnification = 4;
            this.speeddisplay.text = "Now_Speed: X4";
            this.time_magnification = 25;
        }

        else if(input_button == 10)
        {
            this.magnification = 10;
            this.speeddisplay.text = "Now_Speed: X10";
            this.time_magnification = 10;
        }

        foreach(var agv in this.build_agvs)
        {
            agv.elapsed_time = this.base_time*this.magnification;
        }
        foreach(var agv in this.dry_agvs)
        {
            agv.elapsed_time = this.base_time*this.magnification;
        }
        foreach(var agv in this.wash_agvs)
        {
            agv.elapsed_time = this.base_time*this.magnification;
        }
        foreach(var agv in this.return_agvs)
        {
            agv.elapsed_time = this.base_time*this.magnification;
        }
        
        foreach(var inspector in this.inspection_inspector)
        {
            inspector.elapsed_time = this.base_time*this.magnification;
        }
        
    }
    void camera_button(string Camera_Type)
    {
        this.Camera_Type = Camera_Type;
        this.Number_Camera = 0;
        switch(this.Camera_Type)
        {
            case "Scene":
                this.Camera_Type = "Scene";
                this.cameratext.text = "Camera: Scene";
                this.Current_Camera = this.mainCamera;
                this.mainCamera.transform.SetPositionAndRotation(this.Scene_camera.transform.position, this.Scene_camera.transform.rotation);
                break;
            case "Build":
                agv_camera_change(this.build_agvs);
                camera_tracking();
                break;
            case "Wash":
                agv_camera_change(this.wash_agvs);
                camera_tracking();
                break;
            case "Dry":
                agv_camera_change(this.dry_agvs);
                camera_tracking();
                break;
            case "Return":
                agv_camera_change(this.return_agvs);
                camera_tracking();
                break;
            case "Inspect":
                inspect_camera_change();
                camera_tracking();
                break;
        }
            
    }
    
    void Up_Down(string direction)
    {
        switch(this.Camera_Type)
        {
            case "Build":
                if(direction == "Up" && this.Number_Camera + 1 < this.json_data_alt.config.Num_Build_Agvs)
                {
                    this.Number_Camera++;
                }
                else if(direction =="Down" && this.Number_Camera-1>=0)
                {
                    this.Number_Camera--;
                }
                agv_camera_change(this.build_agvs);
                break;
            case "Wash":
                if(direction == "Up" && this.Number_Camera + 1 < this.json_data_alt.config.Num_Wash_Agvs)
                {
                    this.Number_Camera++;
                }
                else if(direction =="Down" && this.Number_Camera-1>=0)
                {
                    this.Number_Camera--;
                }
                agv_camera_change(this.wash_agvs);
                break;
            case "Dry":
                if(direction == "Up" && this.Number_Camera + 1 < this.json_data_alt.config.Num_Dry_Agvs)
                {
                    this.Number_Camera++;
                }
                else if(direction =="Down" && this.Number_Camera-1>=0)
                {
                    this.Number_Camera--;
                }
                agv_camera_change(this.dry_agvs);
                break;
            case "Return":
                if(direction == "Up" && this.Number_Camera + 1 < this.json_data_alt.config.Num_Return_Agvs)
                {
                    this.Number_Camera++;
                }
                else if(direction =="Down" && this.Number_Camera-1>=0)
                {
                    this.Number_Camera--;
                }
                agv_camera_change(this.return_agvs);
                break;
            case "Inspect":
                if(direction == "Up" && this.Number_Camera + 1 < this.json_data_alt.config.Num_Inspector)
                {
                    this.Number_Camera++;
                }
                else if(direction =="Down" && this.Number_Camera-1>=0)
                {
                    this.Number_Camera--;
                }
                inspect_camera_change();
                break;
        }      
        
    }
    void agv_camera_change(List<base_agv> selected_agv)
    {
        if(selected_agv[this.Number_Camera].model != null)
        {
            this.tracking_agv = selected_agv[this.Number_Camera];
            this.Current_Camera = this.tracking_agv.agvCamera; 
            this.cameratext.text = $"Camera: {this.Camera_Type}_{this.Number_Camera}";
            this.mainCamera.transform.SetPositionAndRotation(this.Current_Camera.transform.position, this.Current_Camera.transform.rotation);
        }
        else
        {
            this.mainCamera.transform.SetPositionAndRotation(this.Current_Camera.transform.position, this.Current_Camera.transform.rotation);
        }
   
    }
    void inspect_camera_change()
    {
        if(this.inspection_inspector[this.Number_Camera].model != null)
        {
            this.tracking_inspector = this.inspection_inspector[this.Number_Camera];
            this.Current_Camera = this.tracking_inspector.inspectionCamera;
            this.cameratext.text = $"Camera: {this.Camera_Type}_{this.Number_Camera}";
            this.mainCamera.transform.SetPositionAndRotation(this.Current_Camera.transform.position, this.Current_Camera.transform.rotation);
        }
        else
        {
            this.mainCamera.transform.SetPositionAndRotation(this.Current_Camera.transform.position, this.Current_Camera.transform.rotation);
        }
    }
    void camera_tracking()
    {
        if(this.Camera_Type != "Scene")
        {
            if(this.Camera_Type != "Inspect" && this.tracking_agv != null)
            {
                this.Current_Camera = this.tracking_agv.agvCamera; 
                
            } 
            else if(this.Camera_Type == "Inspect" && this.tracking_inspector != null)
            {
                this.Current_Camera = this.tracking_inspector.inspectionCamera;
            }
            this.mainCamera.transform.SetPositionAndRotation(this.Current_Camera.transform.position, this.mainCamera.transform.rotation);
        }       
           
    }
}
    
