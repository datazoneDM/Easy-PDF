using DevExpress.XtraEditors;
using H2PControl.Events;
using H2PControl.Handlers;
using H2PControl.Models;
using H2PControl.Sockets;
using iText.Forms;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace H2PControl
{
    public partial class H2PControl : DevExpress.XtraEditors.XtraForm
    {
        private FileStream Logfs;
        private StreamWriter Logwr;
        string 로그내용;
        private int _currentLogDay = -1;
        private static int 실행상태 = 0;
        private ChatServer _server;
        private ClientRoomManager _roomManager;
        private ChatClient _client;
        private ClientHandler _clientHandler;
        public string 실행주체 = "";
        private int C_RoomId;
        private string C_UserName;
        private ListBoxControl[] 리스트;
        private bool[] errorStates;
        private System.Drawing.Image[] errorIcon = new System.Drawing.Image[2];
        private System.Drawing.Image ViewerrorIcon = null;
        private 환경설정 설정 = new 환경설정();
        private string 파라메타 = "";
        private Dictionary<string, int> 접속컴 = new Dictionary<string, int>();
        private System.Windows.Forms.Timer monitorTimer;
        private bool isRestarting = false;
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
        private const int PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        public H2PControl(string[] args)
        {
            InitializeComponent();
            if (args.Length >= 1)
                파라메타 = args[0];
        }

        private void H2PControl_Load(object sender, EventArgs e)
        {
            // 프로그램이 켜져 있는 동안 절대 잠들지 않게 설정
            PreventSleep.DonSleep();
            int X = 0;
            int Y = 570;
            this.Location = new System.Drawing.Point(X, Y);
            this.Size = new System.Drawing.Size(518, 465);
            this.Opacity = 0.8;
            리스트박스초기화();
            일하는순서();
        }          

        private async void 일하는순서()
        {
            저장.로그경로 = @"c:\Easy약관Plus\program\LogFile\";

            // ==============================================
            //테스트 1
            //저장.사용컴퓨터 = "kbmain";
            //저장.사용컴퓨터 = "kbsub1";
            // ==============================================
                       

            로그파일열기();
            설정 = new 환경설정();
            기본설정(설정);

            // ======================================================================================================================
            // 프로그램이 2분류로 구분 
            // 1) H2PControl : F1 ~ F8 앱을 감시 실행, PDF병합
            // 2) C/S        : Server, Client로 각종 메세지를 전달하고 파일을 송/수신을 한다.
            // ======================================================================================================================

            if (설정.병렬처리여부)
            {
                if (설정.상태컴퓨터 == "main")
                {
                    실행주체 = "서버";
                    TabControl.SelectedTabPage = Tabserver;
                }
                else
                {
                    btnSendFile.Enabled = true;
                    실행주체 = "클라이언트";
                    TabControl.SelectedTabPage = Tabclient;
                }

                //서버/클라이언트 별로 접속을 한다.
                if (await TCPServer_Client초기화(실행주체, "시작", 설정) != "성공")
                {
                    로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[오류] {실행주체} 환경설정 오류";
                    로그기록(로그내용, 1);
                    return;
                } else
                {
                    // '맞춤약관앱' 감시를 시작합니다.
                    this.Invoke(new MethodInvoker(delegate
                    {
                        StartMonitoring();
                    }));
                }
            }
            

            while (true)
            {

                try
                {
                    // 1)감시\groupKey.단계01 감시
                    // 2)List파일 감시 후 분할전송(서버==>클라이언트)
                    // 초기화
                    현재상태 상태 = new 현재상태(설정);
                    if (DateTime.Now.Day != _currentLogDay)
                    {
                        // 날짜가 바뀌었으면 파일을 닫고 새로 엽니다.
                        로그파일열기();
                    }

                    if (await 단계01파일검색_파일분리(상태) == "실패")
                    {
                        throw new Exception("🚫진행불가 : 단계01파일검색_파일분리 오류, 로그참조");
                    }

                    // ==========================================================================================================
                    // 02 - data처리 Table를 검색해서 데이터를 처리한다.
                    // ==========================================================================================================
                    bool 선택약관PDF병합샐행 = false;

                    if (await data처리Table검색(상태, "HWP2PDF"))
                    {
                        저장.출력폴더 = 상태.출력폴더;

                        // ==========================================================================================================
                        // 03 - F1 ~ F8 Hwp to PDF 변환 앱을 실행한다.
                        //      8개의 앱이 실행되면 최대 10번까지 오류처리를 한다.
                        //      F1 ~ F8을 실행만 하는 역할 데이터가 몇개인지는 불필요하다 
                        //      F1앱이 핫폴더 개념이므로 Hwp파일이 있으면 PDF로 변환한다.
                        //      변환하는 과정에서 오류가 발생하면 data 상태값을 'PDF변환오류'로 변경한다. 
                        // ==========================================================================================================
                        if (await F1실행관리(상태))
                        {
                            // ==========================================================================================================
                            // 한글을 PDF로 변환이 완료 되면 반드시 'PDF병합'을 실행한다.
                            // F1 ~ F8앱이 모두 종료되어야 'data처리Table검색' 실행된다.
                            // ==========================================================================================================
                            선택약관PDF병합샐행 = true;
                        }
                    } else
                    {
                        throw new Exception("🚫진행불가 : data처리Table검색 오류, 로그참조");
                    }

                    if (선택약관PDF병합샐행 || await data처리Table검색(상태, "PDFMERGE"))
                    {
                        // ==========================================================================================================
                        // 04 - 01 선택약관PDF병합 : 세션별로 되어 있는 PDF파일을 하나로 병합 하는 단계
                        // ==========================================================================================================
                        if (await 선택약관PDF병합(상태) == false)
                        {
                            throw new Exception("🚫진행불가 : 선택약관PDF병합 오류, 로그참조");
                        }
                    }

                    string 단계완료파일 = $"{저장.기본출력폴더}감시\\{상태.groupKey}.단계완료";
                    File.WriteAllText(단계완료파일, "H2PControl 완료", Encoding.GetEncoding(949));

                    if (상태.병렬컴 >= 2)
                    {
                        BtnStart.Enabled = true;
                        BtnStop.Enabled = true;
                        btnSendFile.Enabled = true;

                        _ = await 서버수신대기_클라이언트파일송신(상태, 실행주체);
                    }

                    if (실행주체 == "클라이언트")
                    {
                        _clientHandler?.Send(new ChatHub
                        {
                            RoomId = C_RoomId,
                            UserName = C_UserName,
                            Message = "[작업완료] 개별약관 작업이 완료 되었습니다.",
                            상황 = "완료",
                            State = ChatState.진행상황
                        });
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                catch (Exception ex)
                {
                    로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[ERROR] H2PControl 앱 오류 - {ex.Message}";
                    로그기록(로그내용, 1);

                }
                
            }
        }


        // =======================================================================================================================================
        //                                                     [ 프로그램 분리]
        // =======================================================================================================================================

        // ==========================================================================================================
        // 00 StartMonitoring : 각각의 모니터링을 한다.
        // ==========================================================================================================
        private void StartMonitoring()
        {
            if (monitorTimer == null)
            {
                monitorTimer = new System.Windows.Forms.Timer();
                monitorTimer.Interval = 3000; // 3초마다 체크
                monitorTimer.Tick += MonitorTimer_Tick;
            }
            monitorTimer.Start();
        }

        private async void MonitorTimer_Tick(object sender, EventArgs e)
        {
            if (isRestarting) return;

            // 감시 대상 파일 경로
            string statusPath = @"d:\Easy약관Plus\Output\STATUS\앱감시상태.txt";
            string finalStatus = "OFF";
            string finalDetail = "";
            bool needsRestart = false; // 재시작 필요 여부

            try
            {
                if (File.Exists(statusPath))
                {
                    // 파일 읽기 (다른 프로세스 사용 중일 수 있으니 공유 모드로 읽기)
                    using (var fs = new FileStream(statusPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.GetEncoding(949)))
                    {
                        string content = sr.ReadToEnd();
                        string[] parts = content.Split('|'); // [0]시간, [1]상태, [2]내용

                        if (parts.Length >= 2)
                        {
                            DateTime lastTime = DateTime.Parse(parts[0]);
                            string appState = parts[1];
                            string appDetail = parts.Length > 2 ? parts[2] : "";

                            // ★ 상태 판단 로직
                            TimeSpan diff = DateTime.Now - lastTime;

                            if (diff.TotalSeconds > 600) // 600초 이상 갱신 안됨 -> 멈춤, 재시작 고려
                            {
                                finalStatus = "FROZEN";
                                finalDetail = $"응답없음({diff.TotalSeconds:F0}초)";
                                needsRestart = true; // 재시작 플래그 ON
                            }
                            else // WAITING, RUNNING, ERROR, RECORDERR
                            {
                                finalStatus = appState;
                                finalDetail = appDetail;
                            }
                        }
                    }
                }
            }
            catch
            {
                finalStatus = "CHECK_FAIL"; // 파일 읽기 실패
            }

            if (실행주체 == "클라이언트")
            {
                // 서버로 전송
                // _clientHandler가 연결되어 있을 때만 보냅니다.
                if (_clientHandler != null && (finalStatus == "FROZEN" || 
                        finalStatus == "ERROR" || finalStatus == "RECORDERR"))
                {
                    // 룸ID에 맞춰서 메시지를 보냅니다.
                    // 서버는 이 RoomId를 보고 lstH2P_1 ~ 5 중 어디에 뿌릴지 결정합니다.
                    _clientHandler.Send(new ChatHub
                    {
                        RoomId = C_RoomId,                            // 예: 1 (Sub1)
                        UserName = C_UserName,                        // 예: Sub1
                        State = ChatState.진행상황,                   // 상태 업데이트용으로 사용
                        상황 = "오류",                                
                        Message = $"[{finalStatus}]{finalDetail}"     // Step-03, 오류내용 ...
                    });
                }
            }

            if (finalStatus == "FROZEN" || finalStatus == "ERROR" || finalStatus == "RECORDERR")
            {
                string message = $"[{finalStatus}]{finalDetail}";
                int 표시 = 1;
                로그기록($"{DateTime.Now:yy.MM.dd HH:mm:ss}{message}", 표시, 0);
                SetError(true, 0, finalStatus);
            }

            if (needsRestart) 
            {
                // 중복 실행 방지 락 걸기
                isRestarting = true;
                monitorTimer.Stop(); 

                bool success = await Easy약관Plus실행();
                await Task.Delay(10000); // 실행시간 때문에 10초대기 
                                         // 감시 재개
                isRestarting = false;
                monitorTimer.Start();
            }
        }


        // ==========================================================================================================
        // 01 기본화면설정 : 스레드갯수 설정
        // ==========================================================================================================
        private void 기본설정(환경설정 설정)
        {

            string 임시컴 = SystemInformation.ComputerName.ToLower();
            if (파라메타 == "앱실행_싱글처리")
            {
                if (임시컴.StartsWith("kbsub"))
                    저장.사용컴퓨터 = "kbmain";
                else if (임시컴.StartsWith("sub"))
                    저장.사용컴퓨터 = "main";
                else if (임시컴.StartsWith("dzsub"))
                    저장.사용컴퓨터 = "dzmain";
                else
                    저장.사용컴퓨터 = 임시컴;
            }
            else
                저장.사용컴퓨터 = 임시컴;

            //디폴트 저장 인수값
            저장.DB설정파일path = @"c:\Easy약관Plus\program\localDB.txt";
            저장.기본출력폴더 = @"d:\Easy약관Plus\Output\";
            저장.기본입력폴더 = @"d:\Easy약관Plus\List\";
            저장.기본파일폴더 = @"d:\Easy약관Plus\Input\";


            //모두 3곳을 수정해야 한다.(맞춤약관, H2PControl, F1)
            Dictionary<string, string> 설정값 = new Dictionary<string, string>();
            if (new System.IO.FileInfo(저장.DB설정파일path).Exists)
            {
                string[] 설정파일 = System.IO.File.ReadAllLines(저장.DB설정파일path, Encoding.GetEncoding(949));
                foreach (string tmp설정 in 설정파일)
                {
                    string[] 분리 = tmp설정.Split('\t');
                    if (분리.Length < 2) continue;

                    string key = 분리[0].Trim();
                    string value = 분리[1].Trim();
                    if (key == "접속" || key == "FTP" || key == "소켓" || key == "모바일" || key == "약관" || key == "PDF")
                    {
                        if (분리.Length < 3) continue;

                        string 키1 = 분리[1].Trim();
                        string 값1 = 분리[2].Trim();

                        if (key == "접속" || key == "소켓") {
                            키1.Split('|')
                               .ToList()
                               .ForEach(x =>
                               {
                                   if (!설정값.ContainsKey($"{key}{x}"))
                                       설정값.Add($"{key}{x}", 값1);
                               });
                        } else if (key == "FTP") {
                            if (!설정값.ContainsKey($"{key}{키1}"))
                                설정값.Add($"{key}{키1}", 값1);
                        }
                        
                    }
                    else if (key == "병렬") {
                        value.Split('|')
                             .Where(x => x.Contains("main") || x.Contains("sub"))
                             .ToList()
                             .ForEach(x =>
                             {
                                 if (!설정값.ContainsKey(($"{key}{x}")))
                                     설정값.Add($"{key}{x}", x.Contains("main") ? "main" : "sub");
                             });
                    }
                    else {
                        if (!설정값.ContainsKey(key))
                            설정값.Add(key.ToUpper(), value);
                    }
                }
            }

            try
            {
                string 컴유형 = "";
                if (설정값.TryGetValue($"병렬{저장.사용컴퓨터}", out string tmp컴유형))
                    컴유형 = tmp컴유형;

                설정.병렬처리여부 = 컴유형 == "" ? false : true;

                //모두 3곳을 수정해야 한다.(맞춤약관, H2PControl, F1)
                switch (컴유형)
                {
                    //병렬처리
                    case "sub":
                        //끝에서 4글자
                        설정.상태컴퓨터 = 저장.사용컴퓨터.Substring(저장.사용컴퓨터.Length - 4, 4); // ex) sub1, sub2, sub3,..... 반드시 병렬컴퓨터는 컴퓨터명을 sub포함해야 한다..
                        int 인수 = 설정.상태컴퓨터.IndexOf("sub") + 3;
                        int 숫자 = -1;
                        if (int.TryParse(설정.상태컴퓨터.Substring(인수), out 숫자))
                        {
                            설정.처리구분 = string.Format("{0:00}", 숫자);
                        }
                        C_RoomId = 숫자;
                        C_UserName = 설정.상태컴퓨터;
                        break;
                    default:
                        설정.상태컴퓨터 = "main";   // main컴, 일반컴 모두 main컴으로 처리
                        설정.처리구분 = "00";
                        break;
                }

                //data테이블 접속관련
                if (설정값.TryGetValue($"접속{저장.사용컴퓨터}", out string 접속) && 파라메타 != "앱실행_싱글처리")
                    설정.dataTable접속 = 접속;
                 else
                    설정.dataTable접속 = 설정값["접속LOCALDB"];

                //추후 KB약관을 따로 분리할때 고려할것
                //ex)약관KB2
                if (설정값.TryGetValue($"약관전체", out string 약관))
                    저장.약관DB = 약관;



                // ======= [ H2PControl만 해당 ] =======
                //소켓통신 접속관련
                if (파라메타 == "앱실행_싱글처리")
                {
                    //로컬IP로 대체한다.
                    설정.접속IP = "127.0.0.1";
                    설정.접속PORT = 8080;
                } else {
                    if (설정값.TryGetValue($"소켓{저장.사용컴퓨터}", out string 소켓))
                    {
                        string[] 소켓분리 = 소켓.Split('|');
                        if (소켓분리.Length == 2)
                        {
                            설정.접속IP = 소켓분리[0].Trim();
                            설정.접속PORT = Convert.ToInt32(소켓분리[1]);
                        }
                    }
                }

                if (설정.상태컴퓨터 == "main")
                    this.Text = "H2PControl (With 맟춤약관)   SERVER";
                else
                    this.Text = $"H2PControl (With 맞춤약관)   CLIENT[{설정.상태컴퓨터}]";
             
            }
            catch (Exception ee)
            {
                로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[기본설정 Er02] {1}==>{2}", DateTime.Now, ee.Message, ee.StackTrace);
                로그기록(로그내용, 1);
            }
        }

        // ==========================================================================================================
        //  02 -01  *.단계02파일 검색
        // ==========================================================================================================
        private async Task<string> 단계01파일검색_파일분리(현재상태 상태)
        {
            int 재시도 = 0;
            string 실행여부 = "";

            // 재시작     : 컴이 다운현상으로     ==> 감시\*.단계02
            // 오류재처리 : 오류로 인해 다시 처리 ==> @groupKey 파일

            //재시작(끊김현상대비)
            DirectoryInfo dir = new DirectoryInfo($"{저장.기본출력폴더}감시\\");
            foreach (FileInfo 체크파일 in dir.GetFiles("*.단계02"))
            {
                string 체크파일명 = 체크파일.FullName.Replace(".단계02", ".작업완료");
                if (!new FileInfo(체크파일명).Exists)
                {
                    string 내용 = File.ReadAllText(체크파일.FullName, Encoding.GetEncoding(949));
                    string[] 분리 = 내용.Split('|');
                    if (분리.Length == 2)
                    {
                        상태.고객사 = 분리[0].Trim();
                        상태.groupKey = 분리[1].Trim();
                        실행여부 = "성공";
                        return 실행여부;
                    }
                }
            }

            //생성이유 : 오류재처리시 H2P앱에서 '단계01파일검색_파일분리'에서 약관생성(03|완료) 이후 이면 
            //           '단계01'(약관생성에서 생성됨) 생성되지 않아 무한 대기 상태
            // 약관생성(상태) 메소드 다음부분 참조
            int 단계03 = dir.GetFiles("*.단계03").Count();
            if (단계03 > 0)
            {
                실행여부 = "성공";
                return 실행여부;
            }

            string[] tmp감시 = new string[2];
            tmp감시[0] = $"{저장.기본출력폴더}감시\\";
            tmp감시[1] = 저장.기본파일폴더;  //@"d:\Easy약관Plus\Input\";
            bool 오류재처리여부 = false;
            //우선순위 : 단계01 ==> 파일분리
            while (true)
            {

                int tmp병렬cnt = lbxClients.Items.Count; //main만 해당

                try
                {
                    for(int i = 0; i < tmp감시.Length; i++)
                    {

                        DirectoryInfo 감시폴더 = new DirectoryInfo(tmp감시[i]);
                        string 필터 = i == 0 ? "*.단계01" : "*.*";
                        int 작업파일갯수 = 감시폴더.GetFiles(필터).Count();

                        if (작업파일갯수 == 0)
                            continue;

                        string 오류재처리필터 = "@*.*"; // @ : 재처리 파일

                        //실행순서 : I = 1 (파일분리) ==> I = 0  (PDF변환 이동)
                        if (i == 0 && 작업파일갯수 > 0 && 실행여부 != "단계01") // 맞춤약관 실행 후 단계01 ==> 단계02로 변경 하면서 작업 시작
                        {
                            
                            foreach (FileInfo 감시파일 in 감시폴더.GetFiles(필터))
                            {
                                상태.groupKey = System.IO.Path.GetFileNameWithoutExtension(감시파일.Name); //확장자제거

                                string 단계02파일 = 감시파일.FullName.Replace("단계01", "단계02");
                                new FileInfo(감시파일.FullName).CopyTo(단계02파일, true);
                                new FileInfo(감시파일.FullName).Delete();

                                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[작업시작] 선택약관파일(hwp)를 PDF로 변환를 시작합니다.";
                                로그기록(로그내용, 1);
                                this.Opacity = 0.8;
                                실행여부 = "단계01";

                                if (실행주체 == "클라이언트")
                                {
                                    _clientHandler?.Send(new ChatHub
                                    {
                                        RoomId = C_RoomId,
                                        UserName = C_UserName,
                                        Message = "[작업시작] 선택약관파일(hwp)를 PDF로 변환를 시작합니다.",
                                        상황 = "처리",
                                        State = ChatState.진행상황
                                    });
                                } 
                                break;
                            }
                        }
                        else if (i == 1 && 상태.상태컴퓨터 == "main" && 
                            감시폴더.GetFiles(오류재처리필터).Count() > 0 && 오류재처리여부 == false ) // 재처리
                        {
                            foreach (FileInfo 감시파일 in 감시폴더.GetFiles(오류재처리필터))
                            {
                                상태.groupKey = 감시파일.Name.Substring(1);
                                if (tmp병렬cnt >= 1)
                                {
                                    await _server.서버To클라이언트송신("모두", new ChatHub
                                    {
                                        SendType = SendType.ServerToAllFile,
                                        State = ChatState.FileStart,
                                        FileFrom = 저장.기본파일폴더,
                                        플레그 = "S To C : LIST파일전송",
                                        FileFilter = $"@*.*",
                                        UserName = 상태.상태컴퓨터,
                                        RoomId = 0
                                    });
                                }
                                this.Opacity = 0.8;
                                //실행여부 = "단계01";
                                오류재처리여부 = true; // 1번만 처리 함
                                await Task.Delay(TimeSpan.FromSeconds(5));
                                bool 복사여부 = 복사_이동_삭제("복사_삭제", 감시파일.FullName, $"{저장.기본입력폴더}{감시파일.Name}"); //클라이언트로 전송 후 삭제
                                break;
                            }
                        }
                        else if (tmp병렬cnt >= 1 && i == 1 && 작업파일갯수 > 0 && 상태.상태컴퓨터 == "main" && 실행여부 != "파일분리")
                        {
                            List<string> tmp작업파일 = new List<string>();
                            string tmp고객사 = "";
                            bool 처음저장 = false;

                            string 이동폴더 = "";
                            string 이동파일 = "";
                            이동폴더 = 폴더생성(저장.기본파일폴더, "@완료");
                            이동폴더 = 폴더생성(이동폴더, "@파일규칙오류");

                            foreach (FileInfo 파일 in 감시폴더.GetFiles(필터))
                            {
                                if (파일.Name.IndexOf('#') != -1) // 고객사코드는 반드시 #으로 시작한다.
                                    상태.고객사 = 파일.Name.Substring(파일.Name.IndexOf('#') + 1, 2);
                                else
                                {
                                    이동파일 = string.Format("{0}{1}", 이동폴더, 파일.Name);
                                    bool 복사여부 = 복사_이동_삭제("복사_삭제", 파일.FullName, 이동파일);
                                    if (복사여부 == false)
                                        throw new Exception($"파일복사/삭제 오류 : { 파일.FullName}");

                                    로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[오류] 고객사코드는 반드시 #으로 시작 : {1}", DateTime.Now, 파일.Name);
                                    로그기록(로그내용, 1);
                                    continue;
                                }

                                // 처음파일을 기준으로 동일한 고객사를 묶어 한꺼번에 처리 한다.
                                if (처음저장 == false)
                                {
                                    tmp고객사 = 상태.고객사;
                                    처음저장 = true;
                                }

                                if (tmp고객사 == 상태.고객사)
                                {
                                    tmp작업파일.Add(파일.FullName);
                                    if (상태.고객사 == "DB") // DB손해보험 파일명을 기본키값으로 사용해서 Merge를 하지 않는다.(모바일)
                                        break;
                                }
                            }

                            if (tmp작업파일.Count == 0)
                                continue;

                            상태.고객사 = tmp고객사;  // 여러고객사 파일 들어 오면 처리하는 고객사로 원위치 한다.

                            string 처리날짜 = string.Format("{0:yyMMdd}", DateTime.Now);
                            // @완료 폴더생성
                            string 임시이동 = $"@완료\\{상태.고객사}\\{DateTime.Now:yyyy.MM}\\{DateTime.Now:dd}\\";
                            이동폴더 = 폴더생성(저장.기본파일폴더, 임시이동);

                            string 처리파일 = "";
                            string PK중복방지 = string.Format("{0:yyMMdd_HHmmssFFFF}", DateTime.Now);
                            //삭제2 : 리스트파일 삭제(이동)
                            if (tmp작업파일.Count == 1)
                            {
                                처리파일 = new FileInfo(tmp작업파일[0]).Name;
                                이동파일 = 이동폴더 + string.Format("{0}_{1}.Re", PK중복방지, 처리파일);
                                bool 복사여부 = 복사_이동_삭제("복사_삭제", tmp작업파일[0], 이동파일);
                                if (복사여부 == false)
                                {
                                    로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[오류] 복사_삭제 : {1}", DateTime.Now, 처리파일);
                                    로그기록(로그내용, 1);
                                }
                            }
                            else
                            {
                                // 여러개의 파일 존재하면 대표파일명을 새롭게 작성한다.
                                처리파일 = string.Format("#{0}_Merge({1:000}EA).lst", 상태.고객사, tmp작업파일.Count);

                                List<string> 출력내용 = new List<string>();
                                int 파일cnt = 0;
                                foreach (string 파일경로 in tmp작업파일)
                                {
                                    string[] 파일읽기 = File.ReadAllLines(파일경로, Encoding.GetEncoding(949));
                                    출력내용.AddRange(파일읽기);

                                    string 대상경로 = string.Format("{0}{1}_{2}", 이동폴더, PK중복방지, new FileInfo(파일경로).Name);
                                    bool 복사여부 = 복사_이동_삭제("복사_삭제", 파일경로, 대상경로);
                                    if (복사여부 == false)
                                    {
                                        로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[복사삭제] 파일복사/삭제 : {1}", DateTime.Now, 파일경로);
                                        로그기록(로그내용, 1);
                                    }

                                    파일cnt++;
                                    로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[병합시작] ListFile병합시작 : {1:000} - {2} ", DateTime.Now, 파일cnt, new FileInfo(파일경로).Name);
                                    로그기록(로그내용, 1);
                                }

                                이동파일 = 이동폴더 + string.Format("{0}_{1}.Re", PK중복방지, 처리파일);
                                File.WriteAllLines(이동파일, 출력내용, Encoding.GetEncoding(949));

                                로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[파일병합] ListFile병합완료 : {1:000} - {2} ", DateTime.Now, 파일cnt, new FileInfo(이동파일).Name);
                                로그기록(로그내용, 1);
                            }

                            상태.입력폴더 = 이동파일; // d:\Easy약관Plus\Input\@완료\#KB_250726_103620\250726_103620_#KB_Lngtrm.dat.Re
                            로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[파일이동] ListFil이동 :  {1} 이동", DateTime.Now, 이동파일);
                            로그기록(로그내용, 1);


                            string 분리_파일명 = System.IO.Path.GetFileNameWithoutExtension(상태.입력폴더);
                            string 분리_경로 = $"{System.IO.Path.GetDirectoryName(상태.입력폴더)}\\";


                            string[] lines = File.ReadAllLines(상태.입력폴더, Encoding.GetEncoding(949));
                            List<string> uniqueLines = new List<string>(lines.Length);
                            // 고객번호 저장용 HashSet (중복 체크용)
                            HashSet<string> seen = new HashSet<string>();
                            List<string> 중복data = new List<string>();
                            foreach (var line in lines) {
                                string[] 분리 = line.Split('|');
                                if (분리.Length == 0)
                                    continue;

                                string 고객번호 = 분리[0].Trim();
                                if (seen.Add(고객번호))
                                    uniqueLines.Add(line);
                                else
                                    중복data.Add(line);
                            }
                            if (중복data.Count > 0)
                            {
                                string 중복파일명 = $"{분리_경로}[중복]{분리_파일명}";
                                File.WriteAllLines(중복파일명, 중복data);
                            }

                            // data, data처리 Table 내용 삭제
                            string SqlStr = string.Empty;
                            SqlStr = "DELETE FROM data처리";
                            _ = DBUpdate(SqlStr, 상태.dataTable접속);
                            SqlStr = "DELETE FROM data";
                            _ = DBUpdate(SqlStr, 상태.dataTable접속);


                            int total = uniqueLines.Count;
                            int n = total <= 10 ? 1 : tmp병렬cnt + 1 ; // 나눌 개수 ( + 1 : main )
                            int 몫 = total / n;
                            int 나머지 = total % n;
                            string 병렬처리문구 = $"[M{n:00}]";  // 병렬처리중 10건 이하는 싱글처리, 아니면 멀티처리(병렬)
                            int index = 0;

                            상태.병렬컴 = n;

                            for (int ii = 0; ii < n; ii++ )
                            {
                                int 사이즈 = 몫 + (ii < 나머지 ? 1 : 0);

                                // ✅ 배열 복사
                                //string[] part = new string[사이즈];
                                //Array.Copy(lines, index, part, 0, 사이즈);
                                List<string> part = uniqueLines.GetRange(index, 사이즈);

                                index += 사이즈;

                                string 컴구분 = (ii == 0) ? "main" : $"{(string)lbxClients.Items[ii - 1]}"; //main, sub1 ~ sub5
                                string savePath = $"{분리_경로}{병렬처리문구}{분리_파일명}.{컴구분}";
                                File.WriteAllLines(savePath, part, Encoding.GetEncoding(949));
                                await Task.Delay(TimeSpan.FromSeconds(1));

                                if(컴구분 == "main")
                                {
                                    bool 복사여부 = 복사_이동_삭제("복사_삭제", savePath, $"{저장.기본입력폴더}{병렬처리문구}{분리_파일명}.{컴구분}");
                                } else
                                {
                                    int 접속확인 = lbxClients.Items.IndexOf(컴구분);
                                    if (접속확인 == -1)
                                    {
                                        로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[전송오류] {1}컴으로 {2}/{3} LIST파일전송실패", DateTime.Now, 컴구분, ii + 1, 상태.병렬컴);
                                        로그기록(로그내용, 1);
                                        continue;
                                    }

                                    await _server.서버To클라이언트송신(컴구분, new ChatHub
                                    {
                                        SendType = SendType.ServerToOneFile,
                                        State = ChatState.FileStart,
                                        FileFrom = 분리_경로,
                                        플레그 = "S To C : LIST파일전송",
                                        FileFilter = $"*.{컴구분}",
                                        UserName = 상태.상태컴퓨터,
                                        RoomId = 0
                                    });
                                    lbJobs.Items.Add(컴구분);
                                    lbJobs_Display();
                                    await Task.Delay(TimeSpan.FromSeconds (0.2));

                                    if (접속컴.TryGetValue(컴구분, out int ID))
                                    {
                                        로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[전송완료] {1}컴으로 {2}/{3} LIST파일전송 : {4:00000} Line", DateTime.Now, 컴구분, ii + 1, 상태.병렬컴, 사이즈);
                                        로그기록(로그내용, 1, ID);
                                    }
                                }
                            }
                            실행여부 = "파일분리";
                        }
                    }

                    if (실행여부 == "단계01")
                    {
                        실행여부 = "성공";
                        break;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                catch (Exception ex)
                {
                    재시도++;
                    if (재시도 <= 3)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        continue;
                    }
                    실행여부 = "실패";
                    로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[단계01파일검색_파일분리] 오류 : {1} - {2}", DateTime.Now, ex.Message, ex.StackTrace);
                    로그기록(로그내용, 1);
                    

                    int listIndex = -1;
                    if (실행주체 == "클라이언트")
                    {
                        listIndex = 9;
                        _clientHandler?.Send(new ChatHub
                        {
                            RoomId = C_RoomId,
                            UserName = C_UserName,
                            Message = $"[작업오류] {ex.Message}",
                            상황 = "오류",
                            State = ChatState.진행상황
                        });
                    } else
                    {
                        listIndex = 0;
                    }
                    SetError(true, listIndex, "ERROR");
                    break;
                }
            }
            return 실행여부;
        }

        // ==========================================================================================================
        // 02 - 02 data처리 Table를 검색해서 데이터를 처리한다.
        // ==========================================================================================================
        private async Task<bool> data처리Table검색(현재상태 상태, string 작업선택)
        {
            int 재시도 = 0;
            bool 실행여부 = false;
            while (true)
            {
                try
                {
                    string SqlStr = "";
                    string 구분문구 = "";
                    if (작업선택 == "HWP2PDF")
                    {
                        구분문구 = "PDF변환작업 <> 'PDF변환완료'";
                    }
                    else if (작업선택 == "PDFMERGE") {

                        구분문구 = "PDF병합작업 <> 'PDF병합완료'";
                    }
                    else {
                        break;
                    }

                    SqlStr = $@"
                            SELECT * 
                            FROM data처리 
                            WHERE groupKey = '{상태.groupKey}' AND
                                  처리구분 = '{상태.처리구분}' AND
                                  {구분문구}
                    ";

                    using (SqlConnection AdoNet = new SqlConnection(상태.dataTable접속))
                    {
                        await AdoNet.OpenAsync();
                        DataSet ds = new DataSet();
                        DataTable dataTable = new DataTable();
                        SqlCommand sqlcmd = new SqlCommand(SqlStr, AdoNet);
                        sqlcmd.CommandTimeout = 0;
                        SqlDataAdapter adapter = new SqlDataAdapter(sqlcmd);
                        adapter.Fill(ds, "기본");

                        if (ds.Tables["기본"].Rows.Count == 0)
                        {
                            return true;
                        }
                        DataRow row = ds.Tables["기본"].Rows[0];
                        DB2구조체Save(상태, row);
                        실행여부 = true;
                    }
                    break;
                }
                catch (Exception ex)
                {

                    재시도++;
                    if (재시도 <= 3)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        continue;
                    }
                    실행여부 = false;
                    로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[data처리Table검색] 오류 : {1} - {2}", DateTime.Now, ex.Message, ex.StackTrace);
                    로그기록(로그내용, 1);


                    int listIndex = -1;
                    if (실행주체 == "클라이언트")
                    {
                        listIndex = 9;
                        _clientHandler?.Send(new ChatHub
                        {
                            RoomId = C_RoomId,
                            UserName = C_UserName,
                            Message = $"[작업오류] {ex.Message}",
                            상황 = "오류",
                            State = ChatState.진행상황
                        });
                    }
                    else
                    {
                        listIndex = 0;
                    }
                    SetError(true, listIndex, "ERROR");
                    break;
                }
            }
            return 실행여부;
        }

        // ==========================================================================================================
        // 02 - 2 : DB to 구조체 Save
        // ==========================================================================================================
        private void DB2구조체Save<T>(T target, DataRow row)
        {
            var props = typeof(T).GetProperties();

            foreach (var prop in props)
            {
                if (row.Table.Columns.Contains(prop.Name) && prop.CanWrite)
                {
                    var value = row[prop.Name];
                    if (value != DBNull.Value)
                    {
                        prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType));
                    }
                }
            }
        }

        // ==========================================================================================================
        // 03 - 01 : F1 ~ F8 Hwp to PDF 변환 앱을 실행한다.
        //           8개의 앱이 실행되면 최대 10번까지 오류처리를 한다.
        // ==========================================================================================================
        private async Task<bool> F1실행관리(현재상태 상태)
        {
            // 중복 실행 차단
            if (Interlocked.Exchange(ref 실행상태, 1) == 1)
            {
                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[제어앱] 이미 감시 루프 실행 중";
                로그기록(로그내용, 1);
                return false;
            }

            bool 실행여부 = false;
            int 앱수 = 상태.앱실행수; // 최대 8            
            using (var cts = new CancellationTokenSource()) 
            {
                try
                {
                    List<Task<bool>> 감시작업목록 = new List<Task<bool>>();
                    // 앱별 감시 시작
                    for (int i = 0; i < 앱수; i++)
                    {
                        int index = i + 1;
                        감시작업목록.Add(Task.Run(() => F앱감시루프(상태, index, cts.Token)));
                    }

                    // 모든 앱이 정상완료될 때까지 대기
                    bool[] 결과 = await Task.WhenAll(감시작업목록);
                    
                    // 약관완료 감시 (모든 F앱이 *.약관완료 파일을 생성해야 함)
                    bool 전체완료 = await 모든약관완료감시(상태.groupKey, 앱수);

                    string msg = 전체완료 && 결과.All(r => r)
                        ? $"{DateTime.Now:yy.MM.dd HH:mm:ss}[제어앱] 모든 F앱 변환 완료 → PDF 병합 시작"
                        : $"{DateTime.Now:yy.MM.dd HH:mm:ss}[제어앱] 일부 F앱 변환 미완료 또는 오류 발생";

                    this.BeginInvoke(new Action(() => 로그기록(msg, 1)));
                    실행여부 = 전체완료 && 결과.All(r => r);
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new Action(() => 로그기록($"{DateTime.Now:yy.MM.dd HH:mm:ss}[제어앱] 감시관리 오류: {ex.Message}", 1)));
                }
                finally
                {
                    cts.Cancel();
                    Interlocked.Exchange(ref 실행상태, 0);  // 실행 완료 → 0으로 복귀
                }

            }
            return 실행여부;
        }
        // ✅ 모든 약관완료 감시
        private async Task<bool> 모든약관완료감시(string tmpgroupKey, int 앱수)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start) < TimeSpan.FromMinutes(5))
            {
                int 완료파일수 = 0;
                for (int i = 1; i <= 앱수; i++)
                {
                    string H2P완료파일 = $"{저장.기본출력폴더}감시\\{tmpgroupKey}.F{i}_종료";
                    if (new FileInfo (H2P완료파일).Exists)
                        완료파일수++;
                }

                if (완료파일수 >= 앱수)
                    return true;

                await Task.Delay(2000);
            }
            return false;
        }

        private async Task<bool> F앱감시루프(현재상태 상태, int index, CancellationToken token)
        {
            string 실행경로 = $@"c:\Easy약관Plus\program\H2P\F{index}\";
            string exePath = $"{실행경로}f1.exe";
            string 프로세스이름 = System.IO.Path.GetFileNameWithoutExtension(exePath);
            string 완료경로 = $"{상태.출력폴더}{index}\\@완료\\";

            string H2P완료파일 = $"{저장.기본출력폴더}감시\\{상태.groupKey}.F{index}_종료";
            string 작업경로 = $"{상태.출력폴더}{index}\\";

            string 약관완료파일 = $"{상태.출력폴더}{index}\\{index}.약관완료";
            int[] 앱실행 = new int[10];

            if (!Directory.Exists(완료경로))
                Directory.CreateDirectory(완료경로);

            this.Invoke(new Action(() =>
            {
                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[F{index}] 변환앱 감시 시작";
                로그기록(로그내용, 1);
            }));

            bool 정상완료 = false;
            int 실행횟수 = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // 1️⃣ 프로세스 존재 여부 확인
                    Process proc = Process.GetProcesses()
                        .FirstOrDefault(p =>
                        {
                            try { return string.Equals(p.MainModule.FileName, exePath, StringComparison.OrdinalIgnoreCase); }
                            catch { return false; }
                        });

                    
                    if (proc != null && !proc.HasExited)
                    {
                        this.Invoke(new Action(() => {
                            로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[F{index}] 기존 앱 감지됨 - 재사용";
                            로그기록(로그내용, 1);
                        }));

                    } else {

                        // 2️⃣ 없으면 즉시 재실행

                        F1프로그램종료(index);
                        await Task.Delay(500);

                        var startInfo = new ProcessStartInfo
                        {
                            FileName = exePath,
                            WorkingDirectory = 실행경로,
                            Arguments = 파라메타,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        proc = Process.Start(startInfo);
                        this.Invoke(new Action(() => {
                            로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[F{index}] 변환앱 실행 {앱실행[index]++}";
                            로그기록(로그내용, 1);
                        }));

                        // 로딩 대기 (메인 윈도우 핸들이 만들어질 때까지)
                        // proc가 null이 아닌지 확인 후 WaitForInputIdle 시도
                        if (proc != null)
                        {
                            try { proc.WaitForInputIdle(10000); } catch { }
                        }
                        else
                        {
                            this.Invoke(new Action(() => {
                                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[F{index}] 프로세스 시작 실패 (proc null)";
                                로그기록(로그내용, 1);
                            }));
                            await Task.Delay(2000, token);
                            continue;
                        }
                    }

                    // 3️⃣ 플래그 감시
                    DateTime lastCheck = DateTime.Now;
                    int lastFileCount = Directory.GetFiles(완료경로, "*.hwp").Length;

                    while (!proc.HasExited && !token.IsCancellationRequested)
                    {
                        await Task.Delay(5000, token);

                        // 변환 완료 플래그 감지
                        int hwpCount = Directory.GetFiles(작업경로, "*.hwp").Length;
                        bool H2P종료 = new FileInfo(H2P완료파일).Exists;
                        정상완료 = (H2P종료 && hwpCount == 0);

                        if (정상완료)
                        {
                            this.Invoke(new Action(() => 
                            {
                                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[F{index}] 변환앱 완료 플래그 감지(정상종료)";
                                로그기록(로그내용, 1); 
                            }));
                            return 정상완료;
                            // 한 작업 사이클 완료 후 다음 루프로
                        }

                        // HWP 변화 감시 (2분 이상 변화 없으면 재시작)
                        int currentCount = Directory.GetFiles(완료경로, "*.hwp").Length;
                        if (currentCount > lastFileCount)
                        {
                            lastFileCount = currentCount;
                            lastCheck = DateTime.Now;
                        }
                        else if ((DateTime.Now - lastCheck) > TimeSpan.FromMinutes(2) && new FileInfo (약관완료파일).Exists)
                        {
                            try
                            {
                                proc.Kill();
                                F1프로그램종료(index);
                                실행횟수++;
                                this.Invoke(new Action(() =>
                                {
                                    로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[F{index}] 변환앱 오류 발생 → 프로세스 재시작 : {실행횟수: 0000}";
                                    로그기록(로그내용, 1);
                                }));
                            }
                            catch { }
                            break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[F{index}] 감시 중 오류: {ex.Message}";
                        로그기록(로그내용, 1);
                    }));
                   
                }

                await Task.Delay(3000); // 재시작 대기 후 루프 반복

            }
            return 정상완료;
        }

        private bool FindOrKillProcessByPath(string processName, string exePath, string mode)
        {
            bool 찾기 = false;
            string procNameOnly = System.IO.Path.GetFileNameWithoutExtension(processName);

            try
            {
                Process[] processes = Process.GetProcessesByName(procNameOnly);

                foreach (Process p in processes)
                {
                    // ★ 중요: MainModule을 호출하지 않고 바로 API를 씁니다. (오류 원천 차단)
                    string currentPath = GetProcessPathByApi(p.Id);

                    if (!string.IsNullOrEmpty(currentPath) &&
                        string.Equals(currentPath, exePath, StringComparison.OrdinalIgnoreCase))
                    {
                        찾기 = true;

                        if (mode == "kill")
                        {
                            try
                            {
                                p.Kill();
                                p.WaitForExit(1000);
                                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[앱종료] {processName} (PID: {p.Id})";
                                로그기록(로그내용, 1);
                            }
                            catch (Exception ex)
                            {
                                // 권한 문제로 종료 못 할 수도 있음
                                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[종료실패] {ex.Message}";
                                로그기록(로그내용, 1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[프로세스조회오류] {ex.Message}";
                로그기록(로그내용, 1);
            }

            return 찾기;
        }

        private string GetProcessPathByApi(int pid)
        {
            IntPtr hProcess = IntPtr.Zero;
            try
            {
                // 제한된 권한으로 열기 때문에 32비트 앱에서도 64비트 앱 정보를 볼 수 있음
                hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, pid);

                // 접근 권한이 아예 없는 시스템 프로세스(백신, 윈도우 커널 등)는 0을 반환함
                if (hProcess == IntPtr.Zero) return "";

                int capacity = 1024;
                StringBuilder sb = new StringBuilder(capacity);
                int size = capacity;

                if (QueryFullProcessImageName(hProcess, 0, sb, ref size))
                {
                    return sb.ToString();
                }
            }
            catch
            {
                // 혹시 모를 API 오류 무시
            }
            finally
            {
                if (hProcess != IntPtr.Zero) CloseHandle(hProcess);
            }
            return "";
        }

        // ==========================================================================================================
        // Easy약관Plus : 앱이 응답없음이면 재시작
        // ==========================================================================================================

        private async Task<bool> Easy약관Plus실행()
        {
            bool 실행여부 = true;
            const int 최대재시도 = 3;
            string 실행파일명 = "Easy약관Plus.exe";
            string 실행경로 = @"c:\Easy약관Plus\program\";
            //string 실행경로 = @"d:\Easy약관Plus\Easy약관PDF\H2PControl\bin\Debug\";

            string 실행파일전체경로 = System.IO.Path.Combine(실행경로, 실행파일명);
            string 프로세스이름 = System.IO.Path.GetFileNameWithoutExtension(실행파일명);

            try
            {

                // 🔹 앱실행여부  확인 (정확히 같은 경로의 프로세스만)
                bool 앱실행여부 = FindOrKillProcessByPath(프로세스이름, 실행파일전체경로, "check");
                if (앱실행여부)
                {
                    로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[Easy약관Plus APP종료] Easy약관Plus APP 종료 종료합니다.";
                    로그기록(로그내용, 1);
                    FindOrKillProcessByPath(프로세스이름, 실행파일전체경로, "kill");

                    // 약간의 대기 후 재실행 (완전 종료 보장)
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }

                // 🔸 실행되지 않았으면 새로 실행
                for (int 시도 = 1; 시도 <= 최대재시도; 시도++)
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = 실행파일전체경로,
                            WorkingDirectory = 실행경로,
                            //Arguments = $"\"{상태.groupKey}\"", // 따옴표로 감싸기
                            Arguments = "재시작",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        Process Easy약관Plus제어 = new Process { StartInfo = startInfo };
                        Easy약관Plus제어.Start();

                        로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[Easy약관Plus APP실행] Easy약관Plus APP 실행을 시작 합니다.";
                        로그기록(로그내용, 1);
                        break;
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        실행여부 = false;

                        if (시도 == 최대재시도)
                        {
                            로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[ERROR] Easy약관Plus APP실행 오류 : {시도}번 모두 실행 실패 - {ex.Message}";
                            로그기록(로그내용, 1);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                실행여부 = false;
                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[ERROR] ◎진행불가 : Easy약관Plus APP실행 오류 - {ex.Message}";
                로그기록(로그내용, 1);
            }
            return 실행여부;
        }

        // ==========================================================================================================
        // 04 - 01 선택약관PDF병합 : 세션별로 되어 있는 PDF파일을 하나로 병합 하는 단계
        // ==========================================================================================================
        private async Task<bool> 선택약관PDF병합(현재상태 상태)
        {
            int 재시도 = 0;
            bool 실행여부 = false;
            while (true)
            {
                try
                {
                    string StateFile상태 = "";
                    int PDFcnt = new DirectoryInfo(상태.출력폴더).GetFiles("*.pdf").Count();
                    if (PDFcnt == 0)
                    {
                        StateFile상태 = string.Format(@"{0}감시\\", 저장.기본출력폴더);
                        int PDF변환완료Fcnt = new DirectoryInfo(StateFile상태).GetFiles($"{상태.groupKey}.*_종료").Count();

                        if (PDF변환완료Fcnt == 상태.앱실행수)
                        {
                            return true;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }
                    List<세션파일> 분할선택약관 = new List<세션파일>();
                    foreach (FileInfo 파일 in new DirectoryInfo(상태.출력폴더).GetFiles("*.pdf"))
                    {
                        List<string> 분리 = 파일.Name.Split('_').Select(P => P.Trim()).ToList();
                        string[] tmp분리 = 파일.Name.Split('_').Select(P => P.Trim()).ToArray();

                        // ex)) PK(고객번호-MG같은경우는 여러개의 값으로 결합되어 있음)_세션(01,IX:목차)_가변(01:고정, 02:가변).pdf 
                        // 고객번호(파일명)기준으로 그룹화한다.
                        // 라이나생명(CH) - 독립특별약관은 네이밍룰이 다름 : 고객코드_세션코드(06)_장(01~99)_출력순서(01,02,03).pdf
                        
                        int pk시작위치 = (상태.고객사 == "CH" && tmp분리[1] == "06") ? 3 : 2; //기본은 2
                        string tmpPK = string.Join("_", tmp분리, 0, tmp분리.Length - pk시작위치);
                        
                        분할선택약관.Add(new 세션파일()
                        {
                            PK = tmpPK,
                            경로 = 파일.FullName
                        });
                    }

                    List<string> 중복고객 = new List<string>();
                    var semaphore = new SemaphoreSlim(8);
                    var tasks = new List<Task>();

                    var 완료고객번호 = new ConcurrentBag<string>();
                    var 오류고객번호 = new ConcurrentBag<string>();


                    using (var reader = await LoadReaderAsync(상태).ConfigureAwait(false))
                    {
                        while (await reader.ReadAsync().ConfigureAwait(false)) 
                        {
                            string 고객Key = reader["고객Key"]?.ToString() ?? "";
                            if (string.IsNullOrWhiteSpace(고객Key))
                                continue;
                            string[] 분리 = reader["data"].ToString().Split('|').Select(P => P.Trim()).ToArray();
                            // 상품별 샘플출력
                            // 샘플 테스트로 모든 담보내용을 출력한다. - 처음 담보분리를 하고 올바르게 저장 되어 있는지 테스트 한다.
                            // 임의로 상품명에 *로 표시하면 전체 담보내용 출력
                            // 상품테스트 인 경우 데이타 예) 123456789|6AAFG|2108|*|M
                            // ===================================================================
                            //   수정시 frmMerge 04 - 02 : 출력에 필요한 DB내용을 레코드 구조체에 저장한다
                            // ===================================================================
                            if (분리[3] == "*")
                            {
                                string tmp종분류코드 = (string.IsNullOrEmpty(분리[4])) ? "" : 분리[4];
                                Array.Resize(ref 분리, 18);

                                for (int idx = 4; idx <= 17; idx++)
                                {
                                    분리[idx] = "";
                                }
                                분리[9] = tmp종분류코드; // 종분류코드
                                분리[10] = "02";         // 서식구분(01:증권+약관,  02:약관만, 03:증권만)
                                분리[11] = "02";         // 발송구분(01:우편,       02:모바일, 03:이메일)
                            }
                            // 증권만인 경우
                            if (분리[10] == "03" || 분리[10] == "3")
                            {
                                continue;
                            }

                            var 출력약관 = from 파일 in 분할선택약관
                                       where 파일.PK == reader["고객Key"].ToString()
                                       orderby 파일.경로 ascending
                                       select new { PK = 파일.PK, 경로 = 파일.경로 };

                            List<string> 만들파일 = new List<string>();
                            string 목차경로 = "";
                            foreach (var 약관 in 출력약관)
                            {
                                if (약관.경로.Contains("_IX_")) {
                                    목차경로 = 약관.경로;
                                }
                                else {
                                    만들파일.Add(약관.경로);
                                }
                            }

                            // 목차파일을 H파일 다음으로 설정한다.
                            if (!string.IsNullOrWhiteSpace(목차경로))
                            {
                                만들파일.Insert(1, 목차경로);
                            }

                            if (중복고객.IndexOf(reader["고객Key"].ToString()) != -1)
                            {
                                로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[ER중복고객: {1}] ", DateTime.Now, reader["고객Key"].ToString());
                                로그기록(로그내용, 1);
                                continue;
                            }
                            else {
                                중복고객.Add(reader["고객Key"].ToString());
                            }

                            if (출력약관.Count() == 0)
                            {
                                로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[약관병합오류] 고객번호 : {1} 약관이 존재 하지 않습니다.", DateTime.Now, reader["고객Key"].ToString());
                                로그기록(로그내용, 1);
                                continue;
                            }

                            string 처리일자 = string.Format("{0:MMdd}", DateTime.Now);
                            // 파일명 : 출력경로 + 처리일자 + 상품코드 + 개정일 + 증권번호
                            string 출력파일 = string.Format("{0}Y\\Y{1}.pdf", 상태.출력폴더, reader["파일명"].ToString());

                            tasks.Add(Task.Run(async () =>
                            {
                                await semaphore.WaitAsync().ConfigureAwait(false);
                                try
                                {
                                    bool ok = await 개별PDF세션결합(출력파일, 만들파일, 분리, 상태).ConfigureAwait(false);

                                    if (ok)
                                    {
                                        완료고객번호.Add(고객Key);
                                        this.BeginInvoke(new Action(() =>
                                        {
                                            로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[선택약관PDF병합 성공] 고객번호 : {1}", DateTime.Now, 고객Key);
                                            로그기록(로그내용, 1);
                                        }));
                                    }
                                    else
                                    {
                                        오류고객번호.Add(고객Key);
                                        this.BeginInvoke(new Action(() =>
                                        {
                                            로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[선택약관PDF병합 실패] 고객번호 : {1}", DateTime.Now, 고객Key);
                                            로그기록(로그내용, 1);
                                        }));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    오류고객번호.Add(고객Key);
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[선택약관PDF병합 실패] 고객번호 : {1}, {2}", DateTime.Now, 고객Key, ex.Message);
                                        로그기록(로그내용, 1);
                                    }));
                                    
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }));


                        }
                    }

                    // 모든 고객 병합 작업 완료 대기
                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    // ===== 여기서 '반드시' DB 업데이트를 수행합니다 (UI 스레드 필요 없음) =====
                    await BatchUpdateAsync(상태, 완료고객번호, 오류고객번호).ConfigureAwait(false);

                    실행여부 = true;
                    break;
                }
                catch (Exception ex)
                {
                    재시도++;
                    if (재시도 <= 3)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        continue;
                    }
                    실행여부 = false;
                    로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[선택약관PDF병합] 오류 : {1} - {2}", DateTime.Now, ex.Message, ex.StackTrace);
                    로그기록(로그내용, 1);


                    int listIndex = -1;
                    if (실행주체 == "클라이언트")
                    {
                        listIndex = 9;
                        _clientHandler?.Send(new ChatHub
                        {
                            RoomId = C_RoomId,
                            UserName = C_UserName,
                            Message = $"[작업오류] {ex.Message}",
                            상황 = "오류",
                            State = ChatState.진행상황
                        });
                    }
                    else
                    {
                        listIndex = 0;
                    }
                    SetError(true, listIndex, "ERROR");
                    break;
                }

            }

            return 실행여부;
        }

        // ==========================================================================================================
        // 04 - 02 data, data처리 테이블 Update : PDF병합완료/PDF병합오류
        //         data처리는 기본이 '대기'이며 오류가 발생하면 Update한다.
        //         100개씩 배치 업데이트
        // ==========================================================================================================
        private async Task BatchUpdateAsync(현재상태 상태, ConcurrentBag<string> 완료, ConcurrentBag<string> 오류)
        {
            const int 묶음 = 100;
            상태.처리Step = "05";
            // 완료
            var 완료목록 = 완료.Distinct().ToList();
            string SqlStr = "";
            for (int i = 0; i < 완료목록.Count; i += 묶음)
            {
                var part = 완료목록.Skip(i).Take(묶음).ToList();
                string 키목록 = $"'{string.Join("','", part)}'";
                SqlStr = $@"
                    UPDATE data 
                    SET PDF병합작업 = 'PDF병합완료'  
                    WHERE 처리구분 = '{상태.처리구분}' AND 
                          groupKey = '{상태.groupKey}' AND
                          고객Key IN ({키목록})
                    ";
                await DBUpdate(SqlStr, 상태.dataTable접속).ConfigureAwait(false);
            }

            // 오류
            var 오류목록 = 오류.Distinct().ToList();
            for (int i = 0; i < 오류목록.Count; i += 묶음)
            {
                var part = 오류목록.Skip(i).Take(묶음).ToList();
                string 키목록 = $"'{string.Join("','", part)}'";
                SqlStr = $@"
                    UPDATE data 
                    SET PDF병합작업 = 'PDF병합오류' 
                    WHERE 처리구분 = '{상태.처리구분}' AND
                          groupKey = '{상태.groupKey}' AND 
                          고객Key IN ({키목록})
                ";
                await DBUpdate(SqlStr, 상태.dataTable접속).ConfigureAwait(false);
            }
            // 주의요망(PDF병합작업)
            // F1 ~ F8은 모두 완료되서 Update 해야한다. 따라서 여기서는 업데이트를 하면 않된다.
            // 약관작업, PDF변환작업, PDF병합작업 병렬작업은 모든작업이 끝난 후에 일괄 업데이트 한다.
        }

        // ==========================================================================================================
        // 04 - 03 data 테이블 검색 : 처리구분(00~03,병렬처리), groupKey, PDF변환완료
        // ==========================================================================================================
        private async Task<SqlDataReader> LoadReaderAsync(현재상태 상태)
        {
            var AdoNet = new SqlConnection(상태.dataTable접속);
            await AdoNet.OpenAsync().ConfigureAwait(false);

            var cmd = new SqlCommand(@"
                SELECT *
                FROM data
                WHERE groupKey = @groupKey AND
                      처리구분 = @처리구분 AND
                      PDF변환작업 = 'PDF변환완료' AND
                      PDF병합작업 <> 'PDF병합완료'
                ", AdoNet);
            cmd.Parameters.AddWithValue("@groupKey", 상태.groupKey);
            cmd.Parameters.AddWithValue("@처리구분", 상태.처리구분);
            // CommandBehavior.CloseConnection: 리더 닫으면 커넥션도 닫힘
            return await cmd.ExecuteReaderAsync(System.Data.CommandBehavior.CloseConnection).ConfigureAwait(false);
            // 쿼리가 PDF변환작업 <> 'PDF변환오류' 이유는 특별약관이 없은 경우(실손 H만 출력) PDF변환작업 값이 '대기'
            // 대기도 포함
        }

        // ==========================================================================================================
        // 04 - 04 선택약관PDF병합 : 세션별로 (H,B,T,I)를 하나로 병합하는 단계
        // ==========================================================================================================
        private async Task<bool> 개별PDF세션결합(string 출력파일, List<string> tmp만들파일, string[] 분리, 현재상태 상태)
        {
            int 재처리 = 0;
            bool 실행여부 = false;
            while (true)
            {
                if (재처리 >= 3)
                {
                    break;
                }
                try
                {
                    using (FileStream 약관fsRW = new FileStream(출력파일, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    using (PdfDocument 약관DOC = new PdfDocument(new PdfWriter(약관fsRW))) 
                    {
                        PdfMerger 약관Merger = new PdfMerger(약관DOC);
                        약관Merger.SetCloseSourceDocuments(false);

                        int 순서 = 0;
                        string 이전세션 = "";
                        int 짝홀페이지 = 0;
                        string 현재파일명 = "";
                        string 발송구분 = (분리[11] == "01" || 분리[11] == "1") ? "P" : (분리[11] == "02" || 분리[11] == "2") ? "M" : "E";  // P : 우편, M : 모바일, E : 이메일
                        iText.Kernel.Geom.Rectangle pagesize = null;
                        bool 처음만페이지 = false;
                        bool 목차여부 = false;
                       
                        foreach (var 만들파일 in tmp만들파일)
                        {
                            순서++;
                            string[] 파일명분리 = System.IO.Path.GetFileNameWithoutExtension(만들파일).Split('_');
                            string 세션 = 파일명분리[파일명분리.Length - 2];

                            현재파일명 = new FileInfo(만들파일).Name; // 에러확인용

                            // 세션별로 짝수페이지를 설정한다.
                            // 이전세션과 현재세션이 상이 하면서 이전세션의 페이지가 홀수이면 짝수로 세팅한다.
                            // 우편일때만 해당(이메일, 모바일제외)
                            // 현재세션이 목차이면 제외한다. ( 목차는 추가 하는 개념이 아니고 H에 삽입한다.)

                            if (순서 != 1 && 세션 != 이전세션 && 발송구분 == "P" && 세션 != "IX")
                            {
                                짝홀페이지 = 약관DOC.GetNumberOfPages();
                                // 목차가 있는 고객사
                                if (짝홀페이지 % 2 == 1)
                                {
                                    약관DOC.AddNewPage((PageSize)pagesize);  //  *** 세션이 다르면 짝수페이지 설정 ***
                                }
                            }

                            using (FileStream 개별fsRW = new FileStream(만들파일, FileMode.Open, FileAccess.Read, FileShare.Read))
                            using (PdfDocument 개별DOC = new PdfDocument(new PdfReader(개별fsRW)))
                            {
                                if (처음만페이지 == false)
                                {
                                    pagesize = 개별DOC.GetPage(1).GetPageSizeWithRotation();
                                    처음만페이지 = true;
                                }

                                int 페이지 = 개별DOC.GetNumberOfPages();
                                //개별DOC.CopyPagesTo(1, 페이지, 약관DOC);   // CopyPage는 목차가 사라진다.

                                // 목차를 H에 삽입 하는 과정
                                // 목차는 Merge가  아니고 Insert을 해야 한다.

                                if (세션 == "IX")
                                {
                                    목차여부 = true;
                                    int 삽입페이지 = Convert.ToInt32(파일명분리[파일명분리.Length - 1]);

                                    // 일단 세션을 맞추기 위해 짝수 페이지를 맞쳐준다.
                                    // 목차가 있는 고객사.
                                    if (발송구분 == "P" && 삽입페이지 % 2 == 0)
                                    {
                                        약관DOC.AddNewPage(삽입페이지, (PageSize)pagesize);
                                        삽입페이지 += 1;
                                    }

                                    // Insert 개념 ↓↓↓↓
                                    // 증권Doc.CopyPagesTo(증권pagecnt, 증권pagecnt, 개별증권doc, 개별증권doc.GetNumberOfPages(), new PdfPageFormCopier());

                                    개별DOC.CopyPagesTo(1, 페이지, 약관DOC, 삽입페이지, new PdfPageFormCopier());

                                    // 보통약관이 시작하는 부분으로 홀수페이지 부터 시작 할 수 있도록 목차까지 삽입한 H부분을 짝수로 만들어 준다.
                                    if (발송구분 == "P")
                                    {
                                        짝홀페이지 = 개별DOC.GetNumberOfPages();
                                        int 현재까지페이지 = 삽입페이지 + 짝홀페이지 - 1;

                                        if (짝홀페이지 % 2 == 1)
                                        {
                                            // 빈페이지 삽입 (마지막 페이지)
                                            약관DOC.AddNewPage(현재까지페이지 + 1, (PageSize)pagesize);
                                        }
                                    }
                                }
                                else
                                {
                                    약관Merger.Merge(개별DOC, 1, 페이지);
                                    //약관Merger.Merge(개별DOC, iText.IO.Util.JavaUtil.ArraysAsList(1, 5, 7, 1));
                                }
                            }

                            이전세션 = 세션;
                        }

                        if (상태.고객사 == "KB")
                        {
                            PdfFont 나눔바른고딕 = PdfFontFactory.CreateFont(@"c:\Easy약관Plus\program\Font\NanumBarunGothic.ttf", PdfEncodings.IDENTITY_H);
                            Document docw = new Document(약관DOC);
                            int 전체페이지수 = 약관DOC.GetNumberOfPages();

                            for (int i = 1; i <= 전체페이지수; i++)
                            {
                                // 현재 페이지의 크기를 가져와 가로 중앙 위치(x) 설정
                                float x = 약관DOC.GetPage(i).GetPageSize().GetWidth() / 2;
                                float y = MMtoPoint(3.0); // 하단 여백 높이 

                                // "- 1 -" 형식의 쪽번호 텍스트 생성
                                string 쪽번호 = string.Format("- {0} -", i);

                                // 지정한 위치(x, y)에 쪽번호 삽입
                                docw.ShowTextAligned(
                                    new Paragraph(쪽번호).SetFont(나눔바른고딕).SetFontSize(8).SetFontColor(iText.Kernel.Colors.ColorConstants.BLACK),
                                    x, y, i, TextAlignment.CENTER, VerticalAlignment.BOTTOM, 0);
                            }
                        }
                        실행여부 = true;
                        약관Merger.Close();

                        //개별선택약관파일(H,B,IX,T)를 병합하고 '@완료' 폴더로 이동한다.
                        string 완료폴더 = string.Format("{0}@완료\\", 상태.출력폴더);
                        foreach (var 만들파일 in tmp만들파일)
                        {
                            try
                            {
                                new FileInfo(만들파일).MoveTo(string.Format("{0}{1}", 완료폴더, new FileInfo(만들파일).Name));
                            }
                            catch (Exception)
                            {}
                        }
                        break;
                    }
                    
                }
                catch (Exception ee)
                {
                    로그내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[개별PDF세션결합 Er01] {1}==>{2}", DateTime.Now, ee.Message, ee.StackTrace);
                    로그기록(로그내용, 1);
                    재처리++;
                    await Task.Delay(3000);
                    continue;

                }
            }

            return 실행여부;
        }
        private float MMtoPoint(double x)
        {
            float tmpx;
            tmpx = (float)(x * 2.83465);
            return tmpx;
        }

        //========================================================================================================================================
        // 공통
        //========================================================================================================================================

        // =============================================================================
        //  로그파일생성(공통)
        // =============================================================================
        private void 로그파일열기()
        {
            string 로그폴더생성 = 폴더생성(저장.로그경로, $"{DateTime.Now:yyyy.MM}");
            로그폴더생성 = 폴더생성(로그폴더생성, $"{DateTime.Now:dd}");

            DateTime now = DateTime.Now; // 현재 시간

            // 1. 기존에 열린 파일이 있다면 닫아줍니다 (메모리 해제 및 저장)
            if (Logwr != null) { Logwr.Flush(); Logwr.Close(); Logwr = null; }
            if (Logfs != null) { Logfs.Close(); Logfs = null; }

            string 로그파일 = string.Format("{0}({1:yyMMdd})H2PControl.log", 로그폴더생성, DateTime.Now);
            try
            {
                Logfs = new FileStream(로그파일, FileMode.Append, FileAccess.Write, FileShare.Write);
                Logwr = new StreamWriter(Logfs, Encoding.GetEncoding(949));

                // 2. 파일이 성공적으로 열렸을 때의 날짜(Day)를 저장합니다.
                _currentLogDay = now.Day;
            }
            catch (Exception) { }
        }

        // =============================================================================
        //  로그출력(공통)
        // =============================================================================
               
        private void 로그기록(string 내용, int 출력형태, int ID = 0)
        {
            //리스트[0] = lsth2p; 기본/서버
            //리스트[1] = lbxMsg; 클라이언트
            
            if (실행주체 == "클라이언트")
                ID = 9;

            try
            {
                if (출력형태 == 1)
                    Logwr.WriteLine(내용); Logwr.Flush();
            }
            catch (Exception) { }

            string 편집 = 내용;
            if (내용.Length > 17)
                편집 = 내용.Substring(17);

            if (리스트[ID].Items.Count > 300) 
                리스트[ID].Items.RemoveAt(300);

            리스트[ID].Items.Insert(0, 편집);
            리스트[ID].SelectedIndex = 0;
            
        }

        // =============================================================================
        //  데이터 UPDATE/DELETE 상태(공통)
        // =============================================================================
        private async Task<int> DBUpdate(string SqlStr, string dataTable접속)
        {
            int 완료갯수 = -1;
            try
            {
                using (SqlConnection AdoNet = new SqlConnection(dataTable접속))
                {
                    await AdoNet.OpenAsync();
                    using (SqlCommand sqlcmd = new SqlCommand(SqlStr, AdoNet))
                    {
                        sqlcmd.CommandTimeout = 0;
                        완료갯수 = await sqlcmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception)
            {
            }

            return 완료갯수;
        }

        // =============================================================================
        //  앱종료(공통)
        // =============================================================================
        private void H2PControl_FormClosed(object sender, FormClosedEventArgs e)
        {
            for (int i = 1; i <= 8; i++)
            {
                F1프로그램종료(i);
            }
                
        }
        private void bnt종료_Click(object sender, EventArgs e)
        {
            for (int i = 1; i <= 8; i++)
            {
                F1프로그램종료(i);
            }

        }

        
        private void F1프로그램종료(int i)
        {
            // 1단계: PID 파일을 읽어서 '내 한컴'만 정확히 암살 (검색 X)
            string pidFilePath = string.Format(@"{0}PID\F{1}.PID", 저장.기본출력폴더, i);
            if (File.Exists(pidFilePath))
            {
                try
                {
                    // 1. 파일에서 PID 꺼내기
                    string pidStr = File.ReadAllText(pidFilePath);
                    if (int.TryParse(pidStr, out int targetPid))
                    {
                        try
                        {
                            Process p = Process.GetProcessById(targetPid);
                            if (!p.HasExited)
                            {
                                p.Kill();
                                p.WaitForExit(100);
                            }
                        }
                        catch (ArgumentException) { // 이미 죽어서 없으면 OK
                        }
                    }
                }
                catch { }
                finally
                {
                    // 3. 증거 인멸 (PID 파일 삭제)
                    try { File.Delete(pidFilePath); } catch { }
                }
            }

            // 2단계: F(i)번 앱 (f1.exe) 종료
            Process[] fAppProcesses = Process.GetProcessesByName("f1"); // 실행파일 이름 f1.exe
            string fAppFolder = string.Format(@"\F{0}\", i);            // 경로 구분용 (\F1\)

            foreach (Process p in fAppProcesses)
            {
                try
                {
                    if (p.HasExited) continue;
                    bool isTarget = false;

                    // [경로 확인] 가장 확실한 방법
                    try
                    {
                        if (p.MainModule.FileName.IndexOf(fAppFolder, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            isTarget = true;
                        }
                    }
                    catch { /* 권한 문제 등 패스 */ }

                    // 타겟이면 종료
                    if (isTarget)
                    {
                        p.Kill();
                        p.WaitForExit(100);
                    }
                }
                catch { }
                finally
                {
                    try { p.Dispose(); } catch { }
                }
            }
        }

        // =============================================================================
        //  폴더생성(공통)
        // =============================================================================
        private string 폴더생성(string 현재폴더, string 하위폴더)
        {
            string 하위폴더확인 = string.Format("{0}{1}\\", 현재폴더, 하위폴더);
            if (new System.IO.DirectoryInfo(하위폴더확인).Exists == false)
                new System.IO.DirectoryInfo(하위폴더확인).Create();
            return 하위폴더확인;
        }

        // =============================================================================
        //  복사, 이동, 삭제(공통)
        // =============================================================================
        private bool 복사_이동_삭제(string 명령, string 원본경로, string 대상경로)
        {
            
            bool 실행여부 = true;
            try
            {
                switch (명령)
                {
                    case "복사_삭제":
                        new FileInfo(원본경로).CopyTo(대상경로, true);
                        new FileInfo(원본경로).Delete();
                        break;
                    case "복사":
                        new FileInfo(원본경로).CopyTo(대상경로, true);
                        break;
                }
            }
            catch (Exception)
            {
                실행여부 = false;
            }
            return 실행여부;
        }

        // =======================================================================================================
        

        // 오류가 났을 때 호출할 함수
        public void SetError(bool isError, int index, string 오류종류)
        {
            if (index < 0 || index >= 리스트.Length) return;

            errorStates[index] = isError;
            var targetList = 리스트[index];

            if (isError)
            {
                if (오류종류 == "RECORDERR")
                {
                    targetList.Appearance.BackColor = Color.CornflowerBlue;
                    targetList.Appearance.Options.UseBackColor = true;
                    ViewerrorIcon = errorIcon[1];
                }
                else {
                    // 오류 발생 시
                    targetList.Appearance.BackColor = Color.IndianRed;
                    targetList.Appearance.Options.UseBackColor = true;
                    ViewerrorIcon = errorIcon[0];
                }
                
            }
            else
            {
                targetList.Appearance.Options.UseBackColor = false;
            }
            targetList.Invalidate();
        }
        private void lsth2p0_5_Paint(object sender, PaintEventArgs e)
        {
            // 1. 지금 이벤트를 보낸 리스트박스가 누구인지 확인
            ListBoxControl currentList = sender as ListBoxControl;
            if (currentList == null) return;

            // 2. 이 리스트박스가 배열의 몇 번째 놈인지 찾기
            int index = Array.IndexOf(리스트, currentList);

            // 찾지 못했거나( -1 ), 해당 순번이 오류 상태가 아니면 그리지 않고 종료
            if (index < 0 || errorStates[index] == false) return;



            // 아이콘 크기 및 위치 계산 (정중앙)
            int iconSize = 250;
            int x = (currentList.Width - iconSize) / 2;
            int y = (currentList.Height - iconSize) / 2;

            // 투명도 설정
            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix33 = 0.6f; // 투명도 60%
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // 그리기
            e.Graphics.DrawImage(ViewerrorIcon, new System.Drawing.Rectangle(x, y, iconSize, iconSize),
                0, 0, ViewerrorIcon.Width, ViewerrorIcon.Height, GraphicsUnit.Pixel, attributes);
        }

        private void 리스트박스초기화()
        {
            try
            {
                errorIcon = new System.Drawing.Image[] {
                    Properties.Resources.오류2,
                    Properties.Resources.오류1
                };
            }
            catch
            {
                errorIcon = new System.Drawing.Image[] {
                    SystemIcons.Error.ToBitmap(),
                    SystemIcons.Error.ToBitmap()
                };
            }

            리스트 = new ListBoxControl[] {
                lsth2p,
                lsth2p_1,
                lsth2p_2,
                lsth2p_3,
                lsth2p_4,
                lsth2p_5,
                null,
                null,
                null,
                lbxMsg
            };
            //서버(0: main), 클라이언트(9)  
            errorStates = new bool[리스트.Length];
            foreach (var lb in 리스트)
            {
                if (lb != null)
                    lb.Paint += lsth2p0_5_Paint;
            }
        }



        // =============================================================================
        //  Server/Client 초기화(접속)
        // =============================================================================
        public async Task<string> TCPServer_Client초기화(string tmp실행주체, string 시작종료, 환경설정 설정)
        {
            string 실행여부 = "성공";
            bool 작동스위치 = true;
            if (tmp실행주체 == "서버")
            {
                if (시작종료 == "시작")
                {
                    _roomManager = new ClientRoomManager();
                    _server = new ChatServer(IPAddress.Parse(설정.접속IP), 설정.접속PORT);  //192.168.1.20:5150, 127.0.0.1:8080(로컬)
                    _server.Connected += S_Connected;
                    _server.Disconnected += S_Disconnected;
                    _server.Received += S_Received;

                    int 접속시도 = 0;
                    int 한번출력 = 1;
                    while (true)
                    {

                        _ = _server.StartAsync();
                        if (_server.IsRunning)
                        {
                            this.Invoke(new Action(() =>
                            {
                                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[서버구동] === SERVER Start ===";
                                로그기록(로그내용, 1);
                            }));
                            실행여부 = "성공";
                            break;
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));
                            로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[서버구동시도] SERVER 구동시도 {접속시도++:00000}";

                            실행여부 = "실패";
                            로그기록(로그내용, 한번출력);
                            if (한번출력 == 1)
                                한번출력 = 0;
                        }
                    }

                } else if (시작종료 == "종료")
                {
                    _server.Stop();
                    작동스위치 = false;
                }
                
            }
            else if (tmp실행주체 == "클라이언트")
            {
                if (시작종료 == "시작")
                {
                    _client = new ChatClient(IPAddress.Parse(설정.접속IP), 설정.접속PORT);
                    _client.Connected += C_Connected;
                    _client.Disconnected += C_Disconnected;
                    _client.Received += C_Received;

                    int 접속시도 = 0;
                    int 한번출력 = 1;
                    while (true)
                    {
                        await _client.ConnectAsync(new ConnectionDetails
                        {
                            RoomId = C_RoomId,
                            UserName = C_UserName,
                        });

                        if (_client.IsRunning)
                        {                           
                            this.Invoke(new Action(() =>
                            {
                                로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[서버접속] == SERVER Connection == ";
                                로그기록(로그내용, 1);
                            }));

                            실행여부 = "성공";
                            break;
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));
                            로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[서버접속시도] 서버접속시도 {접속시도++:00000}";
                            실행여부 = "실패";
                            로그기록(로그내용, 한번출력);
                            if (한번출력 == 1)
                                한번출력 = 0;
                        }
                    }
                }
                else if (시작종료 == "종료")
                {
                    _client.Close();
                    작동스위치 = false;
                }
                
            }
            RunningStateChanged(작동스위치);
            return 실행여부;
        }


        // =============================================================================
        //  TCP/IP (Sverver)
        // =============================================================================


        // =============================================================================
        //  sub컴완료 확인 후 맞춤약관(main)에 *.전손완료
        //  1) server는 client에서 작업이 완료 될때까지 대기한다.(client는 Pdf병합완료 후 server로 Pdf를 전송하고 마지막을 sub1.END 전송한다.)
        //  2) 서버 : 대기 , 클라이언트 : 작업완료 후 PDF파일,*.END파일 전송
        //  3) 서버 : 모든 sub컴이 완료되면 main컴에 다음단계로 진행하기 위해 *.전송완료를 전달한다.
        //  4) 메인컴 : *.전송완료 수신 후 다음단계로 작업을 진행
        //
        //  클라이언트 : PDF병합이 완료되면 서버로 PDF파일, END파일을 전송한다.
        // =============================================================================
        private async Task<string> 서버수신대기_클라이언트파일송신(현재상태 상태, string 실행주체)
        {
            string 실행여부 = "성공";


            if (실행주체 == "서버")
            {
                while (true)
                {
                    try
                    {
                        int ENDFcnt = new DirectoryInfo($"{상태.출력폴더}StateFile\\").GetFiles("*.END").Count();
                        if (ENDFcnt == 상태.병렬컴 - 1)
                        {
                            //{상태.groupKey}.전송완료 파일을 기준으로 모든파일이 전송완료를 의미한다.(메인프로그램 참조)
                            string EndofFile = $"{상태.출력폴더}StateFile\\{상태.groupKey}.전송완료";
                            string 출력내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[전송완료] : 서브컴 {1:00000} ", DateTime.Now, ENDFcnt);
                            File.WriteAllText(EndofFile, 출력내용, Encoding.GetEncoding(949));

                            lbJobs.Items.Clear();
                            lbJobs_Display();

                            await _server.서버To클라이언트송신("모두", new ChatHub
                            {
                                SendType = SendType.ServerToAll,
                                State = ChatState.Message,
                                플레그 = "S To C : 메세지전송",
                                Message = "*맞춤약관작업완료*",
                                UserName = 상태.상태컴퓨터,
                                RoomId = 0
                            });
                            // Message = "*맞춤약관작업완료*" 이값을 클라이언트에서 받으면 작업을 완료(모든 데이터삭제) 다음 레코드 처리 대기 상태로 간다.

                            int 대기중cnt = 0;
                            while (true)
                            {
                                // main컴작업완료
                                string 작업완료파일 = $"{저장.기본출력폴더}감시\\{상태.groupKey}.main작업완료";
                                if (new FileInfo(작업완료파일).Exists)
                                {
                                    new FileInfo(작업완료파일).Delete();
                                    로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[main컴] mainCOM ===작업완료===";
                                    로그기록(로그내용, 1);

                                    break;
                                } else
                                {
                                    대기중cnt++;
                                    this.Invoke(new Action(() =>
                                    {
                                        로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[main컴] mainCOM 작업진행중...{대기중cnt}";
                                        로그기록(로그내용, 0);
                                    }));
                                    await Task.Delay(TimeSpan.FromSeconds(3));
                                    continue;
                                }
                            }

                            break;
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(3));
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

            } else if(실행주체 == "클라이언트")
            {
                try
                {
                    // 세션별로 분리되어 있던 파일을 PDF병합을 통해 출력\\Y에 저장한다.
                    // \\Y폴더에 있는 파일을 서버의 \\Y폴더로 전송한다.
                    string 약관전송폴더 = $"{저장.출력폴더}Y\\";
                    var files = Directory.GetFiles(약관전송폴더, "*.pdf");
                    var client = _clientHandler;

                    // ✅ 이벤트 연결 , 호출은 FileProgressChanged?.Invoke(fileName, progress) 여기서 한다.
                    client.FileProgressChanged += (fileName, progress) =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            progressBar.EditValue = (int)Math.Min(100, progress);
                            lblProgress.Text = $"{fileName} : {progress:F1}%";
                            로그내용 = $"{DateTime.Now :yy.MM.dd HH:mm:ss}[수신] {fileName} : {progress:F1}%";
                            로그기록(로그내용, 1);
                        }));
                    };

                    client.FileStatusChanged += (fileName, status) =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[시작] {fileName}";
                            로그기록(로그내용, 1);
                        }));
                    };

                    client.FileSendCompleted += (fileName, 파일cnt) =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[완료] {fileName} : count - {파일cnt}";
                            로그기록(로그내용, 1);
                        }));
                    };

                    // ✅ 파일 전송 시작

                    로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[준비]  파일전송 파일  : {files.Length}";
                    로그기록(로그내용, 1);

                    //몇개의 파일을 보낼지 C to S 메세지를 보낸다.

                    //_clientHandler?.Send(new ChatHub
                    //{
                    //    RoomId = C_RoomId,
                    //    UserName = C_UserName,
                    //    Message = 로그내용,
                    //    State = ChatState.Message
                    //});


                    string 플레그 = "C To S : 개별PDF전송";
                    await _clientHandler.파일전송(files, C_RoomId, C_UserName, 플레그);

                    로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[완료] 파일전송 완료 : {files.Length}";
                    로그기록(로그내용, 1);
                    
                }
                catch (Exception)
                {

                }

            } else
            {
                실행여부 = "실패";
                
            }
            return 실행여부;
        }


        private ChatHub CreateNewStateChatHub(ChatHub hub, ChatState state)
        {
            return new ChatHub
            {
                RoomId = hub.RoomId,
                UserName = hub.UserName,
                State = state,
            };
        }
        private void AddClientMessageList(ChatHub hub)
        {
            string message;
            switch (hub.State)
            {
                case ChatState.Connect:
                    message = $"[접  속] {hub.UserName}";
                    break;
                case ChatState.Disconnect:
                    message = $"[종  료] {hub.UserName}";
                    break;
                case ChatState.FileStart:
                case ChatState.FileChunk:
                case ChatState.FileEnd:
                case ChatState.Ack:
                    message = hub.Message;
                    break;
                case ChatState.진행상황:
                    message = hub.Message;
                    if (hub.상황 == "오류")
                    {
                        string 오류상태 = "";
                        if (message.Contains("RECORDERR"))
                            오류상태 = "RECORDERR";
                        else if (message.Contains("ERROR"))
                            오류상태 = "ERROR";
                        else if (message.Contains("FROZEN"))
                            오류상태 = "FROZEN";

                        SetError(true, hub.RoomId, 오류상태) ;
                    }
                        
                    break;
                default:
                    //ChatState.FileStart으로 제어를 해야 하나 공용으로 사용해서 중간에 값이 변경되면 처음한번만 되는것이 아니고 2번 반복된다.
                    if (hub.Message.StartsWith("[수신시작]"))
                        message = hub.Message;
                    else
                        message = $"{hub.UserName} : {hub.Message}";
                    
                    break;
            }
            // case ChatState.Ack : 서버에서 클라이언트로 파일수신완료 응답메세지

            int 표시 = 1;
            if (message.Contains("[RUNNING]") || message.Contains("[WAITING]"))
            {
                표시 = 0;
            }
            로그기록($"{DateTime.Now:yy.MM.dd HH:mm:ss}{message}", 표시, hub.RoomId);

            //클라이언트만 해당
            //클라이언트는 sub컴에게 
            if (hub.Message.EndsWith ("*맞춤약관작업완료*"))
            {
                string 작업완료 = $"{저장.출력폴더}StateFile\\모든작업종료.종료";
                string 출력내용 = string.Format("{0:yy.MM.dd HH:mm:ss}[모든작업종료] : sub컴종료", DateTime.Now);
                File.WriteAllText(작업완료, 출력내용, Encoding.GetEncoding(949));

                string 완료오류 = hub.Message.Substring(0, 2);
                if (완료오류 == "완료")
                {
                    _ = 작업완료후삭제();
                }
            }

        }
        private async Task 작업완료후삭제()
        {
            try
            {
                var di = new DirectoryInfo(저장.출력폴더);
                var 폴더파일 = di.EnumerateFileSystemInfos();

                foreach (var 삭제 in 폴더파일)
                {
                    if ((삭제.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        if (삭제.Exists)
                            Directory.Delete(삭제.FullName, true);
                    }
                    else
                    {
                        if (삭제.Exists)
                            삭제.Delete();
                    }
                }
            }
            catch (Exception)
            {
            }
            await Task.CompletedTask;
        }




        private void S_Connected(object sender, ChatEventArgs e)
        {
            var hub = CreateNewStateChatHub(e.Hub, ChatState.Connect);

            //RoomId 키값으로 딕셔너리에 TCP/IP정보 저장
            _roomManager.Add(e.ClientHandler);

            _roomManager.SendToMyRoom(hub);

            //메세지출력
            lbxClients.Items.Add($"{e.Hub.UserName}");
            AddClientMessageList(hub);
            lbxClients_Display(e.Hub.RoomId);
            if (!접속컴.ContainsKey(e.Hub.UserName))
                접속컴.Add(e.Hub.UserName, e.Hub.RoomId);
        }
        private void S_Disconnected(object sender, ChatEventArgs e)
        {
            var hub = CreateNewStateChatHub(e.Hub, ChatState.Disconnect);

            _roomManager.Remove(e.ClientHandler);
            _roomManager.SendToMyRoom(hub);

            lbxClients.Items.Remove($"{e.Hub.UserName}");
            //lbxClients_Display(e.Hub.RoomId);

            lbJobs.Items.Remove($"{e.Hub.UserName}");
            AddClientMessageList(hub);
            lbJobs_Display();
            접속컴.Remove(e.Hub.UserName);
        }

        private void S_Received(object sender, ChatEventArgs e)
        {
            _roomManager.SendToMyRoom(e.Hub);

            AddClientMessageList(e.Hub);

            if (e.Hub.State == ChatState.Ack) // 서버에서 클라이언트로 파일수신완료
            {
                string 종료확장자 = new FileInfo(e.Hub.FileName).Extension.ToLower();
                if (종료확장자 == ".end")
                {
                    lbJobs.Items.Remove($"{e.Hub.UserName}");
                    lbJobs_Display();
                }
                    
            }
        }

        private void lbxClients_Display(int ID)
        {
            if (lbxClients.Items.Count > 0)
            {
                lab접속중.ForeColor = Color.White;
                lab접속중.Font = new Font(lab접속중.Font, FontStyle.Bold);
                lab접속중.BackColor = Color.Orange;
            }
            else
            {
                lab접속중.ForeColor = System.Drawing.Color.Black;
                lab접속중.Font = new Font(lab접속중.Font, FontStyle.Regular);
                lab접속중.BackColor = Color.Transparent;
            }

            //sub1 리스트 사이즈 : 320
            //공백 사이즈 : 1
            //폼 사이트 : 리스트 (320) + 공백 (1) + 폼여백(10)
            //폼 기본 사이즈 시작 : 498
            //sub가 증가시 마다 : 488 + 개당 * 321 + 10

            int 현재폼사이즈 = this.Size.Width;

            int 기본사이즈 = 488;
            int 리스트사이즈 = 391;
            int 폼여백사이즈 = 10;
            int 갯수 = lbxClients.Items.Count;
            int 세로 = 465;
            int 가로 = 0;
            if (갯수 == 0)
            {
                가로 = 기본사이즈 + 폼여백사이즈;
            } else
            {
                가로 = 기본사이즈 + ID * 리스트사이즈 + 폼여백사이즈;
                가로 = 가로 > 현재폼사이즈 ? 가로 : 현재폼사이즈;
            }
            this.Size = new System.Drawing.Size(가로, 세로);
        }

        private void lbJobs_Display()
        {
            if (lbJobs.Items.Count > 0)
            {
                lab작업중.ForeColor = Color.Yellow;
                lab작업중.Font = new Font(lab작업중.Font, FontStyle.Bold);
                lab작업중.BackColor = Color.Black;
            }
            else
            {
                lab작업중.ForeColor = System.Drawing.Color.Black;
                lab작업중.Font = new Font(lab작업중.Font, FontStyle.Regular);
                lab작업중.BackColor = Color.Transparent;
            }
        }

        private async void BtnStart_Click(object sender, EventArgs e)
        {
            _ =  await TCPServer_Client초기화(실행주체, "시작", 설정);
            
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            this.Close();
            //SetError(true, 0);
        }

        private void RunningStateChanged(bool isRunning)
        {
            BtnStart.Enabled = !isRunning;
            BtnStop.Enabled = isRunning;
        }
        

        // =============================================================================
        //  TCP/IP (Client)
        // =============================================================================

        private void C_Connected(object sender, ChatEventArgs e)
        {
            _clientHandler = e.ClientHandler;
        }

        private async void C_Disconnected(object sender, ChatEventArgs e)
        {
            _clientHandler = null;
            로그내용 = $"{DateTime.Now:yy.MM.dd HH:mm:ss}[연결끊김] 서버와 연결이 끊겼습니다.";
            로그기록(로그내용,1);

            await Task.Delay(TimeSpan.FromSeconds(3));
            _ = await TCPServer_Client초기화(실행주체, "시작", 설정);
        }

        private void C_Received(object sender, ChatEventArgs e)
        {
            //클라이언트에서 서버로 파일을 송신하면 서버쪽에서 파일을 다 수신 후 클라이언트쪽으로 보내는 상태값
            if (e.Hub.State == ChatState.Ack)
            {
                this.Invoke(new Action(() =>
                {
                    // ProgressBar를 100%로 표시
                    progressBar.EditValue = 100;
                }));
            }
            AddClientMessageList(e.Hub);
        }

        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            await Task.CompletedTask;
        }
    }





    // ========================================================================================================================================================

    public class PreventSleep
    {
        // 윈도우 API 불러오기
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint SetThreadExecutionState(uint esFlags);

        // 플래그 상수 정의
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001; // 시스템 잠김 방지
        private const uint ES_DISPLAY_REQUIRED = 0x00000002; // 화면 꺼짐 방지

        /// <summary>
        /// 절전 모드 및 화면 보호기 실행을 막습니다. (작업 시작 시 호출)
        /// </summary>
        public static void DonSleep()
        {
            // 시스템이 잠기거나 화면이 꺼지는 것을 막음
            // HWP 같은 GUI 자동화는 화면이 꺼지면 에러가 날 수 있으므로 Display도 켜둬야 함
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
        }

        /// <summary>
        /// 다시 절전 모드가 가능하도록 설정을 해제합니다. (프로그램 종료 시 호출)
        /// </summary>
        public static void AllowSleep()
        {
            SetThreadExecutionState(ES_CONTINUOUS);
        }
    }

    public class 세션파일
    {
        public string PK { get; set; }
        public string 경로 { get; set; }
    }
    public class 저장
    {
        public static string 약관DB;
        public static string DB설정파일path;
        public static string 기본입력폴더;
        public static string 기본출력폴더;
        public static string 기본파일폴더;
        public static string 사용컴퓨터;
        public static string 로그경로;
        public static string 출력폴더;

    }
    public class 현재상태
    {
        public string 처리Step { get; set; } = string.Empty;
        public string 상태컴퓨터 { get; set; } = string.Empty;
        public string dataTable접속 { get; set; } = string.Empty;
        public int 병렬컴 { get; set; }
        public string 처리구분 { get; set; } = string.Empty;
        public string groupKey { get; set; } = string.Empty;
        public string 고객사 { get; set; } = string.Empty;
        public int 앱실행수 { get; set; }
        public string 출력폴더 { get; set; } = string.Empty;
        public string 입력폴더 { get; set; } = string.Empty;
        public bool 병렬처리여부 { get; set; }
        public string 접속IP { get; set; } = string.Empty;
        public int  접속PORT { get; set; }

        public 현재상태() { }

        public 현재상태(환경설정 설정)
        {
            this.병렬처리여부 = 설정.병렬처리여부;
            this.상태컴퓨터 = 설정.상태컴퓨터;
            this.처리구분 = 설정.처리구분;
            this.dataTable접속 = 설정.dataTable접속;
            this.접속IP = 설정.접속IP;
            this.접속PORT = 설정.접속PORT;
            
        }
    }
   
    public class 환경설정
    {
        public bool 병렬처리여부 { get; set; }
        public string 상태컴퓨터 { get; set; } = string.Empty;
        public string 처리구분 { get; set; } = string.Empty;
        public string dataTable접속 { get; set; } = string.Empty;
        public string 접속IP { get; set; } = string.Empty;
        public int 접속PORT { get; set; } 


    }

}
