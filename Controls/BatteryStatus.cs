using System;
using System.Drawing;      // <-- Color, Point, Font, Size를 쓰기 위해 필수!
using System.Windows.Forms; // <-- Label, Timer, Form을 쓰기 위해 필수!
using MissionPlanner;      // <-- MainV2 데이터를 가져오기 위해 필수!

namespace MissionPlanner.Controls
{
    public partial class BatteryStatus : Form
    {
        Label lblVolt = new Label();
        Label lblCurrent = new Label();
        Label lblSpeed = new Label();
        // 1. 데이터를 갱신할 타이머 선언
        Timer updateTimer = new Timer();

        public BatteryStatus()
        {
            InitializeComponent();
            SetupLabels();
            // 2. 타이머 설정 (0.5초마다 갱신)
            updateTimer.Interval = 500;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            // 창이 닫힐 때 타이머도 멈추도록 설정
            this.FormClosing += (s, e) => updateTimer.Stop();
        }
        private void SetupLabels()
        {
            // 배경을 검정으로, 글자를 형광색으로 하면 HUD 느낌이 납니다.
            this.BackColor = Color.Black;

            // 전압 라벨 설정
            lblVolt.Location = new Point(20, 20);
            lblVolt.Size = new Size(250, 40);
            lblVolt.Font = new Font("Consolas", 16, FontStyle.Bold);
            lblVolt.ForeColor = Color.Lime;
            this.Controls.Add(lblVolt); // 창에 추가

            // 전류 라벨 설정
            lblCurrent.Location = new Point(20, 70);
            lblCurrent.Size = new Size(250, 40);
            lblCurrent.Font = new Font("Consolas", 16, FontStyle.Bold);
            lblCurrent.ForeColor = Color.Lime;
            this.Controls.Add(lblCurrent);

            // 속도 라벨 설정
            lblSpeed.Location = new Point(20, 120);
            lblSpeed.Size = new Size(300, 40);
            lblSpeed.Font = new Font("Consolas", 16, FontStyle.Bold);
            lblSpeed.ForeColor = Color.Aqua; // 속도는 하늘색으로 구분
            this.Controls.Add(lblSpeed);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            var cs = MainV2.comPort.MAV.cs;
            if (cs == null) return;

            // 데이터 가져오기
            double volt = cs.battery_voltage;
            double current = cs.current;
            int remaining = cs.battery_remaining;
            float aspeed = cs.airspeed;
            float gspeed = cs.groundspeed;

            // 창 안의 라벨들에 값 넣기 (\n은 줄바꿈입니다)
            lblVolt.Text = $"Voltage: {volt:0.00} V";
            lblCurrent.Text = $"Current: {current:0.0} A ({remaining}%)";
            lblSpeed.Text = $"AS: {aspeed:0.0} | GS: {gspeed:0.0} m/s";
            this.Text = "갱신 중: " + DateTime.Now.ToString("HH:mm:ss");
        }
    }
}